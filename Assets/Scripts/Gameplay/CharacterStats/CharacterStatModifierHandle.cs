namespace UnityProgrammerTask.Gameplay
{
   public readonly struct CharacterStatModifierHandle
   {
      public int Handle { get; }
      public CharacterStat TargetStat { get; }
      public CharacterStatModifierType Type { get; }

      public CharacterStatModifierHandle(int handle, CharacterStat targetStat, CharacterStatModifierType type)
      {
         Handle = handle;
         TargetStat = targetStat;
         Type = type;
      }
   }
}