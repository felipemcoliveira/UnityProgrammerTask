using UnityEngine;
using UnityEngine.EventSystems;
using UnityProgrammerTask.Gameplay;

namespace UnityProgrammerTask.Presentation
{
   public class EquipmentSlot : UIBehaviour, IItemDropHandler
   {
      public CharacterEquipmentSlot Slot => m_Slot;

      [SerializeField]
      private CharacterEquipmentSlot m_Slot;

      private CharacterEquipment m_CharacterEquipment;

      public void Initialize(CharacterEquipment characterEquipment)
      {
         m_CharacterEquipment = characterEquipment;
      }

      public void OnItemDropped(ItemInInventory item)
      {
         m_CharacterEquipment.EquipItem(item);
      }

      public void OnItemDropped(EquippedItem item)
      {
         // Ignore equipment drops on the equipment slot
      }
   }
}