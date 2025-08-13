using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

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

      public override void OnEnter()
      {
         m_State = State.WaitingStartup;
         LoadingScreen.Show();
      }

      public override GameContextID Update()
      {
         switch (m_State)
         {
            case State.WaitingStartup:
               m_Startup ??= StartupInternal();

               if (m_Startup.IsCompleted)
               {
                  m_State = State.Running;
                  LoadingScreen.Hide();
               }

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
         try
         {
            await Startup();
         }
         catch (Exception e)
         {
            Debug.LogException(e);
            throw;
         }
      }

      public override void OnExit()
      {
         // guarantee that loading screen is hidden
         LoadingScreen.Hide();

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

   public class LoadingScreen
   {
      public static void Show()
      {

      }

      public static void Hide()
      {
      }
   }
}

