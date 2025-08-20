using Newtonsoft.Json;
using UnityEngine;

namespace UnityProgrammerTask
{
   /// <summary>
   /// This code wasn't written for this project, it's a utility class that
   /// I use in many of my Unity projects.
   /// </summary>
   public class Message
   {
      private int CreationFrame { get; } = Time.frameCount;
      private float CreationTime { get; } = Time.realtimeSinceStartup;

      public override string ToString()
      {
         string json = JsonConvert.SerializeObject(this, Formatting.Indented);
         string typeName = GetType().Name;
         return $"{typeName} (Frame {CreationFrame}-{CreationTime:F2}s)\n{json}";
      }
   }
}