using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityProgrammerTask.Presentation
{
   public class ToastMessage : UIBehaviour
   {
      [SerializeField]
      private TMP_Text m_MessageText;

      [SerializeField]
      private float m_DisplayDuration = 3f;

      [SerializeField]
      private float m_AnimationDuration = 0.5f;

      public void Initialize(string message)
      {
         CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
         RectTransform rectTransform = GetComponent<RectTransform>();

         float originalHeight = rectTransform.sizeDelta.y;

         m_MessageText.text = message;

         canvasGroup.alpha = 0f;
         rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, 0f);

         canvasGroup.DOFade(1f, m_AnimationDuration);
         rectTransform.DOSizeDelta(new Vector2(rectTransform.sizeDelta.x, originalHeight), m_AnimationDuration);

         DOVirtual.DelayedCall(m_DisplayDuration, () =>
         {
            canvasGroup.DOFade(0f, m_AnimationDuration);

            Vector2 size = rectTransform.sizeDelta;
            rectTransform.DOSizeDelta(new Vector2(size.x, 0f), m_AnimationDuration);

            DOVirtual.DelayedCall(m_AnimationDuration, () =>
            {
               Destroy(gameObject);
            });
         });
      }
   }
}