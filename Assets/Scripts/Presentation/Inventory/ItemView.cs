using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityProgrammerTask.Gameplay;

namespace UnityProgrammerTask.Presentation
{
   public interface IItemDropHandler
   {
      public void OnItemDropped(ItemInInventory item);
   }

   public class ItemView : UIBehaviour,
      IBeginDragHandler,
      IDragHandler,
      IEndDragHandler,
      IPointerClickHandler,
      IPointerEnterHandler,
      IPointerExitHandler
   {
      private static bool s_IsDragging;

      [SerializeField]
      private Image m_IconImage;

      [SerializeField]
      private TMP_Text m_QuantityText;

      [SerializeField]
      private ItemDetails m_ItemDetailsPrefab;

      private Canvas m_Canvas;
      private RectTransform m_RectTransform;
      private CanvasGroup m_CanvasGroup;
      private Transform m_OriginalParent;
      private int m_OriginalSiblingIndex;
      private ItemInInventory m_Item;
      private ItemDetails m_ItemDetails;

      private Vector2 m_InitialAnchoredPosition;

      protected override void Awake()
      {
         base.Awake();

         m_RectTransform = (RectTransform)transform;
         m_CanvasGroup = GetComponent<CanvasGroup>();

         if (m_CanvasGroup == null)
            m_CanvasGroup = gameObject.AddComponent<CanvasGroup>();
      }

      public void SetCanvas(Canvas canvas)
      {
         m_Canvas = canvas;
      }

      public void Initialize(ItemInInventory item)
      {
         m_Item = item;

         bool canStack = item.ItemDefinition.MaxStackSize > 1;

         m_QuantityText.gameObject.SetActive(canStack);

         if (canStack)
            m_QuantityText.SetText("{0}", item.Quantity);

         m_IconImage.sprite = item.ItemDefinition.ItemIcon;
      }

      public void OnBeginDrag(PointerEventData eventData)
      {
         if (m_Canvas == null)
            m_Canvas = GetComponentInParent<Canvas>();

         m_OriginalParent = transform.parent;
         m_OriginalSiblingIndex = transform.GetSiblingIndex();
         m_InitialAnchoredPosition = m_RectTransform.anchoredPosition;

         Vector3 worldPos = m_RectTransform.position;

         transform.SetParent(m_Canvas.transform, true);
         m_RectTransform.position = worldPos;
         transform.SetAsLastSibling();

         m_CanvasGroup.blocksRaycasts = false;
         m_CanvasGroup.alpha = 0.5f;

         s_IsDragging = true;
      }

      public void OnDrag(PointerEventData eventData)
      {
         m_RectTransform.anchoredPosition += eventData.delta / m_Canvas.scaleFactor;
      }

      public void OnEndDrag(PointerEventData eventData)
      {
         transform.SetParent(m_OriginalParent, true);
         transform.SetSiblingIndex(m_OriginalSiblingIndex);

         m_RectTransform.anchoredPosition = m_InitialAnchoredPosition;
         m_CanvasGroup.blocksRaycasts = true;
         m_CanvasGroup.alpha = 1f;

         List<RaycastResult> results = new();
         EventSystem.current.RaycastAll(eventData, results);

         s_IsDragging = false;

         IItemDropHandler dropHandler = null;
         for (int i = 0; i < results.Count; i++)
         {
            dropHandler = results[i].gameObject.GetComponentInParent<IItemDropHandler>();
            if (dropHandler != null)
               break;
         }

         if (dropHandler == null)
         {
            Ray raycast = Camera.main.ScreenPointToRay(eventData.position);

            int layerMask = LayerMask.GetMask("Environment");

            if (Physics.Raycast(raycast, out RaycastHit hit, Mathf.Infinity, layerMask))
               m_Item.Drop(hit.point);

            return;
         }

         dropHandler?.OnItemDropped(m_Item);
      }

      public void OnPointerClick(PointerEventData eventData)
      {
         if (eventData.button == PointerEventData.InputButton.Left && eventData.clickCount == 2)
         {
            if (m_Item.Inventory.ConsumeItem(m_Item.Position))
               Toast.Instance.ShowMessage($"Consumed one {m_Item.ItemDefinition.ItemName}.");
         }
      }

      public void OnPointerEnter(PointerEventData eventData)
      {
         if (s_IsDragging)
            return;

         if (m_ItemDetails == null)
         {
            m_ItemDetails = Instantiate(m_ItemDetailsPrefab, m_Canvas.transform);
            m_ItemDetails.transform.SetAsLastSibling();
         }

         Vector3[] corners = new Vector3[4];
         m_RectTransform.GetWorldCorners(corners);

         Vector3 topLeft = corners[1];
         m_ItemDetails.Initialize(m_Item.ItemDefinition, $"Quantity: {m_Item.Quantity}", topLeft);
      }

      public void OnPointerExit(PointerEventData eventData)
      {
         if (m_ItemDetails != null)
         {
            m_ItemDetails.HideAndDestroy();
            m_ItemDetails = null;
         }
      }
   }
}
