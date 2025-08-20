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

      public string GetDescription(IGameTextStyler textStyler) => string.Empty;
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

      public string GetDescription(IGameTextStyler textStyler)
      {
         bool isPositive = m_BonusPercentage >= 0;
         int absoluteBonus = Mathf.Abs(m_BonusPercentage);
         string sign = isPositive ? "+" : "-";

         string bonus = $"{sign}{absoluteBonus}% Movement Speed";
         if (!isPositive)
            return textStyler.Negative(bonus);

         return textStyler.Positive(bonus);
      }
   }

   public struct EquippedItem
   {
      public Equipment Item { get; }
      public CharacterEquipment CharacterEquipment { get; }

      public EquippedItem(Equipment item, CharacterEquipment characterEquipment)
      {
         Item = item;
         CharacterEquipment = characterEquipment;
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
         characterEquipment.Equip(this);
      }

      public override string GetDescription(IGameTextStyler textStyler)
      {
         string staticDescription = base.GetDescription(textStyler);

         StringBuilder descriptionBuilder = new(staticDescription);

         if (m_Effects.Length > 0)
         {
            descriptionBuilder.AppendLine();
            descriptionBuilder.AppendLine();

            foreach (IEquipmentEffect effect in m_Effects)
            {
               string effectDescription = effect.GetDescription(textStyler);
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