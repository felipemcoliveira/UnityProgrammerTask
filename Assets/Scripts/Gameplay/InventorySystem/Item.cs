using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityProgrammerTask.Gameplay
{
   [HideMonoScript]
   public class Item : ScriptableObject
   {
      public GUID ID => m_ID;

      public virtual bool IsConsumable => false;

      public int MaxStackSize => m_MaxStackSize;

      public int UnitPriceInGold => m_UnitPriceInGold;

      public string ItemName => m_ItemName;

      public Sprite ItemIcon => m_ItemIcon;

      [TitleGroup("General"), PropertyOrder(-100), DisplayAsString]
      [SerializeField]
      private GUID m_ID;

      [TitleGroup("General", Alignment = TitleAlignments.Left)]
      [VerticalGroup("General/Left"), LabelText("Name"), PropertyOrder(-50), Required]
      [SerializeField]
      private string m_ItemName;

      [VerticalGroup("General/Left"), LabelText("Icon"), PreviewField(64, ObjectFieldAlignment.Left), HideLabel]
      [SerializeField]
      private Sprite m_ItemIcon;

      [VerticalGroup("General/Right"), LabelText("Price"), MinValue(0), SuffixLabel("gold", true)]
      [SerializeField]
      private int m_UnitPriceInGold;

      [VerticalGroup("General/Right"), LabelText("Max Stack"), MinValue(1)]
      [SerializeField]
      private int m_MaxStackSize = 1;

      [BoxGroup("Description")]
      [HideLabel, MultiLineProperty(3)]
      [SerializeField]
      private string m_StaticDescription;

      [TitleGroup("World Representation", Order = 1000)]
      [AssetsOnly, Required]
      [SerializeField]
      private ItemRepresentation m_WorldRepresentationPrefab;

      private void OnValidate()
      {
#if UNITY_EDITOR

         if (UnityEditor.EditorUtility.IsPersistent(this) && m_ID == GUID.Empty)
            m_ID = GUID.Generate();

#endif
      }

      public virtual void OnItemAddedToInventory(Character character, int quantity)
      {
         // Default implementation does nothing
      }

      public virtual void OnItemRemovedFromInventory(Character character, int quantity)
      {
         // Default implementation does nothing
      }

      public virtual void OnItemConsumed(Character character, int quantity)
      {
         // Default implementation does nothing
      }

      public virtual bool TryConsume(Character character, ref int quantity)
      {
         return true;
      }

      public virtual string GetDescription(IGameTextStyler textStyler)
      {
         return GetStaticDescription();
      }

      protected string GetStaticDescription()
      {
         return m_StaticDescription;
      }

      public void CreateWorldRepresentation(Vector3 position, int quantity)
      {
         if (m_WorldRepresentationPrefab != null)
         {
            ItemRepresentation representation = Instantiate(m_WorldRepresentationPrefab);
            representation.transform.position = position;
            representation.Initialize(this, quantity);
         }
      }
   }
}