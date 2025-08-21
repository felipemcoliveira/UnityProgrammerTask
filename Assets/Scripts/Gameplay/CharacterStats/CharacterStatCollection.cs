using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityProgrammerTask.Gameplay
{
   [HideMonoScript]
   public class CharacterStatCollection : MonoBehaviour, IEnumerable<CharacterStat>
   {
      [ShowInInspector, Searchable, DictionaryDrawerSettings(IsReadOnly = true)]
      private Dictionary<CharacterStatID, CharacterStat> m_Stats = new();

      private HashSet<CharacterStatID> m_ActiveStatIDs = new();

      public CharacterStat GetOrCreateStat(CharacterStatID id)
      {
         if (TryGetStat(id, out CharacterStat existingStat))
            return existingStat;

         CharacterStat newStat = new(id, 0f);

         newStat.StatActivated += OnStatActiveStateChanged;
         newStat.StatDeactivated += OnStatActiveStateChanged;

         m_Stats.Add(id, newStat);

         if (newStat.IsActive)
            m_ActiveStatIDs.Add(id);

         return newStat;
      }

      public bool TryGetStat(CharacterStatID id, out CharacterStat stat)
      {
         if (m_Stats.TryGetValue(id, out stat))
            return true;

         stat = null;
         return false;
      }

      public bool TryGetActiveStat(CharacterStatID id, out CharacterStat stat)
      {
         if (m_ActiveStatIDs.Contains(id) && m_Stats.TryGetValue(id, out stat))
            return true;

         stat = null;
         return false;
      }

      private void OnStatActiveStateChanged(CharacterStat stat)
      {
         if (stat.IsActive)
         {
            m_ActiveStatIDs.Add(stat.ID);
            return;
         }

         m_ActiveStatIDs.Remove(stat.ID);
      }

      public bool IsStatActive(CharacterStatID id)
      {
         return m_ActiveStatIDs.Contains(id);
      }

      public IEnumerator<CharacterStat> GetEnumerator()
      {
         foreach (CharacterStatID statID in m_ActiveStatIDs)
            yield return m_Stats[statID];
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         return GetEnumerator();
      }
   }
}