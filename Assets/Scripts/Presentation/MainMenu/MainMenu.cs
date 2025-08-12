using UnityEngine;

namespace UnityProgrammerTask.Presentation
{
   public class MainMenu : MonoBehaviour
   {
      public void HandleStartNewGame()
      {
         MessageQueue.Post<StartNewGameMessage>();
      }

      public void HandleQuitGame()
      {
         MessageQueue.Post<QuitGameMessage>();
      }
   }
}
