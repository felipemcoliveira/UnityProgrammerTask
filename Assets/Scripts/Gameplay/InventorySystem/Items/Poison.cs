using UnityEngine;

namespace UnityProgrammerTask.Gameplay
{
   [CreateAssetMenu(fileName = "Poison", menuName = "Game/Items/Poison")]
   public class Poison : Item
   {
      public override bool IsConsumable => true;

      [SerializeField]
      private int m_DamageAmount = 10;

      public override bool TryConsume(Character character, ref int quantity)
      {
         if (!character.IsAlive)
            return false;

         character.TakeDamage(m_DamageAmount);
         quantity--;

         return true;
      }

      public override string GetDescription(IGameTextStyler textStyler)
      {
         string staticDescription = GetStaticDescription();
         return string.Format(staticDescription, m_DamageAmount);
      }
   }
}
