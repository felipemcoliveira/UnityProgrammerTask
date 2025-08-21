using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityProgrammerTaskEditor
{
   [InitializeOnLoad]
   public static class PlayFromGameEntry
   {
      private const string k_SnapshotKey = "PlayFromGameEntry_Snapshot_v1";
      private const string k_TargetSceneName = "GameEntry";

      [Serializable]
      private class SceneSnapshot
      {
         public string[] Paths;
         public string ActivePath;
      }

      static PlayFromGameEntry()
      {
         EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
      }

      private static void OnPlayModeStateChanged(PlayModeStateChange change)
      {
         switch (change)
         {
            case PlayModeStateChange.ExitingEditMode:
               if (!SaveSnapshot())
               {
                  EditorApplication.isPlaying = false;
                  return;
               }

               if (!OpenGameEntryScene())
               {
                  EditorApplication.isPlaying = false;
                  return;
               }

               break;

            case PlayModeStateChange.EnteredEditMode:
               RestoreSnapshot();
               break;
         }
      }

      private static bool SaveSnapshot()
      {
         if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return false;

         int count = EditorSceneManager.sceneCount;
         string[] paths = new string[count];

         for (int i = 0; i < count; i++)
            paths[i] = EditorSceneManager.GetSceneAt(i).path;

         string active = SceneManager.GetActiveScene().path;
         SceneSnapshot snap = new()
         {
            Paths = paths,
            ActivePath = active
         };

         string json = JsonUtility.ToJson(snap);
         EditorPrefs.SetString(k_SnapshotKey, json);
         return true;
      }

      private static bool OpenGameEntryScene()
      {
         string[] guids = AssetDatabase.FindAssets($"t:Scene {k_TargetSceneName}");
         string path = guids.Select(AssetDatabase.GUIDToAssetPath)
                         .FirstOrDefault(p => System.IO.Path.GetFileNameWithoutExtension(p) == k_TargetSceneName);

         if (string.IsNullOrEmpty(path))
         {
            EditorUtility.DisplayDialog("Play From Game Entry",
                $"Scene named \"{k_TargetSceneName}\" not found. Rename your entry scene or update TargetSceneName.", "OK");
            return false;
         }

         try
         {
            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            return true;
         }
         catch (Exception e)
         {
            Debug.LogError($"Failed to open {path}: {e.Message}");
            return false;
         }
      }

      private static void RestoreSnapshot()
      {
         string json = EditorPrefs.GetString(k_SnapshotKey, "");
         if (string.IsNullOrEmpty(json))
            return;

         EditorPrefs.DeleteKey(k_SnapshotKey);

         SceneSnapshot snap = JsonUtility.FromJson<SceneSnapshot>(json);
         if (snap?.Paths == null || snap.Paths.Length == 0)
            return;

         try
         {
            Scene first = EditorSceneManager.OpenScene(snap.Paths[0], OpenSceneMode.Single);
            for (int i = 1; i < snap.Paths.Length; i++)
            {
               if (!string.IsNullOrEmpty(snap.Paths[i]))
                  EditorSceneManager.OpenScene(snap.Paths[i], OpenSceneMode.Additive);
            }

            if (!string.IsNullOrEmpty(snap.ActivePath))
            {
               for (int i = 0; i < EditorSceneManager.sceneCount; i++)
               {
                  Scene s = EditorSceneManager.GetSceneAt(i);
                  if (s.path == snap.ActivePath)
                  {
                     SceneManager.SetActiveScene(s);
                     break;
                  }
               }
            }
         }
         catch (Exception e)
         {
            Debug.LogException(e);
         }
      }
   }
}