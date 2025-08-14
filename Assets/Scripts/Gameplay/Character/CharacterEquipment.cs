using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace UnityProgrammerTask.Gameplay
{
   public enum ReturningPolicy
   {
      None,
      ReturnToInventory,
      ReturnToInventoryOrDrop,
      Drop
   }

   [RequireComponent(typeof(Character))]
   public class CharacterEquipment : MonoBehaviour, IEnumerable<Equipment>
   {
      public Action<CharacterEquipment> EquipmentChanged;

      private Dictionary<CharacterEquipmentSlot, Equipment> m_EquipmentSlots = new();
      private Character m_Character;

      private void Awake()
      {
         m_Character = GetComponent<Character>();
      }

      public bool IsSlotEquipped(CharacterEquipmentSlot slot)
      {
         return m_EquipmentSlots.ContainsKey(slot);
      }

      public bool TryGetEquippedItem(CharacterEquipmentSlot slot, out Equipment equipment)
      {
         return m_EquipmentSlots.TryGetValue(slot, out equipment);
      }

      public bool EquipItem(ItemInInventory item)
      {
         if (item.ItemDefinition is not Equipment equipmentItem)
            return false;

         item.Inventory.RemoveItem(item.Position, 1);
         return Equip(equipmentItem, ReturningPolicy.ReturnToInventoryOrDrop);
      }

      public bool Equip(Equipment equipment, ReturningPolicy previousReturningPolicy = ReturningPolicy.ReturnToInventory)
      {
         if (m_EquipmentSlots.TryGetValue(equipment.Slot, out Equipment existingEquipment))
            Unequip(existingEquipment, previousReturningPolicy);

         if (IsSlotEquipped(equipment.Slot))
         {
            // Failed to unequip existing equipment, cannot equip new one.
            return false;
         }

         Assert.IsNotNull(equipment, "Equipment cannot be null.");
         m_EquipmentSlots[equipment.Slot] = equipment;

         equipment.ApplyEffects(m_Character);
         EquipmentChanged?.Invoke(this);

         return true;
      }

      public bool Unequip(Equipment equipment, ReturningPolicy returningPolicy, Vector3? dropWorldPosition = default)
      {
         Assert.IsNotNull(equipment, "Equipment cannot be null.");

         if (!TryGetEquippedItem(equipment.Slot, out Equipment equipped))
            return false;

         equipped.RemoveEffects(m_Character);
         m_EquipmentSlots.Remove(equipment.Slot);
         EquipmentChanged?.Invoke(this);

         switch (returningPolicy)
         {
            case ReturningPolicy.None:
               return true;

            case ReturningPolicy.ReturnToInventory:
               return m_Character.Inventory.AddItem(equipment, 1) == 1;

            case ReturningPolicy.ReturnToInventoryOrDrop:
               if (m_Character.Inventory.AddItem(equipment, 1) == 1)
                  return true;
               equipment.CreateWorldRepresentation(dropWorldPosition ?? transform.position, 1);
               return true;

            case ReturningPolicy.Drop:
               equipment.CreateWorldRepresentation(dropWorldPosition ?? transform.position, 1);
               return true;

            default:
               throw new NotImplementedException($"UnequipPolicy {returningPolicy} is not implemented.");
         }
      }

      public IEnumerator<Equipment> GetEnumerator()
      {
         foreach (Equipment equipment in m_EquipmentSlots.Values)
            yield return equipment;
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         return GetEnumerator();
      }

      [ContextMenu("Unequip All")]
      private void UnequipAllContexMenu()
      {
         List<Equipment> equipmentList = new(m_EquipmentSlots.Values);
         foreach (Equipment equipment in equipmentList)
            Unequip(equipment, ReturningPolicy.ReturnToInventoryOrDrop);
      }
   }
}