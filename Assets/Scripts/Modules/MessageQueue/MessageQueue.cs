using System.Collections.Generic;
using UnityEngine.Assertions;

namespace UnityProgrammerTask
{
   /// <summary>
   /// This code wasn't written for this project, it's a utility class that
   /// I use in many of my Unity projects.
   /// </summary>
   public class MessageQueue
   {
      private static readonly Logger s_Logger = Logger.Create(nameof(MessageQueue), "#deaf37");

      private static readonly List<Message> s_MessageQueue = new();

      public static bool IsInQueue(Message messafge)
      {
         Assert.IsNotNull(messafge, "Message cannot be null");
         return s_MessageQueue.Contains(messafge);
      }

      public static T Post<T>() where T : Message, new()
      {
         T message = new();
         Post(message);
         return message;
      }

      public static void Post(Message message)
      {
         Assert.IsNotNull(message, "Message cannot be null");

         s_Logger.LogFormatInfo("Enqueue {0}", message.ToString());

         s_MessageQueue.Add(message);
      }

      public static bool TryPoll<T>() where T : Message
      {
         return TryPoll(out T _);
      }

      public static bool TryPoll<T>(out T message) where T : Message
      {
         for (int i = 0; i < s_MessageQueue.Count; i++)
         {
            if (s_MessageQueue[i] is T message1)
            {
               s_Logger.LogFormatInfo("Poll    {0}", message1.ToString());

               s_MessageQueue.RemoveAt(i);
               message = message1;
               return true;
            }
         }

         message = null;
         return false;
      }

      public static void LogUnhandledMessages()
      {
         if (s_MessageQueue.Count == 0)
            return;

         s_Logger.LogFormatWarning("Unhandled messages in the queue:");

         foreach (Message message in s_MessageQueue)
            s_Logger.LogFormatWarning(message.ToString());
      }
   }
}