using Sirenix.OdinInspector;
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

   [HideMonoScript]
   [RequireComponent(typeof(Character))]
   public class CharacterEquipment : MonoBehaviour, IEnumerable<Equipment>
   {
      public class EquipmentData
      {
         public Equipment Equipment { get; }
         public List<IEquipmentEffect> AppliedEffects { get; }

         public EquipmentData(Equipment equipment, List<IEquipmentEffect> appliedEffects)
         {
            Equipment = equipment;
            AppliedEffects = appliedEffects;
         }
      }

      public Character Character => m_Character;

      public Action<CharacterEquipment> EquipmentChanged;

      private Dictionary<CharacterEquipmentSlot, EquipmentData> m_Equipments = new();
      private Character m_Character;

      private void Awake()
      {
         m_Character = GetComponent<Character>();
      }

      public bool HasEquipment(CharacterEquipmentSlot slot)
      {
         return m_Equipments.ContainsKey(slot);
      }

      public bool TryGetEquippedItem(CharacterEquipmentSlot slot, out Equipment equipment)
      {
         if (m_Equipments.TryGetValue(slot, out EquipmentData equipmentData))
         {
            equipment = equipmentData.Equipment;
            return true;
         }

         equipment = null;
         return false;
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
         Assert.IsNotNull(equipment, "Equipment cannot be null.");

         if (m_Equipments.TryGetValue(equipment.Slot, out EquipmentData existingEquipmentData))
            Unequip(existingEquipmentData.Equipment.Slot, previousReturningPolicy);

         if (HasEquipment(equipment.Slot))
         {
            // Failed to unequip existing equipment, cannot equip new one.
            return false;
         }

         EquipmentData equipmentData = new(equipment, equipment.CreateEffects());
         m_Equipments.Add(equipment.Slot, equipmentData);

         foreach (IEquipmentEffect effect in equipmentData.AppliedEffects)
            effect.ApplyEffect(m_Character);

         EquipmentChanged?.Invoke(this);

         return true;
      }

      public bool Unequip(CharacterEquipmentSlot slot, ReturningPolicy returningPolicy, Vector3? dropWorldPosition = default)
      {
         if (!m_Equipments.TryGetValue(slot, out EquipmentData equippedData))
            return false;

         foreach (IEquipmentEffect effect in equippedData.AppliedEffects)
            effect.RemoveEffect(m_Character);

         m_Equipments.Remove(slot);
         EquipmentChanged?.Invoke(this);

         Item item = equippedData.Equipment;

         switch (returningPolicy)
         {
            case ReturningPolicy.None:
               return true;

            case ReturningPolicy.ReturnToInventory:
               return m_Character.Inventory.AddItem(item, 1) == 1;

            case ReturningPolicy.ReturnToInventoryOrDrop:
               if (m_Character.Inventory.AddItem(item, 1) == 1)
                  return true;
               item.CreateWorldRepresentation(dropWorldPosition ?? transform.position, 1);
               return true;

            case ReturningPolicy.Drop:
               item.CreateWorldRepresentation(dropWorldPosition ?? transform.position, 1);
               return true;

            default:
               throw new NotImplementedException($"UnequipPolicy {returningPolicy} is not implemented.");
         }
      }

      public IEnumerator<Equipment> GetEnumerator()
      {
         foreach (EquipmentData equipmentData in m_Equipments.Values)
            yield return equipmentData.Equipment;
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         return GetEnumerator();
      }
   }
}