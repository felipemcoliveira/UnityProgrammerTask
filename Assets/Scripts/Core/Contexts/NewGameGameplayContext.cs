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
      [SerializeField]
      private AssetReferenceGameObject m_CharacterPrefabAssetReference;

      [SerializeField]
      private Item m_HealPotionItem;

      [SerializeField]
      private Item m_PoisonItem;

      [SerializeField]
      private Equipment m_Equipment;

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

         character.Inventory.AddItem(m_PoisonItem, 10);
         character.Inventory.AddItem(m_HealPotionItem, 20);
         character.Inventory.AddItem(m_Equipment, 1);

         CharacterEquipment characterEquipment = character.GetComponent<CharacterEquipment>();
         characterEquipment.Equip(m_Equipment);

         await LoadGameplayScene("GameplayUI");
      }
   }
}