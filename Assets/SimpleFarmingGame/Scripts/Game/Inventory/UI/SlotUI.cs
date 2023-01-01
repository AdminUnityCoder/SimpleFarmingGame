using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SimpleFarmingGame.Game
{
    public enum SlotType { Bag, Box, Shop }

    public static partial class EventSystem
    {
        public static event Action<ItemDetails, bool> ItemSelectedEvent;

        public static void CallItemSelectedEvent(ItemDetails itemDetails, bool isSelected)
        {
            ItemSelectedEvent?.Invoke(itemDetails, isSelected);
        }

        public static event Action<ItemDetails, bool> ShowTransactionUIEvent;

        /// <param name="itemDetails">物品详情</param>
        /// <param name="isSell">是否是卖</param>
        public static void CallShowTransactionUIEvent(ItemDetails itemDetails, bool isSell)
        {
            ShowTransactionUIEvent?.Invoke(itemDetails, isSell);
        }
    }

    public sealed class SlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        /*
         * 因为需要一开始将 PlayerBag 隐藏，如果不使用组件拖拽的方式进行组件的获取
         * 而是在 Awake 中获取组件，会导致获取不到已经隐藏的物体身上的组件
         */
        [Header("Component")] [SerializeField] private Button Button;
        [SerializeField] private Image ItemSprite;
        [SerializeField] private TextMeshProUGUI AmountTextUI;
        [SerializeField] private Image SlotSelectHighlight;
        public SlotType SlotType;
        public int SlotIndex;
        public bool IsSelected;
        public int ItemAmount;
        public ItemDetails ItemDetails;
        public InventoryUI m_InventoryUI => GetComponentInParent<InventoryUI>();

        public InventoryLocation Location =>
            SlotType switch
            {
                SlotType.Bag => InventoryLocation.Player
              , SlotType.Box => InventoryLocation.Box
              , _ => InventoryLocation.Player
                // FIXME: 后续应该给Shop添加对应的InventoryLocation,即使Shop不是Inventory
            };

        private void Start()
        {
            IsSelected = false;
            if (ItemDetails == null)
            {
                SetSlotEmpty();
            }
        }

        public void SetSlotEmpty()
        {
            if (IsSelected)
            {
                IsSelected = false;
                m_InventoryUI.CancelDisplayAllSlotHighlight();
                EventSystem.CallItemSelectedEvent(ItemDetails, IsSelected);
            }

            ItemDetails = null;
            ItemSprite.enabled = false;
            AmountTextUI.text = string.Empty;
            Button.interactable = false;
        }

        public void SetSlot(ItemDetails itemDetails, int amount)
        {
            ItemDetails = itemDetails;
            ItemAmount = amount;
            ItemSprite.enabled = true;
            ItemSprite.sprite = ItemDetails.ItemIcon;
            AmountTextUI.text = ItemAmount.ToString();
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

        public void OnPointerClick(PointerEventData eventData)
        {
            if (ItemDetails == null) return;
            IsSelected = !IsSelected;

            m_InventoryUI.DisplaySlotHighlight(SlotIndex);

            if (SlotType == SlotType.Bag)
            {
                EventSystem.CallItemSelectedEvent(this.ItemDetails, this.IsSelected);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (ItemDetails == null) return;
            IsSelected = true;
            m_InventoryUI.DisplaySlotHighlight(SlotIndex);
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
                if (eventData.pointerCurrentRaycast.gameObject.TryGetComponent(out SlotUI targetSlot) == false)
                {
                    return;
                }

                int targetIndex = targetSlot.SlotIndex;

                if (SwapLocation(targetSlot))
                {
                    InventoryManager.Instance.SwapItemsWithinPlayerBag(this.SlotIndex, targetIndex);
                }
                else if (Buy(targetSlot))
                {
                    EventSystem.CallShowTransactionUIEvent(ItemDetails, false);
                }
                else if (Sell(targetSlot))
                {
                    EventSystem.CallShowTransactionUIEvent(ItemDetails, true);
                }
                else if (IsBox(targetSlot))
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

        private bool SwapLocation(SlotUI targetSlot) => SlotType == SlotType.Bag && targetSlot.SlotType == SlotType.Bag;
        private bool Buy(SlotUI targetSlot) => SlotType == SlotType.Shop && targetSlot.SlotType == SlotType.Bag;
        private bool Sell(SlotUI targetSlot) => SlotType == SlotType.Bag && targetSlot.SlotType == SlotType.Shop;

        private bool IsBox(SlotUI targetSlot) =>
            SlotType != SlotType.Shop && targetSlot.SlotType != SlotType.Shop && SlotType != targetSlot.SlotType;
    }
}