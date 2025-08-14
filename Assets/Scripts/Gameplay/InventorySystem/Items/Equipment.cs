using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UnityProgrammerTask.Gameplay
{
   public interface IEquipmentEffect
   {
      public void ApplyEffect(Character character);
      public void RemoveEffect(Character character);

      public string GetDescription() => string.Empty;
   }

   [Serializable]
   public class LogOnlyEffect : IEquipmentEffect
   {
      [SerializeField]
      private string m_Message;

      public void ApplyEffect(Character character)
      {
         Debug.Log($"Applying effect: {m_Message}");
      }

      public void RemoveEffect(Character character)
      {
         Debug.Log($"Removing effect: {m_Message}");
      }
   }

   [Serializable]
   public class MovementSpeedEffect : IEquipmentEffect
   {
      [SerializeField]
      private int m_BonusPercentage;

      public void ApplyEffect(Character character)
      {
         character.AddMovementSpeedBonus(m_BonusPercentage);
      }

      public void RemoveEffect(Character character)
      {
         character.RemoveMovementSpeedBonus(m_BonusPercentage);
      }

      public string GetDescription()
      {
         bool isPositive = m_BonusPercentage >= 0;
         int absoluteBonus = Mathf.Abs(m_BonusPercentage);
         string sign = isPositive ? "+" : "-";

         string bonus = $"{sign}{absoluteBonus}% Movement Speed";
         if (!isPositive)
            return $"<color=#eb4034>{bonus}</color>";

         return $"<color=#19fc7f>{bonus}</color>";
      }
   }

   [CreateAssetMenu(fileName = "Equipment", menuName = "Game/Items/Equipment")]
   public class Equipment : Item, IEnumerable<IEquipmentEffect>
   {
      public override bool IsConsumable => true;

      public CharacterEquipmentSlot Slot => m_Slot;

      [SerializeField]
      private CharacterEquipmentSlot m_Slot;

      [SerializeReference, SubclassSelector]
      private IEquipmentEffect[] m_Effects;

      public void ApplyEffects(Character character)
      {
         foreach (IEquipmentEffect effect in m_Effects)
            effect.ApplyEffect(character);
      }

      public void RemoveEffects(Character character)
      {
         foreach (IEquipmentEffect effect in m_Effects)
            effect.RemoveEffect(character);
      }

      public override bool TryConsume(Character character, ref int quantity)
      {
         quantity = 0;
         return true;
      }

      public override void OnItemConsumed(Character character, int quantity)
      {
         CharacterEquipment characterEquipment = character.GetComponent<CharacterEquipment>();
         characterEquipment.EquipItem(this);
      }

      public override string GetDescription()
      {
         string staticDescription = base.GetDescription();

         StringBuilder descriptionBuilder = new(staticDescription);

         if (m_Effects.Length > 0)
         {
            descriptionBuilder.AppendLine();
            descriptionBuilder.AppendLine();

            foreach (IEquipmentEffect effect in m_Effects)
            {
               string effectDescription = effect.GetDescription();
               if (!string.IsNullOrEmpty(effectDescription))
                  descriptionBuilder.AppendLine(effectDescription);
            }
         }

         return descriptionBuilder.ToString();
      }

      public IEnumerator<IEquipmentEffect> GetEnumerator()
      {
         foreach (IEquipmentEffect effect in m_Effects)
            yield return effect;
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         return GetEnumerator();
      }
   }
}
