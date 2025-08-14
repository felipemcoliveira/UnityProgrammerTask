using UnityEngine;

namespace UnityProgrammerTask.Presentation
{
   public class Toast : MonoBehaviour
   {
      public static Toast Instance { get; private set; }

      [SerializeField]
      private ToastMessage m_ToastMessagePrefab;

      private void Awake()
      {
         Instance = this;
      }

      public void ShowMessage(string message)
      {
         if (m_ToastMessagePrefab == null)
         {
            Debug.LogError("ToastMessage prefab is not assigned.");
            return;
         }
         ToastMessage toastMessage = Instantiate(m_ToastMessagePrefab, transform);
         toastMessage.Initialize(message);
      }
   }
}