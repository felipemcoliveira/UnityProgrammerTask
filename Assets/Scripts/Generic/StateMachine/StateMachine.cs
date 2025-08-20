using System;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Assertions;

namespace UnityProgrammerTask.Core
{
   public class StateMachine<T> where T : struct, Enum
   {
      public event Action<T> StateChanged;

      public bool IsRunning => m_CurrentState != null && !EnumEqualsFast(m_CurrentState.StateID, m_HaltState);

      private readonly Dictionary<T, IStateMachineState<T>> m_States;
      private IStateMachineState<T> m_CurrentState;
      private T m_HaltState;
      private bool m_IsFinalized;

      public StateMachine(IEnumerable<IStateMachineState<T>> states, T initialState, T haltState)
      {
         if (states == null)
            throw new ArgumentNullException(nameof(states), "States cannot be null.");

         m_States = new Dictionary<T, IStateMachineState<T>>();
         foreach (IStateMachineState<T> state in states)
         {
            if (state == null)
               throw new ArgumentException("State cannot be null.", nameof(states));

            if (m_States.ContainsKey(state.StateID))
               throw new ArgumentException($"Duplicate state ID: {state.StateID}", nameof(states));

            m_States[state.StateID] = state;
         }

         foreach (IStateMachineState<T> state in m_States.Values)
            state.Initialize();

         TransitionToState(initialState);
         m_HaltState = haltState;
      }

      public void Update()
      {
         Assert.IsFalse(m_IsFinalized, "State machine has been finalized and cannot be updated.");

         if (!IsRunning)
            return;

         T nextStateID = m_CurrentState.Update();

         if (!EnumEqualsFast(nextStateID, m_CurrentState.StateID))
            TransitionToState(nextStateID);
      }

      public void FinalizeStateMachine()
      {
         m_IsFinalized = true;

         if (m_CurrentState != null)
         {
            m_CurrentState.OnExit();
            m_CurrentState = null;
         }
      }

      private void TransitionToState(T newStateID)
      {
         if (m_CurrentState != null && EnumEqualsFast(m_CurrentState.StateID, newStateID))
         {
            Debug.LogWarning($"Already in state {newStateID}, no transition needed.");
            return;
         }

         m_CurrentState?.OnExit();

         if (EnumEqualsFast(newStateID, m_HaltState))
         {
            m_CurrentState = null;
            return;
         }

         if (!m_States.TryGetValue(newStateID, out IStateMachineState<T> newState))
            throw new ArgumentException($"State {newStateID} not found in states.", nameof(newStateID));

         m_CurrentState = newState;
         m_CurrentState.OnEnter();

         StateChanged?.Invoke(newStateID);
      }

      private static bool EnumEqualsFast<TEnum>(TEnum a, TEnum b) where TEnum : struct, Enum
      {
         return UnsafeUtility.SizeOf<TEnum>() switch
         {
            1 => UnsafeUtility.As<TEnum, byte>(ref a) == UnsafeUtility.As<TEnum, byte>(ref b),
            2 => UnsafeUtility.As<TEnum, ushort>(ref a) == UnsafeUtility.As<TEnum, ushort>(ref b),
            4 => UnsafeUtility.As<TEnum, uint>(ref a) == UnsafeUtility.As<TEnum, uint>(ref b),
            8 => UnsafeUtility.As<TEnum, ulong>(ref a) == UnsafeUtility.As<TEnum, ulong>(ref b),
            _ => false
         };
      }
   }
}