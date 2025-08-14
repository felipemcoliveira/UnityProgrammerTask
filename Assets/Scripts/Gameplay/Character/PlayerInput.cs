using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace UnityProgrammerTask.Gameplay
{
   public class PlayerInput : MonoBehaviour
   {
      public Character PlayerCharacter => Character.PlayerCharacter;

      [SerializeField]
      private InputAction m_MoveAction;

      private int m_EnvironmentLayer;

      private void OnEnable()
      {
         m_MoveAction.Enable();
         m_MoveAction.performed += OnMovePerformed;

         m_EnvironmentLayer = LayerMask.GetMask("Environment");
      }

      private void OnMovePerformed(InputAction.CallbackContext context)
      {
         if (PlayerCharacter == null || !PlayerCharacter.IsAlive || EventSystem.current.IsPointerOverGameObject())
            return;

         Vector2 mousePosition = Mouse.current.position.ReadValue();
         Ray ray = Camera.main.ScreenPointToRay(mousePosition);

         if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, m_EnvironmentLayer))
            PlayerCharacter.SetDestination(hit.point);
      }
   }
}