using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityProgrammerTask.Core
{
   [GameContext(GameContextID.NewGameGameplay)]
   public class NewGameGameplayContext : GameplayContext
   {
      private Awaitable m_Startup;

      public override void OnEnter()
      {
         SaveSystem.StartNewSave();
      }

      public override GameContextID Update()
      {
         if (m_Startup == null)
            m_Startup = Startup();

         // Await until startup is completed.
         if (!m_Startup.IsCompleted)
            return StateID;

         HandleSaveGameMessage();
         return StateID;
      }

      public int Write(BinaryWriter stream)
      {
         return 0;
      }

      private async Awaitable Startup()
      {
         await SceneManager.LoadSceneAsync("GameLevel", LoadSceneMode.Additive);
      }
   }
}