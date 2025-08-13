using System;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityProgrammerTask.Core
{
   [Serializable]
   [GameContext(GameContextID.LoadedGameGameplay)]
   public class LoadedGameGameplayContext : GameplayContext
   {
      protected override async Awaitable Startup()
      {
         SaveSystem.LoadSave(GetSaveFilePath());

         await Task.CompletedTask;
      }
   }
}