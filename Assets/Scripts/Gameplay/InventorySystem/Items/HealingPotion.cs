using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityProgrammerTask.Gameplay
{
   [CreateAssetMenu(fileName = "HealingPotion", menuName = "Game/Items/Healing Potion")]
   public class HealingPotion : Item
   {
      public override bool IsConsumable => true;

      [TitleGroup("Consume Effect")]
      [SerializeField]
      private int m_HealAmount = 10;

      public override bool TryConsume(Character character, ref int quantity)
      {
         if (character.IsMaxHealth || !character.IsAlive)
            return false;

         character.Heal(m_HealAmount);
         quantity--;

         return true;
      }

      public override string GetDescription(IGameTextStyler textStyler)
      {
         string staticDescription = GetStaticDescription();
         return string.Format(staticDescription, m_HealAmount);
      }
   }
}