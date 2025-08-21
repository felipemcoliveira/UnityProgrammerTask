using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace UnityProgrammerTask.Gameplay
{
   public struct ItemInInventory
   {
      public Inventory Inventory { get; }
      public Item ItemDefinition { get; }
      public int Quantity { get; }
      public Vector2Int Position { get; }

      public ItemInInventory(Inventory inventory, Item item, int quantity, Vector2Int position)
      {
         Inventory = inventory;
         ItemDefinition = item;
         Quantity = quantity;
         Position = position;
      }

      public void Drop(Vector3 placeWorldPosition)
      {
         Inventory.DropItem(Position, Quantity, placeWorldPosition);
      }
   }

   [HideMonoScript]
   [RequireComponent(typeof(Character))]
   public class Inventory : MonoBehaviour, IEnumerable<ItemInInventory>
   {
      public struct Slot
      {
         public Item Item;
         public int Quantity;
      }

      public event Action<Inventory> InventoryChanged;

      public Character Character
      {
         get
         {
            if (m_Character == null)
               m_Character = GetComponent<Character>();

            return m_Character;
         }
      }

      public Vector2Int InventoryCapacity => new(5, 5);

      private Character m_Character;
      private Slot[,] m_Slots = new Slot[7, 7];
      private int m_LockBroadcastChangeScopeCount;

      public int AddItem(Item item, int quantity)
      {
         using (CreateLockBroadcastChangeScope())
         {
            int remaining = quantity;

            if (item.MaxStackSize > 1)
            {
               int added = TryStackItem(item, quantity);

               remaining -= added;

               if (remaining == 0)
                  return added;
            }

            while (remaining > 0 && TryGetEmptySlot(out Vector2Int emptySlotPosition))
            {
               int toAdd = Mathf.Min(remaining, item.MaxStackSize);
               AddItem(emptySlotPosition, item, toAdd);
               remaining -= toAdd;
            }

            return quantity - remaining;
         }
      }

      public bool HasEmptySlot()
      {
         Vector2Int emptySlotPosition = GetEmptySlotPosition();
         return IsValidSlotPosition(emptySlotPosition);
      }

      public bool TryCombineItems(Vector2Int from, Vector2Int to)
      {
         ValidateSlotPosition(from);
         ValidateSlotPosition(to);

         ref Slot fromSlot = ref m_Slots[from.x, from.y];
         ref Slot toSlot = ref m_Slots[to.x, to.y];

         if (fromSlot.Item == null || toSlot.Item == null)
            return false;

         if (fromSlot.Item != toSlot.Item)
            return false;

         int spaceLeft = toSlot.Item.MaxStackSize - toSlot.Quantity;
         if (spaceLeft <= 0)
            return false;

         int toAdd = Mathf.Min(spaceLeft, fromSlot.Quantity);

         toSlot.Quantity += toAdd;
         fromSlot.Quantity -= toAdd;

         if (fromSlot.Quantity == 0)
            fromSlot.Item = null;

         BroadcastChanged();
         return true;
      }

      private int TryStackItem(Item item, int quantity)
      {
         using (CreateLockBroadcastChangeScope())
         {
            Vector2Int itPosition = Vector2Int.zero;
            int added = 0;

            while (GetFirstStackableSlotFor(item, itPosition, out Vector2Int stackableSlotPosition))
            {
               added += AddItem(stackableSlotPosition, item, quantity);

               if (added == quantity)
                  break;

               itPosition = stackableSlotPosition;
            }

            return added;
         }
      }

      public int AddItem(Vector2Int position, Item item, int quantity)
      {
         ValidateSlotPosition(position);

         Assert.IsNotNull(item, "Item cannot be null.");
         Assert.IsTrue(quantity > 0, "Quantity must be greater than zero.");

         ref Slot slot = ref m_Slots[position.x, position.y];

         if (slot.Item == null)
         {
            slot.Quantity = Mathf.Min(quantity, item.MaxStackSize);
            slot.Item = item;

            item.OnItemAddedToInventory(Character, quantity);

            BroadcastChanged();

            return slot.Quantity;
         }

         if (slot.Item != item)
            return 0;

         int spaceLeft = item.MaxStackSize - slot.Quantity;
         int toAdd = Mathf.Min(spaceLeft, quantity);

         slot.Quantity += toAdd;

         item.OnItemAddedToInventory(Character, toAdd);

         BroadcastChanged();

         return toAdd;
      }

      public void DropItem(Vector2Int position, int quantity, Vector3 placeWorldPosition)
      {
         ValidateSlotPosition(position);

         ref Slot slot = ref m_Slots[position.x, position.y];
         Item item = slot.Item;

         int dropped = RemoveItem(position, quantity);
         if (dropped <= 0)
            return;

         item.CreateWorldRepresentation(placeWorldPosition, dropped);
      }

      public int RemoveItem(Vector2Int position, int quantity)
      {
         ValidateSlotPosition(position);

         ref Slot slot = ref m_Slots[position.x, position.y];
         if (slot.Item == null || slot.Quantity < quantity)
            return 0;

         int removed = Mathf.Min(quantity, slot.Quantity);
         slot.Quantity -= removed;

         if (removed > 0)
            slot.Item.OnItemRemovedFromInventory(Character, removed);

         if (slot.Quantity <= 0)
            slot.Item = null;

         BroadcastChanged();

         return removed;
      }

      public bool GetFirstStackableSlotFor(Item item, Vector2Int from, out Vector2Int position)
      {
         ValidateSlotPosition(from);
         Assert.IsNotNull(item, "Item cannot be null.");

         for (int x = 0; x < InventoryCapacity.x; x++)
         {
            for (int y = from.y; y < InventoryCapacity.y; y++)
            {
               Slot slot = m_Slots[x, y];
               if (slot.Item == item && slot.Quantity < item.MaxStackSize)
               {
                  position = new Vector2Int(x, y);
                  return true;
               }
            }
         }

         position = new Vector2Int(-1, -1);
         return false;
      }

      public bool ConsumeItem(Vector2Int position)
      {
         ValidateSlotPosition(position);

         ref Slot slot = ref m_Slots[position.x, position.y];
         if (slot.Item == null)
            return false;

         if (!slot.Item.IsConsumable)
            return false;

         int originalQuantity = slot.Quantity;
         if (!slot.Item.TryConsume(Character, ref slot.Quantity))
            return false;

         Assert.IsFalse(slot.Quantity >= originalQuantity, "Item consumption should reduce quantity.");

         int removed = originalQuantity - slot.Quantity;
         Item removedItem = slot.Item;

         if (slot.Quantity == 0)
            slot.Item = null;

         if (removed > 0)
         {
            removedItem.OnItemRemovedFromInventory(Character, slot.Quantity - originalQuantity);
            removedItem.OnItemConsumed(Character, removed);
         }

         BroadcastChanged();
         return true;
      }

      public void SwapItems(Vector2Int from, Vector2Int to)
      {
         ValidateSlotPosition(from);
         ValidateSlotPosition(to);

         ref Slot fromSlot = ref m_Slots[from.x, from.y];
         ref Slot toSlot = ref m_Slots[to.x, to.y];

         if (fromSlot.Item == null && toSlot.Item == null)
            return;

         (fromSlot, toSlot) = (toSlot, fromSlot);

         BroadcastChanged();
      }

      public bool TryMoveItem(Vector2Int from, Vector2Int to, int quantity)
      {
         ValidateSlotPosition(from);
         ValidateSlotPosition(to);

         ref Slot fromSlot = ref m_Slots[from.x, from.y];
         ref Slot toSlot = ref m_Slots[to.x, to.y];

         if (fromSlot.Item == null)
            return false;

         if (fromSlot.Quantity < quantity)
            return false;

         if (toSlot.Item == null)
         {
            toSlot.Item = fromSlot.Item;
            toSlot.Quantity = quantity;

            fromSlot.Quantity -= quantity;
            if (fromSlot.Quantity == 0)
               fromSlot.Item = null;

            BroadcastChanged();
            return true;
         }

         if (toSlot.Item != fromSlot.Item)
            return false;

         int spaceLeft = toSlot.Item.MaxStackSize - toSlot.Quantity;
         if (spaceLeft <= 0)
            return false;

         int toAdd = Mathf.Min(spaceLeft, quantity);
         toSlot.Quantity += toAdd;
         fromSlot.Quantity -= toAdd;

         if (fromSlot.Quantity == 0)
            fromSlot.Item = null;

         BroadcastChanged();
         return true;
      }

      public bool TryGetEmptySlot(out Vector2Int position)
      {
         position = GetEmptySlotPosition();
         return IsValidSlotPosition(position);
      }

      public Vector2Int GetEmptySlotPosition()
      {
         for (int x = 0; x < InventoryCapacity.x; x++)
         {
            for (int y = 0; y < InventoryCapacity.y; y++)
            {
               if (m_Slots[x, y].Item == null)
                  return new Vector2Int(x, y);
            }
         }

         return new Vector2Int(-1, -1);
      }

      private void BroadcastChanged()
      {
         if (m_LockBroadcastChangeScopeCount > 0)
            return;

         InventoryChanged?.Invoke(this);
      }

      public void LockBroadcastChange()
      {
         m_LockBroadcastChangeScopeCount++;
      }

      public void UnlockBroadcastChange()
      {
         if (m_LockBroadcastChangeScopeCount > 0)
         {
            m_LockBroadcastChangeScopeCount--;

            if (m_LockBroadcastChangeScopeCount == 0)
               BroadcastChanged();
         }
      }

      public bool IsValidSlotPosition(Vector2Int position)
      {
         return position.x >= 0 && position.x < InventoryCapacity.x &&
                position.y >= 0 && position.y < InventoryCapacity.y;
      }

      private void ValidateSlotPosition(Vector2Int position)
      {
         if (!IsValidSlotPosition(position))
            throw new System.ArgumentOutOfRangeException(nameof(position), "Position is out of inventory bounds.");
      }

      public IEnumerator<ItemInInventory> GetEnumerator()
      {
         for (int x = 0; x < InventoryCapacity.x; x++)
         {
            for (int y = 0; y < InventoryCapacity.y; y++)
            {
               Slot slot = m_Slots[x, y];
               if (slot.Item != null)
                  yield return new ItemInInventory(this, slot.Item, slot.Quantity, new Vector2Int(x, y));
            }
         }
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         return GetEnumerator();
      }

      private LockBroadcastChangeScope CreateLockBroadcastChangeScope()
      {
         return new LockBroadcastChangeScope(this);
      }

      private struct LockBroadcastChangeScope : IDisposable
      {
         private Inventory m_Inventory;
         public LockBroadcastChangeScope(Inventory inventory)
         {
            m_Inventory = inventory;
            m_Inventory.LockBroadcastChange();
         }
         public void Dispose()
         {
            m_Inventory.UnlockBroadcastChange();
         }
      }
   }
}