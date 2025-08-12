using System;

namespace UnityProgrammerTask.Core
{
   [Serializable]
   public abstract class GameContext : IStateMachineState<GameContextID>
   {
      public GameContextID StateID
      {
         get
         {
            if (m_StateID == GameContextID.None)
               m_StateID = GetContextIDFromAttribute();

            return m_StateID;
         }
      }

      private GameContextID m_StateID = GameContextID.None;

      public virtual void Initialize()
      {
         // Default implementation can be empty.
      }

      public virtual void OnEnter()
      {
         // Default implementation can be empty.
      }

      public virtual void OnExit()
      {
         // Default implementation can be empty.
      }

      /// <returns>
      /// Returns the next game context to transition to, or the current 
      /// context ID if no transition is needed.
      /// </returns>
      public abstract GameContextID Update();

      private GameContextID GetContextIDFromAttribute()
      {
         GameContextAttribute contextAttribute =
            (GameContextAttribute)Attribute.GetCustomAttribute
            (
               GetType(),
               typeof(GameContextAttribute)
            );

         if (contextAttribute == null)
         {
            throw new InvalidOperationException($"GameContext {GetType().Name} is missing " +
               $"{nameof(GameContextAttribute)}.");
         }

         return contextAttribute.ContextID;
      }
   }
}