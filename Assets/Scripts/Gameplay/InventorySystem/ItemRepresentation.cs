using UnityEngine;

namespace UnityProgrammerTask.Gameplay
{
   public class ItemRepresentation : MonoBehaviour
   {
      public Item Item => m_Item;
      public int Quantity => m_Quantity;

      private Item m_Item;
      private int m_Quantity;

      public void Initialize(Item item, int quantity)
      {
         m_Item = item;
         m_Quantity = quantity;
      }

      private void OnTriggerEnter(Collider other)
      {
         if (other.TryGetComponent(out Character character))
         {
            m_Quantity -= character.Inventory.AddItem(m_Item, m_Quantity);

            if (m_Quantity == 0)
               Destroy(gameObject);
         }
      }
   }
}
