namespace UnityProgrammerTask.Gameplay
{
   public class GameTextStyler : IGameTextStyler
   {
      public string Positive(string text)
      {
         return $"<color=#19fc7f>{text}</color>";
      }

      public string Negative(string text)
      {
         return $"<color=#eb4034>{text}</color>";
      }
   }
}
