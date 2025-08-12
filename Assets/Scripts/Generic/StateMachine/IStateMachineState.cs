using System;

namespace UnityProgrammerTask.Core
{
   public interface IStateMachineState<T> where T : Enum
   {
      public T StateID { get; }

      public void Initialize()
      {
         // Default implementation can be empty.
      }

      public void OnEnter()
      {
         // Default implementation can be empty.
      }

      public void OnExit()
      {
         // Default implementation can be empty.
      }

      public T Update();
   }
}
