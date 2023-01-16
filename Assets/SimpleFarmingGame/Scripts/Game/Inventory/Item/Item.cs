using UnityEngine;

namespace SimpleFarmingGame.Game
{
    public class Item : MonoBehaviour
    {
        public int ItemID;
        private BoxCollider2D m_BoxCollider2D;

        private SpriteRenderer m_ItemSpriteRenderer;
        public ItemDetails ItemDetails { get; private set; }

        private void Awake()
        {
            m_BoxCollider2D = GetComponent<BoxCollider2D>();
            m_ItemSpriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            if (ItemID != 0)
            {
                Init(ItemID);
            }
        }

        /// <summary>
        /// 初始化物品
        /// </summary>
        /// <param name="itemID">物品ID</param>
        public void Init(int itemID)
        {
            ItemID = itemID;
            ItemDetails = InventoryManager.Instance.GetItemDetails(ItemID);

            if (ItemDetails == null) return;

            m_ItemSpriteRenderer.sprite = ItemDetails.ItemIconOnWorld == null
                ? ItemDetails.ItemIcon
                : ItemDetails.ItemIconOnWorld;

            #region 调整碰撞体大小

            Bounds spriteBounds = m_ItemSpriteRenderer.sprite.bounds;
            Vector2 newSize = new Vector2(spriteBounds.size.x, spriteBounds.size.y);
            m_BoxCollider2D.size = newSize;
            m_BoxCollider2D.offset = new Vector2(0, spriteBounds.center.y);

            #endregion

            if (ItemDetails.ItemType != ItemType.Reapable) return; // 如果该物品类型为可收获的
            gameObject.AddComponent<ReapItem>();
            gameObject.GetComponent<ReapItem>().InitCropDetails(ItemID);
            gameObject.AddComponent<ItemInteractive>();
        }
    }
}