using DG.Tweening;
using EPOOutline;
using UnityEngine;
using UnityProgrammerTask.Presentation;

namespace UnityProgrammerTask.Gameplay
{
   public class ItemRepresentation : MonoBehaviour
   {
      public Item Item => m_Item;
      public int Quantity => m_Quantity;

      private Item m_Item;
      private int m_Quantity;
      private int m_CreatedFrame;

      private void Awake()
      {
         m_CreatedFrame = Time.frameCount;

         Outlinable outlinable = GetComponent<Outlinable>();

         Color originalColor = outlinable.OutlineParameters.Color;
         outlinable.OutlineParameters.Color = originalColor * 0.5f;

         outlinable.OutlineParameters.DOColor(originalColor, 1)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
      }

      public void Initialize(Item item, int quantity)
      {
         m_Item = item;
         m_Quantity = quantity;
      }

      private void OnTriggerEnter(Collider other)
      {
         if (Time.frameCount - m_CreatedFrame < 5)
            return; // Ignore collisions in the first five frames to avoid immediate pickup

         if (other.TryGetComponent(out Character character))
         {
            m_Quantity -= character.Inventory.AddItem(m_Item, m_Quantity);

            if (m_Quantity > 0)
            {
               bool singleItem = m_Quantity == 1;

               string message = singleItem
                  ? $"Picked up {m_Item.ItemName}."
                  : $"Picked up {m_Quantity}x {m_Item.ItemName}.";

               Toast.Instance.ShowMessage(message);
            }

            if (m_Quantity == 0)
               Destroy(gameObject);
         }
      }
   }
}
