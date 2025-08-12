using System;
using UnityEngine.Assertions;

namespace UnityProgrammerTask.Core
{
   [AttributeUsage(AttributeTargets.Class, Inherited = false)]
   public class GameContextAttribute : Attribute
   {
      public GameContextID ContextID { get; }

      public GameContextAttribute(GameContextID contextID)
      {
         Assert.IsTrue(contextID != GameContextID.None, "Context ID cannot be None.");
         ContextID = contextID;
      }
   }
}