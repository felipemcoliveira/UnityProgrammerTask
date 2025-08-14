using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UnityProgrammerTask.Gameplay
{
   [CreateAssetMenu]
   public class ItemLibrary : ScriptableObject, IReadOnlyList<Item>
   {
      public static ItemLibrary Instance
      {
         get
         {
            if (s_Instance == null)
            {
               s_Instance = Addressables.LoadAssetAsync<ItemLibrary>("ItemLibrary")
                  .WaitForCompletion();
            }
            return s_Instance;
         }
      }

      public int Count => m_ItemDefinitions.Count;

      public Item this[int index] => m_ItemDefinitions[index];

      private static ItemLibrary s_Instance;

      [SerializeField]
      private List<Item> m_ItemDefinitions;

      private Dictionary<GUID, Item> m_ItemLookup;

      private void OnValidate()
      {
         m_ItemLookup = null;
      }

      public Item GetItemByID(GUID id)
      {
         InitializeItemLookup();
         if (!m_ItemLookup.TryGetValue(id, out Item item))
         {
            Debug.LogWarning($"Item with ID {id} not found.");
            return null;
         }

         return item;
      }

      private void InitializeItemLookup()
      {
         if (m_ItemLookup != null)
            return;

         m_ItemLookup = new Dictionary<GUID, Item>();
         foreach (Item item in m_ItemDefinitions)
         {
            if (m_ItemLookup.ContainsKey(item.ID))
            {
               Debug.LogWarning($"Duplicate item ID found: {item.ID}");
               continue;
            }

            m_ItemLookup[item.ID] = item;
         }
      }

      public IEnumerator<Item> GetEnumerator()
      {
         return m_ItemDefinitions.GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         return GetEnumerator();
      }
   }
}
