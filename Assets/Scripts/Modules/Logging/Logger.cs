using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using UnityEngine.Pool;
using Debug = UnityEngine.Debug;

namespace UnityProgrammerTask
{
   /// <summary>
   /// This code wasn't written for this project, it's a utility class that
   /// I use in many of my Unity projects.
   /// </summary>
   public struct Logger
   {
      private const int k_PrefixWidth = 22;

      private static readonly string s_WhiteSpace = new(' ', k_PrefixWidth);
      private static readonly StringBuilder s_StringBuilder = new(256);

      private string m_MessageFormat;
      private string m_NonFirstLineMessageFormat;

      public static Logger Create(string prefix, string color)
      {
         prefix = $"[{prefix}]";
         prefix += new string(' ', Mathf.Max(k_PrefixWidth - prefix.Length, 0));

         return new Logger
         {
            m_MessageFormat = $"<size=12><color={color}>{prefix}{{0}}</color></size><size=10>",
            m_NonFirstLineMessageFormat = $"<size=12><color={color}>{s_WhiteSpace}{{0}}</color></size><size=10>",
         };
      }

      [HideInCallstack]
      public readonly void Log(LogType logType, string message, Object context)
      {
         if (!Debug.unityLogger.logEnabled || !Debug.unityLogger.IsLogTypeAllowed(logType))
            return;

         if (message.IndexOf('\n') == -1)
         {
            s_StringBuilder.Clear();
            s_StringBuilder.AppendFormat(m_MessageFormat, message);
            s_StringBuilder.AppendLine();
            Debug.unityLogger.Log(logType, (object)s_StringBuilder.ToString(), context);
            return;
         }

         using (ListPool<string>.Get(out List<string> lines))
         {
            SplitLines(message, lines);

            s_StringBuilder.Clear();

            for (int i = 0; i < lines.Count; i++)
            {
               string line = lines[i];

               string messageFormat = i == 0 ? m_MessageFormat : m_NonFirstLineMessageFormat;

               s_StringBuilder.AppendFormat(messageFormat, line);
               s_StringBuilder.AppendLine();
            }

            s_StringBuilder.AppendLine();

            Debug.unityLogger.Log(logType, s_StringBuilder.ToString());
         }
      }

      private readonly void SplitLines(string message, List<string> output)
      {
         int start = 0;
         int length = message.Length;

         for (int i = 0; i < length; ++i)
         {
            if (message[i] == '\n')
            {
               int lineLength = i - start;
               if (lineLength > 0 && message[i - 1] == '\r')
                  lineLength--;

               output.Add(message.Substring(start, lineLength));
               start = i + 1;
            }
         }

         if (start < length)
            output.Add(message[start..]);
      }

      [HideInCallstack, Conditional("DEBUG")]
      public readonly void LogError(string message, Object context = null)
      {
         Log(LogType.Error, message, context);
      }

      [HideInCallstack, Conditional("DEBUG")]
      public readonly void LogWarning(string message, Object context = null)
      {
         Log(LogType.Warning, message, context);
      }

      [HideInCallstack, Conditional("DEBUG")]
      public readonly void LogInfo(string message, Object context = null)
      {
         Log(LogType.Log, message, context);
      }

      [HideInCallstack, Conditional("DEBUG")]
      public readonly void LogFormat(LogType logType, Object context, string format, params object[] args)
      {
         if (!Debug.unityLogger.logEnabled || !Debug.unityLogger.IsLogTypeAllowed(logType))
            return;

         Log(logType, string.Format(format, args), context);
      }

      [HideInCallstack, Conditional("DEBUG")]
      public readonly void LogFormatError(string format, params object[] args)
      {
         LogFormat(LogType.Error, null, format, args);
      }

      [HideInCallstack, Conditional("DEBUG")]
      public readonly void LogFormatError(Object context, string format, params object[] args)
      {
         LogFormat(LogType.Error, context, format, args);
      }

      [HideInCallstack, Conditional("DEBUG")]
      public readonly void LogFormatWarning(string format, params object[] args)
      {
         LogFormat(LogType.Warning, null, format, args);
      }

      [HideInCallstack, Conditional("DEBUG")]
      public readonly void LogFormatWarning(Object context, string format, params object[] args)
      {
         LogFormat(LogType.Warning, context, format, args);
      }

      [HideInCallstack, Conditional("DEBUG")]
      public readonly void LogFormatInfo(string format, params object[] args)
      {
         LogFormat(LogType.Log, null, format, args);
      }

      [HideInCallstack, Conditional("DEBUG")]
      public readonly void LogFormatInfo(Object context, string format, params object[] args)
      {
         LogFormat(LogType.Log, context, format, args);
      }
   }
}