using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SimpleFarmingGame.Game
{
    public enum SlotType { Bag, Box, Shop }

    public sealed class SlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        /*
         * 因为需要一开始将 PlayerBag 隐藏，如果不使用组件拖拽的方式进行组件的获取
         * 而是在 Awake 中获取组件，会导致获取不到已经隐藏的物体身上的组件
         */
        [SerializeField, Header("Component")] private Button Button;
        [SerializeField] private Image ItemSprite;
        [SerializeField] private TextMeshProUGUI AmountTextUI;
        [SerializeField] private Image SlotSelectHighlight;
        [SerializeField] private SlotType m_SlotType;
        public int SlotIndex;
        public bool IsSelected;
        private int m_ItemAmount;
        private ItemDetails m_ItemDetails;
        public ItemDetails ItemDetails => m_ItemDetails;
        public SlotType SlotType => m_SlotType;
        public InventoryUI m_InventoryUI => GetComponentInParent<InventoryUI>();

        private InventoryLocation Location
        {
            get
            {
                return m_SlotType switch
                {
                    SlotType.Bag => InventoryLocation.PlayerBag
                  , SlotType.Box => InventoryLocation.StorageBox
                  , _ => InventoryLocation.PlayerBag
                    // FIXME: 后续应该给Shop添加对应的InventoryLocation,即使Shop不是Inventory
                };
            }
        }

        private void Start()
        {
            IsSelected = false;
            if (m_ItemDetails == null)
            {
                SetSlotEmpty();
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (m_ItemDetails == null) return;
            IsSelected = true;
            m_InventoryUI.DisplaySlotHighlight(SlotIndex, m_SlotType);
            m_InventoryUI.ShowDragItemImage(ItemSprite.sprite);
        }

        public void OnDrag(PointerEventData eventData)
        {
            m_InventoryUI.SetDragItemImagePosition(Input.mousePosition);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            m_InventoryUI.HideDragItemImage();

            if (eventData.pointerCurrentRaycast.gameObject != null)
            {
                if (eventData.pointerCurrentRaycast.gameObject.TryGetComponent(out SlotUI targetSlot) == false) return;

                int targetIndex = targetSlot.SlotIndex;

                // BUG: 这里需要重构
                if (IsSwapInPlayerBag(targetSlot))
                {
                    InventoryManager.Instance.SwapItemsWithinPlayerBag(this.SlotIndex, targetIndex);
                }
                else if (IsBuyFromStore(targetSlot))
                {
                    EventSystem.CallShowTransactionUIEvent(m_ItemDetails, false);
                }
                else if (IsSellToStore(targetSlot))
                {
                    EventSystem.CallShowTransactionUIEvent(m_ItemDetails, true);
                }
                else if (IsBox(targetSlot) || IsSwapInStorageBox(targetSlot))
                {
                    // 跨背包数据交换物品
                    InventoryManager.Instance.SwapItem
                    (
                        Location
                      , SlotIndex
                      , targetSlot.Location
                      , targetSlot.SlotIndex
                    );
                }

                m_InventoryUI.CancelDisplayAllSlotHighlight();
            }

            #region Disable this code here because distance cannot be determined.

            // else // Test throwing items on the ground
            // {
            //     if (ItemDetails.CanDropped)
            //     {
            //         // Screen Position To World Position
            //         if (Camera.main != null)
            //         {
            //             Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(
            //                 Input.mousePosition.x
            //               , Input.mousePosition.y
            //               , -Camera.main.transform.Position.z));
            //
            //             EventSystem.CallInstantiateItemInScene(ItemDetails.ItemID, worldPosition);
            //         }
            //     }
            // }

            #endregion
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (m_ItemDetails == null) return;
            IsSelected = !IsSelected;

            m_InventoryUI.DisplaySlotHighlight(SlotIndex, m_SlotType);

            if (m_SlotType == SlotType.Bag)
            {
                EventSystem.CallItemSelectedEvent(m_ItemDetails, IsSelected);
            }
        }

        public void SetSlotEmpty()
        {
            if (IsSelected)
            {
                IsSelected = false;
                m_InventoryUI.CancelDisplayAllSlotHighlight();
                EventSystem.CallItemSelectedEvent(m_ItemDetails, IsSelected);
            }

            m_ItemDetails = null;
            ItemSprite.enabled = false;
            AmountTextUI.text = string.Empty;
            Button.interactable = false;
        }

        public void SetSlot(ItemDetails itemDetails, int amount)
        {
            m_ItemDetails = itemDetails;
            m_ItemAmount = amount;
            ItemSprite.enabled = true;
            ItemSprite.sprite = m_ItemDetails.ItemIcon;
            AmountTextUI.text = m_ItemAmount.ToString();
            Button.interactable = true;
        }

        public void DisplaySlotSelectHighlight()
        {
            SlotSelectHighlight.gameObject.SetActive(true);
        }

        public void CancelDisplaySlotSelectHighlight()
        {
            IsSelected = false;
            SlotSelectHighlight.gameObject.SetActive(false);
        }

        private bool IsSwapInPlayerBag(SlotUI targetSlot) =>
            m_SlotType == SlotType.Bag && targetSlot.m_SlotType == SlotType.Bag;

        private bool IsSwapInStorageBox(SlotUI targetSlot) =>
            m_SlotType == SlotType.Box && targetSlot.m_SlotType == SlotType.Box;

        private bool IsBuyFromStore(SlotUI targetSlot) =>
            m_SlotType == SlotType.Shop && targetSlot.m_SlotType == SlotType.Bag;

        private bool IsSellToStore(SlotUI targetSlot) =>
            m_SlotType == SlotType.Bag && targetSlot.m_SlotType == SlotType.Shop;

        private bool IsBox(SlotUI targetSlot) =>
            m_SlotType != SlotType.Shop && targetSlot.m_SlotType != SlotType.Shop
         && m_SlotType != targetSlot.m_SlotType;
    }
}