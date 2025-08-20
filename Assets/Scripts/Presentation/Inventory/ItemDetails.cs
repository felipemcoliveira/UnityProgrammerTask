using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityProgrammerTask.Gameplay;

namespace UnityProgrammerTask.Presentation
{
   public class ItemDetails : UIBehaviour
   {
      [SerializeField]
      private TMP_Text m_ItemNameText;

      [SerializeField]
      private Image m_ItemIconImage;

      [SerializeField]
      private TMP_Text m_ItemDescriptionText;

      [SerializeField]
      private TMP_Text m_SubHeadlineText;

      [SerializeField]
      private Vector2 m_Offset;

      [SerializeField]
      private float m_AnimationDuration = 0.1f;

      public void Initialize(Item item, string subHeadline, Vector2 position)
      {
         m_ItemNameText.text = item.ItemName;
         m_ItemIconImage.sprite = item.ItemIcon;
         m_ItemDescriptionText.text = item.GetDescription(GameTextStylerLibrary.DefaultStyler);
         m_SubHeadlineText.text = subHeadline;

         RectTransform rectTransform = (RectTransform)transform;
         CanvasGroup canvasGroup = GetComponent<CanvasGroup>();

         canvasGroup.alpha = 0f;
         rectTransform.localScale = Vector3.one * 0.8f;
         rectTransform.anchoredPosition = position;

         canvasGroup.DOFade(1f, m_AnimationDuration).SetTarget(gameObject);
         rectTransform.DOScale(Vector3.one, m_AnimationDuration).SetTarget(gameObject);

         rectTransform.DOAnchorPos(position + m_Offset, m_AnimationDuration * 2)
            .SetEase(Ease.OutCirc)
            .SetTarget(gameObject);
      }

      public void HideAndDestroy()
      {
         DOTween.Kill(gameObject);

         RectTransform rectTransform = (RectTransform)transform;
         CanvasGroup canvasGroup = GetComponent<CanvasGroup>();

         rectTransform.DOScale(Vector3.one * 0.8f, m_AnimationDuration).SetTarget(gameObject);
         rectTransform.DOAnchorPos(rectTransform.anchoredPosition - m_Offset, m_AnimationDuration).SetTarget(gameObject);

         DOVirtual.DelayedCall(m_AnimationDuration, () =>
         {
            Destroy(gameObject); ;
         });
      }
   }
}