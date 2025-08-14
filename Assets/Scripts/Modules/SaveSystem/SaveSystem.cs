using System;
using System.IO;

namespace UnityProgrammerTask
{
   public class SaveSystem
   {
      public static GameSave ActiveSave { get; private set; }

      public static void StartNewSave()
      {
         ActiveSave = new GameSave();
         ActiveSave.Initialize();
      }

      public static void LoadSave(string filePath)
      {
         ActiveSave = new GameSave();

         FileStream file = File.OpenRead(filePath);
         ActiveSave.Initialize(file);
      }

      public static void Save(string filePath)
      {
         if (ActiveSave == null)
            throw new InvalidOperationException("No active save game to save.");

         if (string.IsNullOrEmpty(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

         ActiveSave.FreeFile();

         using FileStream file = File.Create(filePath);
         using BinaryWriter writer = new(file);

         ActiveSave.Write(writer);
      }

      public static void DisposeActiveSave()
      {
         ActiveSave?.Dispose();
         ActiveSave = null;
      }
   }
}
