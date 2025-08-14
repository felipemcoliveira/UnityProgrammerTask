using System;
using System.IO;
using UnityEngine;
using UnityProgrammerTask.Gameplay;

namespace UnityProgrammerTask.Core
{
   [Serializable]
   public class SaveableItemRepresentationHandler : ISaveableComponentHandler
   {
      public void Load(GameObject gameObject, BinaryReader stream)
      {
         ItemRepresentation itemRepresentation = gameObject.GetComponent<ItemRepresentation>();
         if (itemRepresentation == null)
            return;

         GUID itemId = GUID.Read(stream);
         Item item = ItemLibrary.Instance.GetItemByID(itemId);

         if (item != null)
            itemRepresentation.Initialize(item, stream.ReadInt32());
      }

      public void Save(GameObject gameObject, BinaryWriter stream)
      {
         ItemRepresentation itemRepresentation = gameObject.GetComponent<ItemRepresentation>();
         if (itemRepresentation == null)
            return;

         itemRepresentation.Item.ID.Write(stream);
         stream.Write(itemRepresentation.Quantity);
      }
   }
}