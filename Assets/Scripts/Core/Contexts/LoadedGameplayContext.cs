namespace UnityProgrammerTask.Core
{
   [GameContext(GameContextID.LoadedGameGameplay)]
   public class LoadedGameGameplayContext : GameplayContext
   {
      public override void OnEnter()
      {
         SaveSystem.LoadSave(GetSaveFilePath());
      }

      public override GameContextID Update()
      {
         HandleSaveGameMessage();

         return StateID;
      }
   }
}