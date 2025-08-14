using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace UnityProgrammerTask.Gameplay
{
   [RequireComponent(typeof(Character))]
   public class CharacterEquipment : MonoBehaviour, IEnumerable<Equipment>
   {
      public Action EquipmentChanged;

      private Dictionary<CharacterEquipmentSlot, Equipment> m_EquipmentSlots = new();
      private Character m_Character;

      private void Awake()
      {
         m_Character = GetComponent<Character>();
      }

      public void EquipItem(Equipment equipment)
      {
         if (m_EquipmentSlots.TryGetValue(equipment.Slot, out Equipment existingEquipment))
            UnequipItem(existingEquipment);

         Assert.IsNotNull(equipment, "Equipment cannot be null.");
         m_EquipmentSlots[equipment.Slot] = equipment;

         equipment.ApplyEffects(m_Character);
         EquipmentChanged?.Invoke();
      }

      public void UnequipItem(Equipment equipment)
      {
         Assert.IsNotNull(equipment, "Equipment cannot be null.");
         Assert.IsTrue(m_EquipmentSlots.ContainsKey(equipment.Slot), "Equipment is not equipped in the specified slot.");

         m_EquipmentSlots[equipment.Slot].RemoveEffects(m_Character);
         m_EquipmentSlots.Remove(equipment.Slot);

         EquipmentChanged?.Invoke();

         Inventory inventory = m_Character.Inventory;
         if (inventory.AddItem(equipment, 1) == 1)
            return;

         equipment.CreateWorldRepresentation(transform.position, 1);
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
            UnequipItem(equipment);
      }
   }
}