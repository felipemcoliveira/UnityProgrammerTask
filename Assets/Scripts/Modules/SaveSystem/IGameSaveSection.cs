using System.Collections.Generic;
using System.IO;

namespace UnityProgrammerTask
{
   public interface IGameSaveSection
   {
      public void GetDependencies(List<GUID> dependencies)
      {
         // Default implementation does nothing
      }

      public void Load(BinaryReader stream, int size);
      public void Save(BinaryWriter stream);
   }
}