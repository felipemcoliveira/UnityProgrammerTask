using UnityEngine;
using UnityEngine.InputSystem;

namespace UnityProgrammerTask.Presentation
{
   public class InGameOptionsPanel : MonoBehaviour
   {
      [SerializeField]
      private InputAction m_ToggleVisibilityAction;

      private PanelVisibility m_PanelVisibility;

      private void Awake()
      {

         m_PanelVisibility = GetComponent<PanelVisibility>();

         m_ToggleVisibilityAction.Enable();
         m_ToggleVisibilityAction.performed += OnToggleVisibility;
      }

      private void OnToggleVisibility(InputAction.CallbackContext context)
      {
         m_PanelVisibility.ToggleVisibility();
      }

      public void HandleSaveGame()
      {
         MessageQueue.Post<SaveGameMessage>();
      }

      public void HandleReturnMainMenu()
      {
         MessageQueue.Post<ReturnToMainMenuMessage>();
      }

      public void HandleQuitGame()
      {
         MessageQueue.Post<QuitGameMessage>();
      }
   }
}