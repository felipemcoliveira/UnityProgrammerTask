using System;
using System.Collections.Generic;
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
      [SerializeField]
      private AssetReferenceGameObject m_CharacterPrefabAssetReference;

      [SerializeField]
      private int m_SpawnItemCount = 10;

      public override void OnEnter()
      {
         base.OnEnter();

         SaveSystem.StartNewSave();
      }

      protected override async Awaitable Startup()
      {
         await LoadGameplayScene("FirstLevel");
         Scene firstLevelScene = SceneManager.GetSceneByName("FirstLevel");

         SceneManager.SetActiveScene(firstLevelScene);

         // Load the character prefab from the asset reference
         AsyncOperationHandle<GameObject> instantiateOp = m_CharacterPrefabAssetReference.InstantiateAsync();
         GameObject characterGameObject = await instantiateOp.Task;

         if (!characterGameObject.TryGetComponent(out Character character))
         {
            Debug.LogError("Character component not found on the instantiated GameObject.");
            return;
         }

         character.SetAsPlayerCharacter();

         // I don't like this approach, but time is getting short.
         GameObject itemSpawnPointsContainer = GameObject.Find("ItemSpawnPoints");
         SpawnItems(itemSpawnPointsContainer);

         await LoadGameplayScene("GameplayUI");
      }

      private void SpawnItems(GameObject itemSpawnPointsContainer)
      {
         List<Vector3> spawnPosition = new();
         foreach (Transform child in itemSpawnPointsContainer.transform)
            spawnPosition.Add(child.position);

         for (int i = 0; i < m_SpawnItemCount; i++)
         {
            int randomIndex = UnityEngine.Random.Range(0, spawnPosition.Count);
            Vector3 position = spawnPosition[randomIndex];
            spawnPosition.RemoveAt(randomIndex);

            int randomItemIndex = UnityEngine.Random.Range(0, ItemLibrary.Instance.Count);
            Item item = ItemLibrary.Instance[randomItemIndex];

            int quantity = UnityEngine.Random.Range(1, item.MaxStackSize + 1);
            item.CreateWorldRepresentation(position, quantity);
         }
      }
   }
}