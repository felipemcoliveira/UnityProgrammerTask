using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityProgrammerTask.Core
{
   [Serializable]
   [GameContext(GameContextID.MainMenu)]
   public class MainMenuContext : GameContext
   {
      private AsyncOperation m_LoadSceneOperation;

      public override void OnEnter()
      {
         m_LoadSceneOperation = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Additive);
         m_LoadSceneOperation.completed += OnSceneLoaded;
      }

      public override GameContextID Update()
      {
         if (!m_LoadSceneOperation.isDone)
            return StateID; // Await until the scene is loaded.

         if (MessageQueue.TryPoll<StartNewGameMessage>())
            return GameContextID.NewGameGameplay;

         if (MessageQueue.TryPoll<QuitGameMessage>())
            return GameContextID.None;

         return StateID;
      }

      private void OnSceneLoaded(AsyncOperation operation)
      {
         Scene scene = SceneManager.GetSceneByName("MainMenu");
         SceneManager.SetActiveScene(scene);
      }

      public override void OnExit()
      {
         SceneManager.UnloadSceneAsync("MainMenu");
      }
   }

}