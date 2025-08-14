using UnityEngine;
using UnityEngine.EventSystems;
using UnityProgrammerTask.Gameplay;

namespace UnityProgrammerTask.Presentation
{
   public class ItemSlot : UIBehaviour, IItemDropHandler
   {
      private Vector2Int m_Position;
      private Inventory m_Inventory;

      public void Initialize(Vector2Int position, Inventory inventory)
      {
         m_Position = position;
         m_Inventory = inventory;
      }

      public void OnItemDropped(ItemInInventory item)
      {
         if (item.Inventory != m_Inventory)
         {
            Debug.LogError($"Item {item.ItemDefinition.ItemName} does not belong to this inventory.");
            return;
         }

         if (item.Position == m_Position)
            return;

         if (m_Inventory.TryMoveItem(item.Position, m_Position, item.Quantity))
            return;

         if (m_Inventory.TryCombineItems(item.Position, m_Position))
            return;

         m_Inventory.SwapItems(item.Position, m_Position);
      }

      public void OnItemDropped(EquippedItem item)
      {
         item.CharacterEquipment.Unequip(item.Item, ReturningPolicy.ReturnToInventory);
      }
   }
}
