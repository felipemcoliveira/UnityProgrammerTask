using UnityEngine;

namespace UnityProgrammerTask.Gameplay
{
   [CreateAssetMenu(fileName = "HealingPotion", menuName = "Game/Items/Healing Potion")]
   public class HealingPotion : Item
   {
      public override bool IsConsumable => true;

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

      public override string GetDescription()
      {
         string staticDescription = GetStaticDescription();
         return string.Format(staticDescription, m_HealAmount);
      }
   }
}
