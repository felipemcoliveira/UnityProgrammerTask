namespace UnityProgrammerTask.Gameplay
{
   public class TextOnlyGameTextStyler : IGameTextStyler
   {
      public string Positive(string text)
      {
         return text;
      }

      public string Negative(string text)
      {
         return text;
      }
   }
}