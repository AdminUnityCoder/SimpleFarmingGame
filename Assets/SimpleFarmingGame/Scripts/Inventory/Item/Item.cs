using SFG.CropSystem;
using UnityEngine;

namespace SFG.InventorySystem
{
    public class Item : MonoBehaviour
    {
        public int ItemID;

        private SpriteRenderer m_ItemSpriteRenderer;
        private BoxCollider2D m_BoxCollider2D;
        public ItemDetails ItemDetails { get; private set; }

        private void Awake()
        {
            m_ItemSpriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
            m_BoxCollider2D = GetComponent<BoxCollider2D>();
        }

        private void Start()
        {
            if (ItemID != 0)
            {
                Init(ItemID);
            }
        }

        public void Init(int itemID)
        {
            ItemID = itemID;
            ItemDetails = InventoryManager.Instance.GetItemDetails(ItemID);

            if (ItemDetails == null) return;

            m_ItemSpriteRenderer.sprite = ItemDetails.ItemIconOnWorld == null
                ? ItemDetails.ItemIcon
                : ItemDetails.ItemIconOnWorld;
            ModifyColliderSize();

            if (ItemDetails.ItemType == ItemType.Reapable)
            {
                gameObject.AddComponent<ReapItem>();
                gameObject.GetComponent<ReapItem>().InitCropDetails(ItemID);
                gameObject.AddComponent<ItemInteractive>();
            }
        }

        private void ModifyColliderSize()
        {
            Bounds spriteBounds = m_ItemSpriteRenderer.sprite.bounds;
            Vector2 newSize = new Vector2(spriteBounds.size.x, spriteBounds.size.y);
            m_BoxCollider2D.size = newSize;
            m_BoxCollider2D.offset = new Vector2(0, spriteBounds.center.y);
        }
    }
}