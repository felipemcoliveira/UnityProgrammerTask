using DG.Tweening;
using UnityEngine;


namespace UnityProgrammerTask.Core
{
   public class GameFlow : MonoBehaviour
   {
      private static readonly Logger s_Logger = Logger.Create("GameFlow", "#8954FB");

      [SerializeField]
      private GameContextID m_InitialGameContext;

      [SerializeReference, SubclassSelector]
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
            s_Logger.Log(LogType.Log, "Game flow has stopped running.", this);
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
         s_Logger.Log(LogType.Log, $"Switched to context: {newContextID}", this);
      }

      private void QuitApplication()
      {
         s_Logger.Log(LogType.Log, "Quitting application.", this);

#if UNITY_EDITOR
         UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
      }
   }
}