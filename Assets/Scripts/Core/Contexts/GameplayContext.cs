using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityProgrammerTask.Core
{
   public abstract class GameplayContext : GameContext
   {
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