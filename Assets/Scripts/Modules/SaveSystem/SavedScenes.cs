using System.IO;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.SceneManagement;

namespace UnityProgrammerTask.Core
{
   public class SavedScenes : IGameSaveSection
   {
      public void Load(BinaryReader stream, int size)
      {
         int sceneCount = size / UnsafeUtility.SizeOf<int>();
         for (int i = 0; i < sceneCount; i++)
         {
            int sceneBuildIndex = stream.ReadInt32();
            Scene scene = SceneManager.GetSceneByBuildIndex(sceneBuildIndex);

            if (scene.isLoaded)
               continue;

            SceneManager.LoadScene(sceneBuildIndex, LoadSceneMode.Additive);
         }
      }

      public void Save(BinaryWriter stream)
      {
         int sceneCount = SceneManager.sceneCount;
         for (int i = 0; i < sceneCount; i++)
         {
            int sceneBuildIndex = SceneManager.GetSceneAt(i).buildIndex;
            stream.Write(sceneBuildIndex);
         }
      }
   }
}