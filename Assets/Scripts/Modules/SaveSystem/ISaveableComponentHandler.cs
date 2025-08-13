using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UnityProgrammerTask.Core
{
   public interface ISaveableComponentHandler
   {
      public void Load(GameObject gameObject, BinaryReader stream);
      public void Save(GameObject gameObject, BinaryWriter stream);

      public void GetDependencies(GameObject gameObject, List<SaveableGameObject> dependencies)
      {
         // Default implementation does nothing
      }

   }
}