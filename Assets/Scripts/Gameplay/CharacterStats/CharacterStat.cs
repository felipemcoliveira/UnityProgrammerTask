using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityProgrammerTask.Gameplay
{
   public class CharacterStat
   {
      private struct Modifier
      {
         public int Handle;
         public float Value;
      }

      public Action<CharacterStat> StatChanged;
      public Action<CharacterStat> StatActivated;
      public Action<CharacterStat> StatDeactivated;

      public CharacterStatID ID => m_ID;
      public float CurrentValue => m_CurrentValue;
      public bool IsActive => m_IsActive;

      public float BaseValue
      {
         get => m_BaseValue;
         set
         {
            if (Mathf.Approximately(m_BaseValue, value))
               return;

            m_BaseValue = value;
            m_CurrentValue = EvaluateCurrentValue();
            StatChanged?.Invoke(this);
         }
      }

      private int m_BonusHandleGenerator = 0;

      private CharacterStatID m_ID;
      private float m_BaseValue;
      private float m_CurrentValue;
      private List<Modifier> m_PercentageModifiers = new();
      private List<Modifier> m_FlatModifiers = new();
      private bool m_IsActive;

      public CharacterStat(CharacterStatID id, float baseValue)
      {
         m_ID = id;
         m_BaseValue = baseValue;
         m_CurrentValue = baseValue;
      }

      public void SetActive(bool isActive)
      {
         if (m_IsActive == isActive)
            return;

         m_IsActive = isActive;

         if (isActive)
         {
            StatActivated?.Invoke(this);
            return;
         }
         else
            StatDeactivated?.Invoke(this);
      }

      public CharacterStatModifierHandle AddModifier(CharacterStatModifierType type, float value)
      {
         Modifier modifier = new()
         {
            Handle = m_BonusHandleGenerator++,
            Value = value
         };

         List<Modifier> modifiers = type == CharacterStatModifierType.Percentage ? m_PercentageModifiers : m_FlatModifiers;
         modifiers.Add(modifier);

         m_CurrentValue = EvaluateCurrentValue();
         StatChanged?.Invoke(this);

         return new CharacterStatModifierHandle(modifier.Handle, this, type);
      }

      public void RemoveModifier(CharacterStatModifierHandle handle)
      {
         List<Modifier> modifiers = handle.Type == CharacterStatModifierType.Percentage ? m_PercentageModifiers : m_FlatModifiers;
         int index = -1;
         for (int i = 0; i < modifiers.Count; i++)
         {
            if (modifiers[i].Handle == handle.Handle)
            {
               index = i;
               break;
            }
         }

         if (index == -1)
         {
            Debug.LogWarning($"Modifier with handle {handle.Handle} not found in stat {m_ID}.");
            return;
         }

         modifiers.RemoveAt(index);

         m_CurrentValue = EvaluateCurrentValue();
         StatChanged?.Invoke(this);
      }

      public void UpdateModifier(CharacterStatModifierHandle handle, float newValue)
      {
         List<Modifier> modifiers = handle.Type == CharacterStatModifierType.Percentage ? m_PercentageModifiers : m_FlatModifiers;
         for (int i = 0; i < modifiers.Count; i++)
         {
            if (modifiers[i].Handle == handle.Handle)
            {
               modifiers[i] = new Modifier { Handle = handle.Handle, Value = newValue };

               m_CurrentValue = EvaluateCurrentValue();
               StatChanged?.Invoke(this);

               return;
            }
         }

         Debug.LogWarning($"Modifier with handle {handle.Handle} not found in stat {m_ID}.");
      }

      private float EvaluateCurrentValue()
      {
         float percentageBonus = 0f;
         foreach (Modifier modifier in m_PercentageModifiers)
            percentageBonus += modifier.Value;

         float flatBonus = 0f;
         foreach (Modifier modifier in m_FlatModifiers)
            flatBonus += modifier.Value;

         return (m_BaseValue + flatBonus) * (1 + percentageBonus / 100f);
      }
   }
}