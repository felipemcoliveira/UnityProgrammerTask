using DG.Tweening;
using System;
using UnityEngine;

namespace UnityProgrammerTask.Presentation
{
   public class MainMenu : MonoBehaviour
   {
      [SerializeField]
      private AudioClip m_OpenAndCloseSoundFX;

      [SerializeField]
      private AudioSource m_BackgroundSoundAudioSource;

      [SerializeField]
      private float m_AnimationDelayInSeconds = 0.5f;

      [SerializeField]
      private float m_AnimationDurationInSeconds = 1.5f;

      private CanvasGroup m_CanvasGroup;
      private RectTransform m_RectTransform;
      private AudioSource m_AudioSource;
      private Sequence m_Sequence;

      private void Start()
      {
         m_RectTransform = GetComponent<RectTransform>();
         m_CanvasGroup = GetComponent<CanvasGroup>();
         m_AudioSource = GetComponent<AudioSource>();

         Vector2 startPos = m_RectTransform.anchoredPosition;
         float originalBackgroundVolume = m_BackgroundSoundAudioSource.volume;

         m_RectTransform.localScale = Vector3.one * 0.9f;
         m_RectTransform.anchoredPosition = new Vector2(startPos.x, startPos.y - 30f);
         m_CanvasGroup.alpha = 0;
         m_BackgroundSoundAudioSource.volume = 0;

         m_Sequence = DOTween.Sequence()
            .SetAutoKill(false);

         m_Sequence.AppendInterval(m_AnimationDelayInSeconds);

         m_AudioSource.clip = m_OpenAndCloseSoundFX;
         m_AudioSource.PlayDelayed(m_AnimationDelayInSeconds);

         m_Sequence.Insert(0f, DOVirtual.DelayedCall(m_AnimationDurationInSeconds, () => { }));

         m_Sequence.Join(m_RectTransform.DOScale(1f, m_AnimationDurationInSeconds).SetEase(Ease.OutBack));
         m_Sequence.Join(m_CanvasGroup.DOFade(1f, m_AnimationDurationInSeconds));
         m_Sequence.Join(m_RectTransform.DOAnchorPosY(startPos.y, m_AnimationDurationInSeconds).SetEase(Ease.OutCubic));
         m_Sequence.Join(m_BackgroundSoundAudioSource.DOFade(originalBackgroundVolume, m_AnimationDurationInSeconds));
      }

      public void HandleStartNewGame()
      {
         MessageQueue.Post<StartNewGameMessage>();
      }

      public void HandleLoadGame()
      {
         MessageQueue.Post<LoadGameMessage>();
      }

      public void HandleQuitGame()
      {
         MessageQueue.Post<QuitGameMessage>();
      }

      public void PlayCloseAnimation(Action onComplete)
      {
         m_AudioSource.Play();

         m_Sequence.PlayBackwards();
         m_Sequence.OnRewind(() => onComplete());
      }
   }
}
