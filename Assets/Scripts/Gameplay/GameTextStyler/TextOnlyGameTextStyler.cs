namespace UnityProgrammerTask.Gameplay
{
   public class TextOnlyGameTextStyler : IGameTextStyler
   {
      public virtual string Stat(CharacterStatID statID)
      {
         // TODO: Implement a more sophisticated stat name retrieval if needed.
         return statID.ToString();
      }

      public virtual string Positive(string text)
      {
         if (string.IsNullOrEmpty(text))
            return string.Empty;

         return text;
      }

      public virtual string Negative(string text)
      {
         if (string.IsNullOrEmpty(text))
            return string.Empty;

         return text;
      }
   }
}