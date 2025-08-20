namespace UnityProgrammerTask.Gameplay
{
   public class GameTextStyler : TextOnlyGameTextStyler
   {
      public override string Positive(string text)
      {
         string innerText = base.Positive(text);
         if (string.IsNullOrEmpty(innerText))
            return string.Empty;

         return $"<color=#19fc7f>{innerText}</color>";
      }

      public override string Negative(string text)
      {
         string innerText = base.Negative(text);
         if (string.IsNullOrEmpty(innerText))
            return string.Empty;

         return $"<color=#eb4034>{innerText}</color>";
      }
   }
}