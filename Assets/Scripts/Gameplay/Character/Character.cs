using NUnit.Framework;
using System;
using UnityEngine;

namespace UnityProgrammerTask.Gameplay
{
   public class Character : MonoBehaviour
   {
      public static Action<Character> PlayerCharacterSet;

      public event Action<Character> CharacterDied;

      public static Character PlayerCharacter { get; private set; }

      public Inventory Inventory
      {
         get
         {
            if (m_Inventory == null)
               m_Inventory = GetComponent<Inventory>();

            return m_Inventory;
         }
      }

      private float Health { get; set; } = 100f;

      public bool IsAlive => Health > 0;

      private float m_MaxHealth = 100f;
      private Inventory m_Inventory;

      public void TakeDamage(float damage)
      {
         Assert.IsTrue(damage > 0, "Damage must be greater than zero.");

         Health -= damage;
         if (Health <= 0)
            Health = 0;

         CharacterDied?.Invoke(this);
      }

      public void Heal(float amount)
      {
         Assert.IsTrue(amount > 0, "Heal amount must be greater than zero.");
         Health += Mathf.Min(amount, m_MaxHealth - Health);
      }

      public void SetAsPlayerCharacter()
      {
         Assert.IsNull(PlayerCharacter, "Player character is already set.");

         PlayerCharacter = this;
         PlayerCharacterSet?.Invoke(this);

         Debug.Log($"Character {name} set as player character.");
      }
   }
}