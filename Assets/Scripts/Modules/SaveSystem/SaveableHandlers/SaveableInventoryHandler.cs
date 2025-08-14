using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityProgrammerTask.Gameplay;

namespace UnityProgrammerTask.Core
{
   [Serializable]
   public class SaveableInventoryHandler : ISaveableComponentHandler
   {
      public void Load(GameObject gameObject, BinaryReader stream)
      {
         Inventory inventory = gameObject.GetComponent<Inventory>();

         int itemCount = stream.ReadInt16();

         for (int i = 0; i < itemCount; i++)
         {
            GUID itemId = GUID.Read(stream);
            int quantity = stream.ReadInt32();
            int posX = stream.ReadInt16();
            int posY = stream.ReadInt16();
            Vector2Int position = new(posX, posY);

            Item item = ItemLibrary.Instance.GetItemByID(itemId);

            inventory.AddItem(position, item, quantity);
         }
      }

      public void Save(GameObject gameObject, BinaryWriter stream)
      {
         Inventory iventory = gameObject.GetComponent<Inventory>();

         ItemInInventory[] items = iventory.ToArray();
         stream.Write((short)items.Length);

         for (int i = 0; i < items.Length; i++)
         {
            ItemInInventory item = items[i];

            item.ItemDefinition.ID.Write(stream);
            stream.Write(item.Quantity);
            stream.Write((short)item.Position.x);
            stream.Write((short)item.Position.y);
         }
      }
   }
}