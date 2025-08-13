using UnityEngine;

namespace UnityProgrammerTask.Gameplay
{
   public class Character : MonoBehaviour
   {
      private void Update()
      {
         if (Input.GetKeyDown(KeyCode.Space))
         {
            MessageQueue.Post<SaveGameMessage>();
         }
      }
   }
}
