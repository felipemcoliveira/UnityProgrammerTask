using UnityEngine;

namespace UnityProgrammerTask.Gameplay
{
   public class Item : ScriptableObject
   {
      public GUID ID => m_ID;

      public virtual bool IsConsumable => false;

      public int MaxStackSize => m_MaxStackSize;

      public int UnitPriceInGold => m_UnitPriceInGold;

      public string ItemName => m_ItemName;

      public Sprite ItemIcon => m_ItemIcon;

      [SerializeField]
      private GUID m_ID;

      [SerializeField]
      private string m_ItemName;

      [SerializeField]
      private Sprite m_ItemIcon;

      [SerializeField]
      private int m_UnitPriceInGold;

      [SerializeField]
      private int m_MaxStackSize = 1;

      [SerializeField, Multiline(4)]
      private string m_StaticDescription;

      [SerializeField]
      private ItemRepresentation m_WorldRepresentationPrefab;

      private void OnValidate()
      {
#if UNITY_EDITOR

         if (UnityEditor.EditorUtility.IsPersistent(this) && m_ID == GUID.Empty)
            m_ID = GUID.Generate();

#endif
      }

      public virtual void OnItemAddedToInventory(Character character, int quantity)
      {
         // Default implementation does nothing
      }

      public virtual void OnItemRemovedFromInventory(Character character, int quantity)
      {
         // Default implementation does nothing
      }

      public virtual void OnItemConsumed(Character character, int quantity)
      {
         // Default implementation does nothing
      }

      public virtual bool TryConsume(Character character, ref int quantity)
      {
         return true;
      }

      public virtual string GetDescription()
      {
         return GetStaticDescription();
      }

      protected string GetStaticDescription()
      {
         return m_StaticDescription;
      }

      public void CreateWorldRepresentation(Vector3 position, int quantity)
      {
         if (m_WorldRepresentationPrefab != null)
         {
            ItemRepresentation representation = Instantiate(m_WorldRepresentationPrefab);
            representation.transform.position = position;
            representation.Initialize(this, quantity);
         }
      }
   }
}
