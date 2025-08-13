using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Pool;

namespace UnityProgrammerTask.Core
{
   public class SaveableGameObject : MonoBehaviour, IGameSaveSection
   {
      public GUID Id => m_Id;
      public string AddressableName => m_AddressableName;

      private static readonly Dictionary<GUID, SaveableGameObject> s_ActiveSaveableGameObjects = new();

      [SerializeField]
      private string m_AddressableName;

      [SerializeField, HideInInspector]
      private GUID m_Id;

      [SerializeField, HideInInspector]
      private bool m_IsSceneObject;

      [SerializeReference, SubclassSelector]
      private ISaveableComponentHandler[] m_SaveableComponentHandlers;

      private void Awake()
      {
         if (!SavedDynamicGameObjects.IsLoading && !m_IsSceneObject)
            m_Id = SavedDynamicGameObjects.AddSaveableGameObject(this);
         else
            SaveSystem.ActiveSave.RegisterSection(m_Id, this);

         s_ActiveSaveableGameObjects.Add(m_Id, this);
      }

      private void OnDestroy()
      {
         if (!m_IsSceneObject)
            SavedDynamicGameObjects.RemoveSaveableGameObject(this);
         else
            SaveSystem.ActiveSave.UnregisterSection(m_Id);


         s_ActiveSaveableGameObjects.Remove(m_Id);
      }

      private void OnValidate()
      {
#if UNITY_EDITOR
         // check if is a scene object, if so, generate a persisit GUID
         m_IsSceneObject = UnityEditor.EditorUtility.IsPersistent(gameObject)
            && gameObject.scene.IsValid() && gameObject.scene.isLoaded;

         if (m_IsSceneObject && m_Id == GUID.Empty)
         {
            m_Id = GUID.Generate();
            m_IsSceneObject = true;
         }
#endif
      }

      public static SaveableGameObject GetSaveableGameObject(GUID id)
      {
         if (s_ActiveSaveableGameObjects.TryGetValue(id, out SaveableGameObject saveable))
            return saveable;

         Debug.LogWarning($"SaveableGameObject with ID {id} not found.");
         return null;
      }

      public void Load(BinaryReader stream, int size)
      {
         foreach (ISaveableComponentHandler handler in m_SaveableComponentHandlers)
            handler.Load(gameObject, stream);
      }

      public void Save(BinaryWriter stream)
      {
         foreach (ISaveableComponentHandler handler in m_SaveableComponentHandlers)
            handler.Save(gameObject, stream);
      }

      public void GetDependencies(List<GUID> dependencies)
      {
         foreach (ISaveableComponentHandler handler in m_SaveableComponentHandlers)
         {
            using (ListPool<SaveableGameObject>.Get(out List<SaveableGameObject> saveableDependencies))
            {
               handler.GetDependencies(gameObject, saveableDependencies);
               foreach (SaveableGameObject dep in saveableDependencies)
               {
                  if (dep != null && dep.Id != GUID.Empty)
                     dependencies.Add(dep.Id);
               }
            }
         }
      }
   }
}