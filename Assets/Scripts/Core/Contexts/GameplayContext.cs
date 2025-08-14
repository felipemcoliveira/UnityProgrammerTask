using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityProgrammerTask.Gameplay;
using UnityProgrammerTask.Presentation;

namespace UnityProgrammerTask.Core
{
   public abstract class GameplayContext : GameContext
   {
      private enum State
      {
         WaitingStartup,
         Running,
      }

      [SerializeField]
      private PlayerInput m_PlayerInputPrefab;

      private Awaitable m_Startup;
      private State m_State = State.WaitingStartup;
      private PlayerInput m_PlayerInput;
      private List<Scene> m_GameplayScenes = new();

      protected abstract Awaitable Startup();

      public override void OnEnter()
      {
         m_State = State.WaitingStartup;
      }

      public override GameContextID Update()
      {
         switch (m_State)
         {
            case State.WaitingStartup:
               m_Startup ??= StartupInternal();

               if (m_Startup.IsCompleted)
               {
                  OnStartupComplete();
                  m_State = State.Running;
               }

               return StateID;

            case State.Running:
               HandleSaveGameMessage();

               if (MessageQueue.TryPoll<ReturnToMainMenuMessage>())
                  return GameContextID.MainMenu;

               if (MessageQueue.TryPoll<QuitGameMessage>())
                  return GameContextID.None;

               // Heres is where the game mode logic would be processed in a real game.
               // m_GameMode.Update();
               // if (m_GameMode.IsFinished)
               //    return GameContextID.MainMenu; // return to main menu when game mode is finished.

               return StateID;

            default:
               throw new NotImplementedException($"{m_State}");
         }
      }

      protected virtual void OnStartupComplete()
      {
         m_PlayerInput = UnityEngine.Object.Instantiate(m_PlayerInputPrefab);
         UnityEngine.Object.DontDestroyOnLoad(m_PlayerInput.gameObject);
      }

      private async Awaitable StartupInternal()
      {
         m_State = State.WaitingStartup;
         LoadingScreen.Show();

         try
         {
            float startTime = Time.realtimeSinceStartup;

            while (LoadingScreen.IsFadingIn)
            {
               // Wait until the loading screen is fully faded in
               await Awaitable.EndOfFrameAsync();
            }

            await Startup();

            const float fakeDelay = 1.5f;
            if (Time.realtimeSinceStartup - startTime < fakeDelay)
            {
               // If startup is too fast, wait a bit to show the loading screen
               await Awaitable.WaitForSecondsAsync(fakeDelay - (Time.realtimeSinceStartup - startTime));
            }
         }
         catch (Exception e)
         {
            Debug.LogException(e);
         }
         finally
         {
            LoadingScreen.Hide();
         }
      }

      public override void OnExit()
      {
         SaveSystem.DisposeActiveSave();

         if (m_PlayerInput != null)
            UnityEngine.Object.Destroy(m_PlayerInput.gameObject);

         foreach (Scene scene in m_GameplayScenes)
         {
            if (scene.isLoaded)
               SceneManager.UnloadSceneAsync(scene);
         }

         m_GameplayScenes.Clear();

         m_Startup = null;
      }

      protected static string GetSaveFilePath()
      {
         string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
         string saveDir = Path.Combine(documentsPath, "UnityProgrammerTask");
         Directory.CreateDirectory(saveDir);

         string saveFileName = "SaveGame.dat";
         return Path.Combine(saveDir, saveFileName);
      }

      protected void HandleSaveGameMessage()
      {
         if (!MessageQueue.TryPoll<SaveGameMessage>())
            return;

         SaveSystem.Save(GetSaveFilePath());
      }

      protected async Awaitable LoadGameplayScene(string sceneName)
      {
         await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

         Scene scene = SceneManager.GetSceneByName(sceneName);
         if (m_GameplayScenes.Contains(scene))
            return;

         m_GameplayScenes.Add(scene);
      }
   }
}