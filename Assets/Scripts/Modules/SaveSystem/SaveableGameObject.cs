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

      [SerializeField]
      private GUID m_Id;

      [SerializeField]
      private bool m_IsPersistent;

      [SerializeReference, SubclassSelector]
      private ISaveableComponentHandler[] m_SaveableComponentHandlers;

      private void Awake()
      {
         if (SavedDynamicGameObjects.IsLoading && !m_IsPersistent)
         {
            m_Id = SavedDynamicGameObjects.AddSaveableGameObject(this);
            return;
         }

         SaveSystem.ActiveSave.RegisterSection(m_Id, this);

         s_ActiveSaveableGameObjects.Add(m_Id, this);
      }

      private void OnDestroy()
      {
         if (!m_IsPersistent)
            SavedDynamicGameObjects.RemoveSaveableGameObject(this);

         SaveSystem.ActiveSave.UnregisterSection(m_Id);

         s_ActiveSaveableGameObjects.Remove(m_Id);
      }

      private void OnValidate()
      {
         // check if is a scene object, if so, generate a persisit GUID
#if UNITY_EDITOR
         m_IsPersistent = UnityEditor.EditorUtility.IsPersistent(gameObject);
         if (m_IsPersistent && m_Id == GUID.Empty)
         {
            m_Id = GUID.Generate();
            m_IsPersistent = true;
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
      }

      public void Save(BinaryWriter stream)
      {
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