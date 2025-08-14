using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

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

      public float Health { get; private set; } = 100f;

      public bool IsMaxHealth => Health >= m_MaxHealth;

      public bool IsAlive => Health > 0;

      private static readonly int k_IsAliveHash = Animator.StringToHash("IsAlive");
      private static readonly int k_MovementSpeedHash = Animator.StringToHash("MovementSpeed");

      /// <summary>
      /// Movement speed bonus in percentage.
      /// </summary>
      private float m_MovementSpeedBase;
      private int m_MovementSpeedBonus;
      private float m_MaxHealth = 100f;
      private Inventory m_Inventory;
      private NavMeshAgent m_NavMeshAgent;
      private Animator m_Animator;

      private void Awake()
      {
         m_Animator = GetComponent<Animator>();
         m_NavMeshAgent = GetComponent<NavMeshAgent>();

         m_MovementSpeedBase = m_NavMeshAgent.speed;
      }

      private void Update()
      {
         m_Animator.SetFloat(k_MovementSpeedHash, m_NavMeshAgent.velocity.magnitude);
         m_Animator.SetBool(k_IsAliveHash, IsAlive);
      }

      public void TakeDamage(float damage)
      {
         if (!IsAlive)
            return;

         Assert.IsTrue(damage > 0, "Damage must be greater than zero.");

         Health -= damage;
         if (Health <= 0)
            Health = 0;

         CharacterDied?.Invoke(this);
      }

      public void Heal(float amount)
      {
         if (!IsAlive)
            return;

         Assert.IsTrue(amount > 0, "Heal amount must be greater than zero.");
         Health += Mathf.Min(amount, m_MaxHealth - Health);
      }

      public void SetAsPlayerCharacter()
      {
         Assert.IsNull(PlayerCharacter, "Player character is already set.");

         PlayerCharacter = this;
         PlayerCharacterSet?.Invoke(this);
      }

      public void SetDestination(Vector3 position)
      {
         if (!IsAlive)
            return;

         m_NavMeshAgent.SetDestination(position);
      }

      public void AddMovementSpeedBonus(int bonus)
      {
         m_MovementSpeedBonus += bonus;
         UpdateMovementSpeed();
      }

      public void RemoveMovementSpeedBonus(int bonus)
      {
         m_MovementSpeedBonus -= bonus;
         UpdateMovementSpeed();
      }

      private void UpdateMovementSpeed()
      {
         m_NavMeshAgent.speed = m_MovementSpeedBase * (1 + m_MovementSpeedBonus / 100f);
      }

      /// <summary>
      /// This method should only be used by the save system.
      /// </summary>
      public void SetHealth(float health)
      {
         Health = Mathf.Min(health, m_MaxHealth);
      }
   }
}