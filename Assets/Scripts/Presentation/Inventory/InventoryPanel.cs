using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityProgrammerTask.Gameplay;

namespace UnityProgrammerTask.Presentation
{
   public class InventoryPanel : UIBehaviour, IItemDropHandler
   {
      [SerializeField]
      private RectTransform m_SlotsContainer;

      [SerializeField]
      private ItemSlot m_SlotPrefab;

      [SerializeField]
      private ItemView m_ItemViewPrefab;

      [SerializeField]
      private Canvas m_Canvas;

      [SerializeField]
      private InputAction m_ToggleVisibilityAction;

      private PanelVisibility m_PanelVisibility;
      private bool m_Initialized = false;
      private Dictionary<Vector2Int, ItemSlot> m_Slots = new();
      private List<ItemView> m_ItemViews = new();

      protected override void Awake()
      {
         m_PanelVisibility = GetComponent<PanelVisibility>();

         m_ToggleVisibilityAction.Enable();
         m_ToggleVisibilityAction.performed += OnToggleVisibility;
      }

      protected override void OnEnable()
      {
         if (Character.PlayerCharacter == null)
         {
            Character.PlayerCharacterSet += OnPlayerCharacterSet;
            return;
         }

         Initialize(Character.PlayerCharacter.Inventory);
      }

      private void OnPlayerCharacterSet(Character character)
      {
         Initialize(character.Inventory);
      }

      private void Initialize(Inventory inventory)
      {
         if (m_Initialized)
            return;

         InstantiateSlots(inventory);
         RefreshItems(inventory);

         inventory.InventoryChanged += RefreshItems;

         m_Initialized = true;
      }

      private void RefreshItems(Inventory inventory)
      {
         foreach (ItemView itemView in m_ItemViews)
            Destroy(itemView.gameObject);

         m_ItemViews.Clear();

         foreach (ItemInInventory item in inventory)
         {
            if (!m_Slots.TryGetValue(item.Position, out ItemSlot itemSlot))
            {
               Debug.LogError($"No slot found at position {item.Position}");
               continue;
            }

            ItemView itemView = Instantiate(m_ItemViewPrefab, itemSlot.transform);

            itemView.SetCanvas(m_Canvas);
            itemView.Initialize(item);

            m_ItemViews.Add(itemView);
         }
      }

      private void InstantiateSlots(Inventory inventory)
      {
         foreach (ItemSlot slot in m_Slots.Values)
         {
            if (slot != null)
               Destroy(slot.gameObject);
         }

         m_Slots.Clear();

         for (int x = 0; x < inventory.InventoryCapacity.x; x++)
         {
            for (int y = 0; y < inventory.InventoryCapacity.y; y++)
            {
               Vector2Int position = new(x, y);

               ItemSlot slot = Instantiate(m_SlotPrefab, m_SlotsContainer);
               m_Slots.Add(position, slot);

               slot.Initialize(position, inventory);
            }
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
         // When the user drops an item over the inventory, no action is taken.
      }
   }
}