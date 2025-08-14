using System;
using System.IO;
using UnityEngine;
using UnityProgrammerTask.Gameplay;

namespace UnityProgrammerTask.Core
{
   [Serializable]
   public class SaveableCharacterStatsHandler : ISaveableComponentHandler
   {
      public void Load(GameObject gameObject, BinaryReader stream)
      {
         Character character = gameObject.GetComponent<Character>();
         if (character == null)
            return;

         float health = stream.ReadSingle();
         character.SetHealth(health);
      }
      public void Save(GameObject gameObject, BinaryWriter stream)
      {
         Character character = gameObject.GetComponent<Character>();
         if (character == null)
            return;

         stream.Write(character.Health);
      }
   }
}