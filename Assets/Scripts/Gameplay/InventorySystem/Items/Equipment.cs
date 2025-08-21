using Sirenix.OdinInspector;
using System;
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
   public class CharacterStatModifierEffect : IEquipmentEffect
   {
      [Serializable]
      [TypeRegistryItem("Character Stat Modifier", "Equipment Effects", SdfIconType.LightningChargeFill)]
      public class Factory : IEquipmentEffectFactory
      {
         [HorizontalGroup("Inline", Width = 140), HideLabel]
         [SerializeField]
         private CharacterStatID m_TargetStat;

         [HorizontalGroup("Inline", Width = 130), HideLabel]
         [SerializeField]
         private CharacterStatModifierType m_ModifierType;

         [HorizontalGroup("Inline"), HideLabel]
         [SerializeField]
         private float m_ModifierValue;

         [ShowInInspector, DisplayAsString(EnableRichText = true)]
         private string DescriptionPreview
         {
            get
            {
               IEquipmentEffect tempEffect = CreateEffect();
               return tempEffect.GetDescription(GameTextStylerLibrary.DefaultStyler);
            }
         }

         public IEquipmentEffect CreateEffect()
         {
            return new CharacterStatModifierEffect
            {
               m_TargetStat = m_TargetStat,
               m_ModifierType = m_ModifierType,
               m_ModifierValue = m_ModifierValue
            };
         }
      }

      [SerializeField]
      private CharacterStatID m_TargetStat;

      [SerializeField]
      private CharacterStatModifierType m_ModifierType;

      [SerializeField]
      private float m_ModifierValue;

      private CharacterStatModifierHandle m_ModifierHandle;

      public void ApplyEffect(Character character)
      {
         CharacterStat stat = character.Stats.GetOrCreateStat(m_TargetStat);
         m_ModifierHandle = stat.AddModifier(m_ModifierType, m_ModifierValue);
      }

      public void RemoveEffect(Character character)
      {
         CharacterStat stat = character.Stats.GetOrCreateStat(m_TargetStat);
         stat.RemoveModifier(m_ModifierHandle);
      }

      public string GetDescription(IGameTextStyler textStyler)
      {
         bool isPositive = m_ModifierValue >= 0;
         char sign = isPositive ? '+' : '-';
         float absoluteBonus = Mathf.Abs(m_ModifierValue);
         string statName = textStyler.Stat(m_TargetStat);
         string text = m_ModifierType switch
         {
            CharacterStatModifierType.Flat => $"{sign}{absoluteBonus} {statName}",
            CharacterStatModifierType.Percentage => $"{sign}{absoluteBonus}% {statName}",
            _ => string.Empty
         };

         return isPositive ? textStyler.Positive(text) : textStyler.Negative(text);
      }
   }

   public interface IEquipmentEffectFactory
   {
      public IEquipmentEffect CreateEffect();
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
   public class Equipment : Item
   {
      public override bool IsConsumable => true;
      public CharacterEquipmentSlot Slot => m_Slot;

      public IEquipmentEffectFactory[] EffectFactories => m_EffectFactories;

      [TitleGroup("Equipment"), EnumToggleButtons]
      [SerializeField]
      private CharacterEquipmentSlot m_Slot;

      [TitleGroup("Equipment"), Searchable, ListDrawerSettings(ShowFoldout = false), LabelText("Effects"), InlineProperty]
      [PropertySpace(8, 8), PolymorphicDrawerSettings(ShowBaseType = false)]
      [SerializeReference]
      private IEquipmentEffectFactory[] m_EffectFactories;

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

         List<IEquipmentEffect> effects = CreateEffects();
         if (effects.Count > 0)
         {
            descriptionBuilder.AppendLine();
            descriptionBuilder.AppendLine();

            foreach (IEquipmentEffect effect in effects)
            {
               string effectDescription = effect.GetDescription(textStyler);
               if (!string.IsNullOrEmpty(effectDescription))
                  descriptionBuilder.AppendLine(effectDescription);
            }
         }

         return descriptionBuilder.ToString();
      }

      public List<IEquipmentEffect> CreateEffects()
      {
         List<IEquipmentEffect> effects = new();
         foreach (IEquipmentEffectFactory factory in m_EffectFactories)
         {
            IEquipmentEffect effect = factory.CreateEffect();
            if (effect != null)
               effects.Add(effect);
         }
         return effects;
      }
   }
}