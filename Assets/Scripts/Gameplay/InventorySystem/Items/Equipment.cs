using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityProgrammerTask.Gameplay
{
   public interface IEquipmentEffect
   {
      public void ApplyEffect(Character character);
      public void RemoveEffect(Character character);
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
