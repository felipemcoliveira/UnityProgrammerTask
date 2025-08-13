using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UnityProgrammerTask.Presentation
{
   public class LoadingScreen : MonoBehaviour
   {
      public static bool IsFadingIn => s_Instance != null && s_Instance.IsFadingInInternal();

      private static LoadingScreen s_Instance;

      [SerializeField]
      private float m_FadeDuration = 0.4f;

      [SerializeField]
      private TMP_Text m_LoadingText;

      private CanvasGroup m_CanvasGroup;
      private Tween m_FadeInTween;

      public static void Show()
      {
         if (s_Instance != null)
            return;

         s_Instance = Addressables.InstantiateAsync("LoadingScreen")
            .WaitForCompletion()
            .GetComponent<LoadingScreen>();

         DontDestroyOnLoad(s_Instance.gameObject);
      }

      private void Start()
      {
         m_CanvasGroup = GetComponent<CanvasGroup>();
         m_CanvasGroup.alpha = 0f;

         m_FadeInTween = m_CanvasGroup.DOFade(1, m_FadeDuration);

         // blink the loading text
         m_LoadingText.DOFade(0.5f, 0.9f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
      }

      public static void Hide()
      {
         if (s_Instance != null)
            s_Instance.HideInternal();
      }

      private void HideInternal()
      {
         m_CanvasGroup.DOFade(0, m_FadeDuration)
            .OnComplete(() => Addressables.ReleaseInstance(gameObject));
      }

      private bool IsFadingInInternal()
      {
         return m_FadeInTween != null && m_FadeInTween.IsActive() && m_FadeInTween.IsPlaying();
      }
   }
}

