using System;
using System.IO;
using UnityEngine;

namespace UnityProgrammerTask
{
   [Serializable]
   public struct GUID : IEquatable<GUID>, IComparable<GUID>
   {
      public const int kSizeInBytes = 16;

      public static readonly GUID Empty = default;

      [SerializeField]
      private long m_High;

      [SerializeField]
      private long m_Low;

      public GUID(string guidString)
      {
         if (string.IsNullOrEmpty(guidString))
            throw new ArgumentException("GUID string cannot be null or empty.", nameof(guidString));

         if (!System.Guid.TryParse(guidString, out System.Guid guid))
            throw new FormatException($"Invalid GUID format: {guidString}");

         this = new GUID(guid);
      }

      public GUID(System.Guid guid)
      {
         Span<byte> bytes = stackalloc byte[16];
         guid.TryWriteBytes(bytes);

         m_High = BitConverter.ToInt64(bytes[..8]);
         m_Low = BitConverter.ToInt64(bytes.Slice(8, 8));
      }

      public static GUID Generate()
      {
         return new GUID(Guid.NewGuid());
      }

      public System.Guid ToSystemGuid()
      {
         Span<byte> bytes = stackalloc byte[16];
         BitConverter.TryWriteBytes(bytes[..8], m_High);
         BitConverter.TryWriteBytes(bytes.Slice(8, 8), m_Low);
         return new System.Guid(bytes);
      }

      public override string ToString()
      {
         return ToSystemGuid().ToString();
      }

      public static implicit operator GUID(System.Guid guid)
      {
         return new GUID(guid);
      }

      public static implicit operator System.Guid(GUID guid)
      {
         return guid.ToSystemGuid();
      }

      public override bool Equals(object obj)
      {
         if (obj is GUID other)
         {
            return m_High == other.m_High && m_Low == other.m_Low;
         }
         return false;
      }
      public bool Equals(GUID other)
      {
         return m_High == other.m_High && m_Low == other.m_Low;
      }

      public override int GetHashCode()
      {
         return HashCode.Combine(m_High, m_Low);
      }

      public int CompareTo(GUID other)
      {
         if (m_High != other.m_High)
            return m_High.CompareTo(other.m_High);
         return m_Low.CompareTo(other.m_Low);
      }

      public static bool operator ==(GUID left, GUID right)
      {
         return left.Equals(right);
      }

      public static bool operator !=(GUID left, GUID right)
      {
         return !(left == right);
      }

      public static GUID Read(BinaryReader reader)
      {
         if (reader == null)
            throw new ArgumentNullException(nameof(reader), "Reader cannot be null.");

         long high = reader.ReadInt64();
         long low = reader.ReadInt64();
         return new GUID { m_High = high, m_Low = low };
      }

      public void Write(BinaryWriter writer)
      {
         if (writer == null)
            throw new ArgumentNullException(nameof(writer), "Writer cannot be null.");

         writer.Write(m_High);
         writer.Write(m_Low);
      }
   }
}