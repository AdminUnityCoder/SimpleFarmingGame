using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SFG.InventorySystem
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("Tooltip")] [SerializeField] private ItemTooltip ItemTooltip;

        [Header("DragItem")] [SerializeField] private Image DragItemImage;

        [Header("PlayerBag")] [SerializeField] private GameObject PlayerBagGo; // TODO: 后续考虑将UI移出屏幕渲染范围外
        [SerializeField] private SlotUI[] PlayerSlots;

        [Header("All-purpose Bag")]
        [SerializeField]
        private GameObject BaseBagPanel;

        [SerializeField] private GameObject ShopSlotPrefab;
        [SerializeField] private List<SlotUI> BaseBagSlotList;

        [Header("Box")] [SerializeField] private GameObject BoxSlotPrefab;

        [Header("交易UI")] public TransactionUI TransactionUI;
        public TextMeshProUGUI PlayerMoneyText;

        private Button m_BagOpenButton;
        private bool m_IsBagOpened;

        #region MonoBehaviour

        private void Awake()
        {
            m_BagOpenButton = transform.GetChild(1).GetChild(0).GetComponent<Button>();
            m_BagOpenButton.onClick.AddListener(OpenPlayerBag);
        }

        private void OnEnable()
        {
            EventSystem.UpdateInventoryUI += OnUpdateInventoryUI;
            TransitionSystem.EventSystem.BeforeSceneUnloadedEvent += OnBeforeSceneUnloadedEvent;
            Characters.NPC.EventSystem.BaseBagOpenEvent += OnBaseBagOpenEvent;
            Characters.NPC.EventSystem.BaseBagCloseEvent += OnBaseBagCloseEvent;
            EventSystem.ShowTransactionUIEvent += OnShowTransactionUIEvent;
        }

        private void OnDisable()
        {
            EventSystem.UpdateInventoryUI -= OnUpdateInventoryUI;
            TransitionSystem.EventSystem.BeforeSceneUnloadedEvent -= OnBeforeSceneUnloadedEvent;
            Characters.NPC.EventSystem.BaseBagOpenEvent -= OnBaseBagOpenEvent;
            Characters.NPC.EventSystem.BaseBagCloseEvent -= OnBaseBagCloseEvent;
            EventSystem.ShowTransactionUIEvent -= OnShowTransactionUIEvent;
        }

        private void Start()
        {
            SetupSlotIndex();

            m_IsBagOpened = PlayerBagGo.activeInHierarchy;

            PlayerMoneyText.text = InventoryManager.Instance.PlayerMoney.ToString();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                OpenPlayerBag();
            }
        }

        #endregion

        #region SlotUI

        private void OnUpdateInventoryUI(InventoryLocation location, List<InventoryItem> itemList)
        {
            switch (location)
            {
                case InventoryLocation.Player:
                    for (int i = 0; i < PlayerSlots.Length; ++i)
                    {
                        if (itemList[i].ItemAmount > 0)
                        { 
                            // itemList => PlayerBagList
                            ItemDetails itemDetails = InventoryManager.Instance.GetItemDetails(itemList[i].ItemID);
                            PlayerSlots[i].SetSlot(itemDetails, itemList[i].ItemAmount);
                        }
                        else
                        {
                            PlayerSlots[i].SetSlotEmpty();
                        }
                    }

                    break;
                case InventoryLocation.Box:
                    for (int i = 0; i < BaseBagSlotList.Count; ++i)
                    {
                        if (itemList[i].ItemAmount > 0)
                        {
                            // itemList => PlayerBagList
                            ItemDetails itemDetails = InventoryManager.Instance.GetItemDetails(itemList[i].ItemID);
                            BaseBagSlotList[i].SetSlot(itemDetails, itemList[i].ItemAmount);
                        }
                        else
                        {
                            BaseBagSlotList[i].SetSlotEmpty();
                        }
                    }

                    break;
            }

            PlayerMoneyText.text = InventoryManager.Instance.PlayerMoney.ToString();
        }

        private void OnBeforeSceneUnloadedEvent()
        {
            CancelDisplayAllSlotHighlight();
        }

        private void OnBaseBagOpenEvent(SlotType slotType, InventoryBagSO bagData)
        {
            GameObject prefab = slotType switch
            {
                SlotType.Shop => ShopSlotPrefab, SlotType.Box => BoxSlotPrefab, _ => null
            };

            BaseBagPanel.SetActive(true);
            BaseBagSlotList = new List<SlotUI>();
            for (int i = 0; i < bagData.ItemList.Count; ++i)
            {
                SlotUI slot = Instantiate(prefab, BaseBagPanel.transform.GetChild(0)).GetComponent<SlotUI>();
                slot.SlotIndex = i;
                BaseBagSlotList.Add(slot);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(BaseBagPanel.GetComponent<RectTransform>());

            if (slotType == SlotType.Shop)
            {
                PlayerBagGo.GetComponent<RectTransform>().pivot = new Vector2(-1f, 0.5f);
                PlayerBagGo.SetActive(true);
                m_IsBagOpened = true;
            }

            OnUpdateInventoryUI(InventoryLocation.Box, bagData.ItemList);
        }

        private void OnBaseBagCloseEvent(SlotType slotType, InventoryBagSO bagData)
        {
            BaseBagPanel.SetActive(false);
            ItemTooltip.gameObject.SetActive(false);
            CancelDisplayAllSlotHighlight();
            foreach (SlotUI slot in BaseBagSlotList)
            {
                Destroy(slot.gameObject);
            }

            BaseBagSlotList.Clear();
            if (slotType == SlotType.Shop)
            {
                PlayerBagGo.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
                PlayerBagGo.SetActive(false);
                m_IsBagOpened = false;
            }
        }

        private void OnShowTransactionUIEvent(ItemDetails itemDetails, bool isSell)
        {
            TransactionUI.gameObject.SetActive(true);
            TransactionUI.SetupTransactionUI(itemDetails, isSell);
        }

        private void SetupSlotIndex()
        {
            for (int index = 0; index < PlayerSlots.Length; ++index)
            {
                PlayerSlots[index].SlotIndex = index;
            }
        }

        public void DisplaySlotHighlight(int index)
        {
            for (var slotIndex = 0; slotIndex < PlayerSlots.Length; slotIndex++)
            {
                var playerSlot = PlayerSlots[slotIndex];
                if (playerSlot.IsSelected && playerSlot.SlotIndex == index)
                {
                    playerSlot.DisplaySlotSelectHighlight();
                }
                else
                {
                    playerSlot.CancelDisplaySlotSelectHighlight();
                }
            }
        }

        public void CancelDisplayAllSlotHighlight()
        {
            for (var slotIndex = 0; slotIndex < PlayerSlots.Length; slotIndex++)
            {
                PlayerSlots[slotIndex].CancelDisplaySlotSelectHighlight();
            }
        }

        #endregion

        #region DragItem

        public void ShowDragItemImage(Sprite itemSprite)
        {
            DragItemImage.enabled = true;      // Show DragItem Image
            DragItemImage.sprite = itemSprite; // Set DragItem Sprite
            DragItemImage.SetNativeSize();
        }

        public void HideDragItemImage()
        {
            DragItemImage.enabled = false;
        }

        public void SetDragItemImagePosition(Vector3 pos)
        {
            DragItemImage.transform.position = pos;
        }

        #endregion

        #region ItemTooltip

        public void ShowItemTooltip(SlotUI slotUI)
        {
            ItemTooltip.gameObject.SetActive(true);
            ItemTooltip.SetupTooltip(slotUI.ItemDetails, slotUI.SlotType);
            ItemTooltip.transform.position = slotUI.transform.position + Vector3.up * 60;

            if (slotUI.ItemDetails.ItemType == ItemType.Furniture)
            {
                ItemTooltip.RequireResourcePanel.gameObject.SetActive(true);
                ItemTooltip.SetupRequireResourcePanel(slotUI.ItemDetails.ItemID);
            }
            else
            {
                ItemTooltip.RequireResourcePanel.gameObject.SetActive(false);
            }
        }

        public void HideItemTooltip()
        {
            ItemTooltip.gameObject.SetActive(false);
        }

        #endregion

        private void OpenPlayerBag()
        {
            m_IsBagOpened = !m_IsBagOpened;
            PlayerBagGo.SetActive(m_IsBagOpened);
        }
    }
}