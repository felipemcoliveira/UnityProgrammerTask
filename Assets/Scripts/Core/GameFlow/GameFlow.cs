using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;


namespace UnityProgrammerTask.Core
{
   [HideMonoScript]
   public class GameFlow : MonoBehaviour
   {
      [SerializeField]
      private GameContextID m_InitialGameContext;

      [Searchable, ListDrawerSettings(ShowFoldout = false), InlineProperty]
      [PropertySpace(16, 16)]
      [SerializeReference]
      private GameContext[] m_GameContexts;

      private StateMachine<GameContextID> m_ContextStateMachine;

      private void Awake()
      {
         DOTween.Init();

         m_ContextStateMachine = new
         (
            m_GameContexts,
            m_InitialGameContext,
            GameContextID.None
         );

         m_ContextStateMachine.StateChanged += OnContextSwitch;
      }

      private void Update()
      {
         m_ContextStateMachine.Update();

         if (!m_ContextStateMachine.IsRunning)
         {
            Debug.Log("Game flow has stopped running.", this);
            Destroy(gameObject);
         }
      }

      private void OnDestroy()
      {
         MessageQueue.LogUnhandledMessages();

         if (m_ContextStateMachine != null)
         {
            m_ContextStateMachine.StateChanged -= OnContextSwitch;
            m_ContextStateMachine.FinalizeStateMachine();
         }

         QuitApplication();
      }

      private void OnContextSwitch(GameContextID newContextID)
      {
         Debug.Log($"Switched to context: {newContextID}", this);
      }

      private void QuitApplication()
      {
         Debug.Log("Quitting application.", this);

#if UNITY_EDITOR
         UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
      }
   }
}