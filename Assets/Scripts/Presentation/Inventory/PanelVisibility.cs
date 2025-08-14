using DG.Tweening;
using UnityEngine;

namespace UnityProgrammerTask.Presentation
{
   public class PanelVisibility : MonoBehaviour
   {
      private enum AnimatingState
      {
         None,
         Opening,
         Closing
      }

      [SerializeField]
      private float m_VisiblePivot;

      [SerializeField]
      private float m_HiddenPivot;

      [SerializeField]
      private float m_AnchoredPositionXOffset = 100f;

      [SerializeField]
      private float m_AnimationDuration = 0.2f;

      private RectTransform m_RectTransform;
      private AnimatingState m_AnimatingState = AnimatingState.None;

      private void Start()
      {
         m_RectTransform = GetComponent<RectTransform>();
         m_RectTransform.pivot = new Vector2(m_HiddenPivot, m_RectTransform.pivot.y);
         m_RectTransform.anchoredPosition = new Vector2(-m_AnchoredPositionXOffset, m_RectTransform.anchoredPosition.y);

         gameObject.SetActive(false);
      }

      public void ToggleVisibility()
      {
         if (m_AnimatingState != AnimatingState.None)
            return;

         if (gameObject.activeSelf)
         {
            StartCloseAnimation();
            return;
         }

         StartOpenAnimation();
      }

      public void StartOpenAnimation()
      {
         if (m_AnimatingState != AnimatingState.None)
            return;

         gameObject.SetActive(true);

         m_AnimatingState = AnimatingState.Opening;

         m_RectTransform.DOAnchorPosX(m_AnchoredPositionXOffset, m_AnimationDuration).SetEase(Ease.OutCubic);
         m_RectTransform.DOPivotX(m_VisiblePivot, m_AnimationDuration).SetEase(Ease.OutBack);

         DOVirtual.DelayedCall(m_AnimationDuration, () =>
         {
            m_AnimatingState = AnimatingState.None;
         });
      }

      public void StartCloseAnimation()
      {
         if (m_AnimatingState != AnimatingState.None)
            return;

         m_AnimatingState = AnimatingState.Closing;

         m_RectTransform.DOAnchorPosX(-m_AnchoredPositionXOffset, m_AnimationDuration).SetEase(Ease.InCubic);
         m_RectTransform.DOPivotX(m_HiddenPivot, m_AnimationDuration).SetEase(Ease.InBack);

         DOVirtual.DelayedCall(m_AnimationDuration, () =>
         {
            gameObject.SetActive(false);
            m_AnimatingState = AnimatingState.None;
         });
      }
   }
}
