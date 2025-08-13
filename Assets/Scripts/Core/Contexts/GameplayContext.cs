using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
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

      private Awaitable m_Startup;
      private State m_State = State.WaitingStartup;

      protected abstract Awaitable Startup();

      public override GameContextID Update()
      {
         switch (m_State)
         {
            case State.WaitingStartup:
               m_Startup ??= StartupInternal();

               if (m_Startup.IsCompleted)
                  m_State = State.Running;

               return StateID;

            case State.Running:
               HandleSaveGameMessage();

               // Heres is where the game mode logic would be processed in a real game.
               // m_GameMode.Update();
               // if (m_GameMode.IsFinished)
               //    return GameContextID.MainMenu; // return to main menu when game mode is finished.

               return StateID;

            default:
               throw new NotImplementedException($"{m_State}");
         }
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

      protected async Awaitable LoadLevel(string levelName)
      {
         await SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);
      }
   }
}