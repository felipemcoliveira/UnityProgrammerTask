using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityProgrammerTask.Gameplay;

namespace UnityProgrammerTask.Presentation
{
   public class EquipmentPanel : UIBehaviour, IItemDropHandler
   {
      [SerializeField]
      private EquipmentView m_EquipmentViewPrefab;

      [SerializeField]
      private Canvas m_Canvas;

      [SerializeField]
      private InputAction m_ToggleVisibilityAction;

      private PanelVisibility m_PanelVisibility;
      private bool m_Initialized = false;
      private Dictionary<CharacterEquipmentSlot, EquipmentSlot> m_Slots = new();
      private List<EquipmentView> m_EquipmentViews = new();
      private CharacterEquipment m_CharacterEquipment;

      protected override void Awake()
      {
         m_PanelVisibility = GetComponent<PanelVisibility>();

         m_ToggleVisibilityAction.Enable();
         m_ToggleVisibilityAction.performed += OnToggleVisibility;
      }

      protected override void OnDestroy()
      {
         m_ToggleVisibilityAction.performed -= OnToggleVisibility;
      }

      protected override void OnEnable()
      {
         if (Character.PlayerCharacter == null)
         {
            Character.PlayerCharacterSet += OnPlayerCharacterSet;
            return;
         }

         CharacterEquipment characterEquipment = Character.PlayerCharacter.GetComponent<CharacterEquipment>();
         Initialize(characterEquipment);
      }

      private void OnPlayerCharacterSet(Character character)
      {
         m_CharacterEquipment = character.GetComponent<CharacterEquipment>();
         RefreshItems();
      }

      private void Initialize(CharacterEquipment characterEquipment)
      {
         if (m_Initialized)
            return;

         m_CharacterEquipment = characterEquipment;

         FetchSlots();
         RefreshItems();

         m_CharacterEquipment.EquipmentChanged += _ => RefreshItems();

         m_Initialized = true;
      }

      private void RefreshItems()
      {
         if (m_CharacterEquipment == null)
            return;

         foreach (EquipmentView equipmentView in m_EquipmentViews)
            Destroy(equipmentView.gameObject);

         m_EquipmentViews.Clear();

         foreach (Equipment equipment in m_CharacterEquipment)
         {
            EquipmentSlot slot = m_Slots[equipment.Slot];
            EquipmentView equipmentView = Instantiate(m_EquipmentViewPrefab, slot.transform);

            equipmentView.Initialize(m_CharacterEquipment, equipment);
            equipmentView.SetCanvas(m_Canvas);

            m_EquipmentViews.Add(equipmentView);
         }
      }

      private void FetchSlots()
      {
         m_Slots.Clear();

         EquipmentSlot[] slots = GetComponentsInChildren<EquipmentSlot>();

         foreach (EquipmentSlot slot in slots)
         {
            slot.Initialize(m_CharacterEquipment);
            m_Slots[slot.Slot] = slot;
         }
      }

      private void OnToggleVisibility(InputAction.CallbackContext context)
      {
         if (Character.PlayerCharacter == null)
            return;

         m_PanelVisibility.ToggleVisibility();
      }

      public void OnItemDropped(ItemInInventory item)
      {
         if (item.ItemDefinition is not Equipment equipment)
            return;

         m_CharacterEquipment.Unequip(equipment, ReturningPolicy.ReturnToInventory);
      }

      public void OnItemDropped(EquippedItem item)
      {
         // Ignore drops on the equipment panel
      }
   }
}