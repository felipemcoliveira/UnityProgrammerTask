namespace UnityProgrammerTask.Gameplay
{
   public class GameTextStylerLibrary
   {
      public static GameTextStyler DefaultStyler { get; } = new();
      public static TextOnlyGameTextStyler TextOnlyStyler { get; } = new();
   }
}