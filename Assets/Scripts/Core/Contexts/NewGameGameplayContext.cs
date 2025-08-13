using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityProgrammerTask.Gameplay;

namespace UnityProgrammerTask.Core
{
   [Serializable]
   [GameContext(GameContextID.NewGameGameplay)]
   public class NewGameGameplayContext : GameplayContext
   {
      private static readonly Logger s_Logger = Logger.Create("NewGameGameplay", "#62FF8E");

      [SerializeField]
      private AssetReferenceGameObject m_CharacterPrefabAssetReference;

      public override void OnEnter()
      {
         base.OnEnter();

         SaveSystem.StartNewSave();
      }

      protected override async Awaitable Startup()
      {
         //AsyncOperation uiSceneLoadOp = SceneManager.LoadSceneAsync("GameplayUI", LoadSceneMode.Additive);

         await SceneManager.LoadSceneAsync("FirstLevel", LoadSceneMode.Additive);
         Scene firstLevelScene = SceneManager.GetSceneByName("FirstLevel");

         SceneManager.SetActiveScene(firstLevelScene);

         // Load the character prefab from the asset reference
         AsyncOperationHandle<GameObject> instantiateOp = m_CharacterPrefabAssetReference.InstantiateAsync();
         GameObject characterGameObject = await instantiateOp.Task;

         if (!characterGameObject.TryGetComponent(out Character character))
         {
            s_Logger.LogFormatError("Character component not found on the instantiated GameObject.");
            return;
         }

         //await uiSceneLoadOp;
      }
   }
}