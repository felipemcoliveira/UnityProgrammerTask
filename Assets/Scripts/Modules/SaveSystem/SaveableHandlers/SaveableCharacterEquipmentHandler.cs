using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityProgrammerTask.Gameplay;

namespace UnityProgrammerTask.Core
{
   [Serializable]
   public class SaveableCharacterEquipmentHandler : ISaveableComponentHandler
   {
      public void Load(GameObject gameObject, BinaryReader stream)
      {
         CharacterEquipment characterEquipment = gameObject.GetComponent<CharacterEquipment>();
         if (characterEquipment == null)
            return;

         int equipmentCount = stream.ReadInt32();
         for (int i = 0; i < equipmentCount; i++)
         {
            GUID itemId = GUID.Read(stream);
            Item item = ItemLibrary.Instance.GetItemByID(itemId);
            if (item is Equipment equipment)
               characterEquipment.Equip(equipment);
         }
      }
      public void Save(GameObject gameObject, BinaryWriter stream)
      {
         CharacterEquipment characterEquipment = gameObject.GetComponent<CharacterEquipment>();
         if (characterEquipment == null)
            return;

         Equipment[] equippedItems = characterEquipment.ToArray();
         stream.Write(equippedItems.Length);
         foreach (Equipment equipment in equippedItems)
            equipment.ID.Write(stream);
      }
   }
}