using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace UnityProgrammerTask.Core
{
   public class SavedDynamicGameObjects : IGameSaveSection
   {
      public static bool IsLoading { get; private set; }

      private static Dictionary<GUID, SaveableGameObject> s_SaveableGameObjects = new();
      private static Dictionary<SaveableGameObject, GUID> s_SaveableGameObjectGUIDs = new();

      public static GUID AddSaveableGameObject(SaveableGameObject saveable)
      {
         Assert.IsNotNull(saveable, "SaveableGameObject cannot be null.");

         GUID guid = GUID.Generate();
         s_SaveableGameObjects.Add(guid, saveable);
         s_SaveableGameObjectGUIDs.Add(saveable, guid);

         return guid;
      }

      public static void RemoveSaveableGameObject(SaveableGameObject saveable)
      {
         Assert.IsNotNull(saveable, "SaveableGameObject cannot be null.");

         if (!s_SaveableGameObjectGUIDs.TryGetValue(saveable, out GUID guid))
            Debug.LogWarning($"SaveableGameObject {saveable.name} not found in the saved dynamic game objects.");

         s_SaveableGameObjects.Remove(guid);
         s_SaveableGameObjectGUIDs.Remove(saveable);
      }

      public void Load(BinaryReader stream, int size)
      {
         IsLoading = true;

         List<string> addressablesTable = new();
         int addressablesCount = stream.ReadInt32();

         for (int i = 0; i < addressablesCount; i++)
         {
            int byteCount = stream.ReadInt16();
            Span<byte> bytes = stackalloc byte[byteCount];
            stream.Read(bytes);

            string addressableName = Encoding.UTF8.GetString(bytes);
            addressablesTable.Add(addressableName);
         }

         int count = stream.ReadInt32();
         for (int i = 0; i < count; i++)
         {
            GUID saveGameObjectSectionId = GUID.Read(stream);
            int addressableIndex = stream.ReadInt32();
            int sceneBuildIndex = stream.ReadInt16();

            string addressableName = addressablesTable[addressableIndex];

            GameObject go = Addressables.LoadAssetAsync<GameObject>(addressableName).WaitForCompletion();
            SaveableGameObject saveable = go.GetComponent<SaveableGameObject>();

            Scene scene = SceneManager.GetSceneByBuildIndex(sceneBuildIndex);
            if (go.scene != scene)
               SceneManager.MoveGameObjectToScene(go, scene);

            s_SaveableGameObjects.Add(saveGameObjectSectionId, saveable);

            SaveSystem.ActiveSave.RegisterSection(saveGameObjectSectionId, saveable);
         }

         IsLoading = false;
      }

      public void Save(BinaryWriter stream)
      {
         List<string> addressablesTable = new();

         foreach (SaveableGameObject saveable in s_SaveableGameObjects.Values)
         {
            if (saveable == null || string.IsNullOrEmpty(saveable.AddressableName))
               continue;

            if (!addressablesTable.Contains(saveable.AddressableName))
               addressablesTable.Add(saveable.AddressableName);
         }

         stream.Write(addressablesTable.Count);
         foreach (string addressableName in addressablesTable)
         {
            int byteCount = Encoding.UTF8.GetByteCount(addressableName);
            Span<byte> bytes = stackalloc byte[byteCount];

            stream.Write((short)byteCount);
            stream.Write(bytes);
            stream.Write(addressableName);
         }

         List<GUID> saveableIds = new(s_SaveableGameObjects.Keys);

         // sort the IDs to ensure that dependencies are handled correctly
         SaveSystem.ActiveSave.SortSectionIDs(saveableIds);

         foreach (GUID guid in saveableIds)
         {
            SaveableGameObject saveable = s_SaveableGameObjects[guid];

            if (saveable == null)
               continue;

            int addressableIndex = addressablesTable.IndexOf(saveable.AddressableName);
            int sceneBuildIndex = saveable.gameObject.scene.buildIndex;

            guid.Write(stream);
            stream.Write(addressableIndex);
            stream.Write(saveable.AddressableName);
            stream.Write((short)sceneBuildIndex);
         }
      }
   }
}