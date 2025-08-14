using System;
using System.IO;
using UnityEngine;
using UnityProgrammerTask.Gameplay;

namespace UnityProgrammerTask.Core
{
   [Serializable]
   public class SaveablePlayerCharacterHandler : ISaveableComponentHandler
   {
      public void Load(GameObject gameObject, BinaryReader stream)
      {
         Character character = gameObject.GetComponent<Character>();
         bool isPlayerCharacter = stream.ReadBoolean();

         if (isPlayerCharacter)
            character.SetAsPlayerCharacter();
      }
      public void Save(GameObject gameObject, BinaryWriter stream)
      {
         Character character = gameObject.GetComponent<Character>();
         bool isPlayerCharacter = character == Character.PlayerCharacter;

         stream.Write(isPlayerCharacter);
      }
   }
}