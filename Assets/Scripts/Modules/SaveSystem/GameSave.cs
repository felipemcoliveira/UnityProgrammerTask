using System;
using System.Collections.Generic;
using System.IO;
using UnityProgrammerTask.Core;

namespace UnityProgrammerTask
{
   public class GameSave : IDisposable
   {
      public struct SectionHeader
      {
         public GUID SectionID;
         public int SectionSize;
         public int PositionInFile;
      }

      private static readonly GUID k_SavedDynamicGameObjectsSectionId = new("7CD302D5-37DF-4B69-B9B1-63D8B81C314D");
      private static readonly GUID k_SaveScenesSectionId = new("CE98AB3D-0415-4849-865F-C7895EA9968E");

      private Dictionary<GUID, IGameSaveSection> m_Sections = new();
      private Dictionary<GUID, int> m_SectionHeaderLookUp = new();
      private BinaryReader m_Reader;
      private FileStream m_FileStream;
      private bool m_Disposed;

      private List<SectionHeader> m_SectionHeaders = new();

      public void Initialize()
      {
         RegisterGlobalSections();
      }

      public void Initialize(FileStream fileStream)
      {
         m_FileStream = fileStream;
         m_Reader = new BinaryReader(fileStream);

         ReadSectionHeaders(m_Reader);
         RegisterGlobalSections();
      }

      private void RegisterGlobalSections()
      {
         RegisterSection(k_SaveScenesSectionId, new SavedScenes());
         RegisterSection(k_SavedDynamicGameObjectsSectionId, new SavedDynamicGameObjects());
      }

      ~GameSave()
      {
         Dispose(false);
      }

      public void RegisterSection(GUID sectionId, IGameSaveSection section)
      {
         if (m_Sections.ContainsKey(sectionId))
         {
            // already registered
            return;
         }

         if (m_SectionHeaderLookUp.TryGetValue(sectionId, out int sectionIndex))
         {
            SectionHeader header = m_SectionHeaders[sectionIndex];

            m_Reader.BaseStream.Seek(header.PositionInFile, SeekOrigin.Begin);
            section.Load(m_Reader, header.SectionSize);
         }

         m_Sections[sectionId] = section;
      }

      public void UnregisterSection(GUID sectionId)
      {
         if (!m_Sections.ContainsKey(sectionId))
            throw new KeyNotFoundException($"Section with ID {sectionId} not found.");

         m_Sections.Remove(sectionId);
      }

      public void Write(BinaryWriter writer)
      {
         if (writer == null)
            throw new ArgumentNullException(nameof(writer), "Writer cannot be null.");

         List<GUID> sectionIdsArray = TopologicalOrder(m_Sections);
         List<IGameSaveSection> sectionsArray = new(sectionIdsArray.Count);

         for (int i = 0; i < sectionIdsArray.Count; i++)
            sectionsArray.Add(m_Sections[sectionIdsArray[i]]);

         writer.Write(sectionIdsArray.Count);

         long headerStart = writer.BaseStream.Position;
         for (int i = 0; i < sectionsArray.Count; i++)
         {
            GUID id = sectionIdsArray[i];

            id.Write(writer);

            // size and position placeholders
            writer.Write(0);
            writer.Write(0);
         }

         m_SectionHeaders.Clear();

         for (int i = 0; i < sectionIdsArray.Count; i++)
         {
            GUID id = sectionIdsArray[i];
            IGameSaveSection section = sectionsArray[i];

            int dataPos = checked((int)writer.BaseStream.Position);
            section.Save(writer);

            int size = checked((int)(writer.BaseStream.Position - dataPos));

            //                                   guid + size + position
            long headerBase = headerStart + i * (GUID.kSizeInBytes + 4 + 4);
            long sizeAndPosOffset = headerBase + GUID.kSizeInBytes;

            // patch size and position
            writer.BaseStream.Seek(sizeAndPosOffset, SeekOrigin.Begin);
            writer.Write(size);
            writer.Write(dataPos);

            writer.BaseStream.Seek(0, SeekOrigin.End);

            m_SectionHeaders.Add(new SectionHeader
            {
               SectionID = id,
               SectionSize = size,
               PositionInFile = dataPos
            });
         }
      }

      public void Dispose()
      {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      protected void Dispose(bool disposing)
      {
         if (m_Disposed)
            return;

         if (disposing)
            FreeFile();

         m_Sections.Clear();
         m_SectionHeaders.Clear();
         m_Disposed = true;
      }

      public void FreeFile()
      {
         m_Reader?.Dispose();
         m_Reader = null;

         m_FileStream?.Dispose();
         m_FileStream = null;
      }

      private void ReadSectionHeaders(BinaryReader stream)
      {
         int sectionCount = stream.ReadInt32();

         for (int i = 0; i < sectionCount; i++)
         {
            m_SectionHeaders.Add(new()
            {
               SectionID = GUID.Read(stream),
               SectionSize = stream.ReadInt32(),
               PositionInFile = stream.ReadInt32()
            });

            m_SectionHeaderLookUp.Add(m_SectionHeaders[i].SectionID, i);
         }
      }

      // TODO: document this method
      public void SortSectionIDs(List<GUID> sectionIds)
      {
         sectionIds.Sort((a, b) =>
         {
            if (a == b)
               return 0;

            if (a == GUID.Empty)
               return -1;

            if (b == GUID.Empty)
               return 1;

            int indexA = m_SectionHeaderLookUp.TryGetValue(a, out int indexAValue) ? indexAValue : int.MaxValue;
            int indexB = m_SectionHeaderLookUp.TryGetValue(b, out int indexBValue) ? indexBValue : int.MaxValue;

            return indexA.CompareTo(indexB);
         });
      }

      private enum VisitState : byte
      {
         Visiting,
         Visited
      }

      private static List<GUID> TopologicalOrder(Dictionary<GUID, IGameSaveSection> sections)
      {
         List<GUID> ordered = new(sections.Count);
         Dictionary<GUID, VisitState> state = new(sections.Count);
         List<GUID> deps = new(8);

         void Visit(GUID id)
         {
            if (state.TryGetValue(id, out VisitState s))
            {
               if (s == VisitState.Visiting)
                  throw new InvalidOperationException($"Dependency cycle including {id}.");

               if (s == VisitState.Visited)
                  return;
            }

            state[id] = VisitState.Visiting;

            if (sections.TryGetValue(id, out IGameSaveSection sec))
            {
               deps.Clear();
               sec.GetDependencies(deps);
               for (int i = 0; i < deps.Count; i++)
               {
                  GUID dep = deps[i];
                  if (!sections.ContainsKey(dep))
                     throw new KeyNotFoundException($"Dependency {dep} not found in sections.");

                  Visit(dep);
               }
            }

            state[id] = VisitState.Visited;
            ordered.Add(id);
         }

         foreach (GUID id in sections.Keys)
            Visit(id);

         return ordered;
      }
   }
}