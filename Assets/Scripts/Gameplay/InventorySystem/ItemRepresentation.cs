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
   }
}
