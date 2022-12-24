using System;
using System.Collections.Generic;
using SFG.Save;
using UnityEngine;

namespace SFG.InventorySystem
{
    public enum InventoryLocation { Player, Box }

    public static partial class EventSystem
    {
        public static Action<InventoryLocation, List<InventoryItem>> UpdateInventoryUI;

        public static void CallUpdateInventoryUI(InventoryLocation location, List<InventoryItem> itemList)
        {
            UpdateInventoryUI?.Invoke(location, itemList);
        }
    }

    public class InventoryManager : Singleton<InventoryManager>, ISavable
    {
        public ItemDataListSO ItemData;
        public InventoryBagSO PlayerBagTemp;
        public InventoryBagSO PlayerBag;
        public BluePrintDataListSO BluePrintData;
        [Header("Transaction")] public int PlayerMoney;
        private InventoryBagSO m_CurrentBoxBag;

        private Dictionary<string, List<InventoryItem>> m_BoxDataDict = new();
        public int BoxDataCount => m_BoxDataDict.Count;

        private void OnEnable()
        {
            EventSystem.DropItemEvent += OnDropItemEvent;
            CropSystem.EventSystem.SpawnFruitAtPlayerPosition += SpawnFruitAtPlayerBag;
            MapSystem.EventSystem.BuildFurnitureEvent += OnBuildFurnitureEvent;
            Characters.NPC.EventSystem.BaseBagOpenEvent += OnBaseBagOpenEvent;
            UI.EventSystem.StartNewGameEvent += OnStartNewGameEvent; // UI
        }

        private void OnDisable()
        {
            EventSystem.DropItemEvent -= OnDropItemEvent;
            CropSystem.EventSystem.SpawnFruitAtPlayerPosition -= SpawnFruitAtPlayerBag;
            MapSystem.EventSystem.BuildFurnitureEvent -= OnBuildFurnitureEvent;
            Characters.NPC.EventSystem.BaseBagOpenEvent -= OnBaseBagOpenEvent;
            UI.EventSystem.StartNewGameEvent -= OnStartNewGameEvent; // UI
        }

        private void Start()
        {
            ISavable savable = this;
            savable.RegisterSavable();
            // EventSystem.CallUpdateInventoryUI(InventoryLocation.Player, PlayerBag.ItemList);
        }

        public ItemDetails GetItemDetails(int itemID) => ItemData.GetItemDetails(itemID);

        public void AddItemInBag(Item item, bool destroy)
        {
            // var index = GetItemIndexByItemID(item.ItemID);
            // AddItemInBagAtIndex(item.ItemID, index, 1);

            GetItemIndexByItemID(item.ItemID, out int index);
            AddItemInBagAtIndex(item.ItemID, index, 1);

            if (destroy) Destroy(item.gameObject);

            EventSystem.CallUpdateInventoryUI(InventoryLocation.Player, PlayerBag.ItemList);
        }

        private bool CheckBagCapacity()
        {
            for (int i = 0; i < PlayerBag.ItemList.Count; ++i)
            {
                if (PlayerBag.ItemList[i].ItemID == 0) return true;
            }

            return false;
        }

        /// <returns>Return the item index or -1 if the item exists.</returns>
        private void GetItemIndexByItemID(int itemID, out int index)
        {
            index = 0;
            for (; index < PlayerBag.ItemList.Count; ++index)
            {
                if (PlayerBag.ItemList[index].ItemID == itemID) return;
            }

            index = -1;
        }

        private void AddItemInBagAtIndex(int itemID, int index, int amount)
        {
            InventoryItem inventoryItem = new InventoryItem { ItemID = itemID };

            if (index == -1 && CheckBagCapacity()) // Bag don't have this item and bag have empty Position.
            {
                inventoryItem.ItemAmount = amount;
                for (int i = 0; i < PlayerBag.ItemList.Count; ++i)
                {
                    if (PlayerBag.ItemList[i].ItemID == 0)
                    {
                        PlayerBag.ItemList[i] = inventoryItem;
                        break;
                    }
                }
            }
            else // Bag already have this item.
            {
                int newAmount = PlayerBag.ItemList[index].ItemAmount + amount;
                inventoryItem.ItemAmount = newAmount;
                PlayerBag.ItemList[index] = inventoryItem;
            }
        }

        public void SwapItemsWithinPlayerBag(int fromIndex, int targetIndex)
        {
            InventoryItem currentItem = PlayerBag.ItemList[fromIndex];
            InventoryItem targetItem = PlayerBag.ItemList[targetIndex];

            if (targetItem.ItemID != 0) // if target Position have item
            {
                PlayerBag.ItemList[targetIndex] = currentItem;
                PlayerBag.ItemList[fromIndex] = targetItem;
            }
            else // if target Position don't have item
            {
                PlayerBag.ItemList[targetIndex] = currentItem;
                PlayerBag.ItemList[fromIndex] = new InventoryItem();
            }

            EventSystem.CallUpdateInventoryUI(InventoryLocation.Player, PlayerBag.ItemList);
        }

        public void SwapItem
            (InventoryLocation fromLocation, int fromIndex, InventoryLocation targetLocation, int targetIndex)
        {
            List<InventoryItem> currentItemList = GetItemList(fromLocation);
            List<InventoryItem> targetItemList = GetItemList(targetLocation);
            InventoryItem currentItem = currentItemList[fromIndex];
            if (targetIndex < targetItemList.Count)
            {
                InventoryItem targetItem = targetItemList[targetIndex];

                if (targetItem.ItemID != 0 && currentItem.ItemID != targetItem.ItemID)
                {
                    currentItemList[fromIndex] = targetItem;
                    targetItemList[targetIndex] = currentItem;
                }
                else if (targetItem.ItemID != 0 && currentItem.ItemID == targetItem.ItemID)
                {
                    targetItem.ItemAmount += currentItem.ItemAmount;
                    targetItemList[targetIndex] = targetItem;
                    currentItemList[fromIndex] = new InventoryItem();
                }
                else // targetItem.ItemID == 0
                {
                    targetItemList[targetIndex] = currentItem;
                    currentItemList[fromIndex] = new InventoryItem();
                }
            }

            EventSystem.UpdateInventoryUI(fromLocation, currentItemList);
            EventSystem.UpdateInventoryUI(targetLocation, targetItemList);
        }

        private List<InventoryItem> GetItemList(InventoryLocation location)
        {
            return location switch
            {
                InventoryLocation.Player => PlayerBag.ItemList
              , InventoryLocation.Box => m_CurrentBoxBag.ItemList
              , _ => null
            };
        }

        private void RemoveItem(int itemID, int removeAmount)
        {
            GetItemIndexByItemID(itemID, out int index);
            if (PlayerBag.ItemList[index].ItemAmount > removeAmount)
            {
                int remainAmount = PlayerBag.ItemList[index].ItemAmount - removeAmount;
                InventoryItem inventoryItem = new InventoryItem { ItemID = itemID, ItemAmount = remainAmount };
                PlayerBag.ItemList[index] = inventoryItem;
            }
            else if (PlayerBag.ItemList[index].ItemAmount == removeAmount)
            {
                InventoryItem inventoryItem = new InventoryItem();
                PlayerBag.ItemList[index] = inventoryItem;
            }

            EventSystem.UpdateInventoryUI(InventoryLocation.Player, PlayerBag.ItemList);
        }

        private void OnDropItemEvent(int itemID, Vector3 position, ItemType itemType)
        {
            RemoveItem(itemID, 1);
        }

        private void SpawnFruitAtPlayerBag(int cropID)
        {
            GetItemIndexByItemID(cropID, out int index);
            AddItemInBagAtIndex(cropID, index, 1);
            EventSystem.CallUpdateInventoryUI(InventoryLocation.Player, PlayerBag.ItemList);
        }

        // 在背包当中移除图纸 InventoryManager
        // 移除建造家具所需资源物品 InventoryManager
        private void OnBuildFurnitureEvent(int buildingPaperID, Vector3 mouseWorldPosition)
        {
            RemoveItem(buildingPaperID, 1);
            BluePrintDetails bluePrintDetails = BluePrintData.GetBluePrintDetails(buildingPaperID);
            foreach (InventoryItem resourceItem in bluePrintDetails.RequireResourceItems)
            {
                RemoveItem(resourceItem.ItemID, resourceItem.ItemAmount);
            }
        }

        private void OnBaseBagOpenEvent(SlotType slotType, InventoryBagSO bagData)
        {
            m_CurrentBoxBag = bagData;
        }

        private void OnStartNewGameEvent(int obj)
        {
            PlayerBag = Instantiate(PlayerBagTemp);
            PlayerMoney = 300;
            m_BoxDataDict.Clear();
            EventSystem.CallUpdateInventoryUI(InventoryLocation.Player, PlayerBag.ItemList);
        }

        /// <summary>
        /// 检查建造资源物品库存
        /// </summary>
        /// <param name="buildingPaperID">建造图纸ID</param>
        /// <returns></returns>
        public bool CheckStorage(int buildingPaperID)
        {
            BluePrintDetails bluePrintDetails = BluePrintData.GetBluePrintDetails(buildingPaperID);
            foreach (InventoryItem requireResourceItem in bluePrintDetails.RequireResourceItems)
            {
                InventoryItem playerBagItem = PlayerBag.GetInventoryItem(requireResourceItem.ItemID);
                if (playerBagItem.ItemAmount >= requireResourceItem.ItemAmount)
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// 交易物品
        /// </summary>
        /// <param name="itemDetails">交易物品的物品详情</param>
        /// <param name="amount">交易数量</param>
        /// <param name="isSell">是否为卖</param>
        public void TransactionItem(ItemDetails itemDetails, int amount, bool isSell)
        {
            int cost = itemDetails.ItemPrice * amount; // 计算金额
            GetItemIndexByItemID(itemDetails.ItemID, out int index);
            if (isSell)
            {
                if (PlayerBag.ItemList[index].ItemAmount >= amount)
                {
                    RemoveItem(itemDetails.ItemID, amount);
                    // 卖出总价
                    cost = (int)(cost * itemDetails.SellPercentage);
                    PlayerMoney += cost;
                }
            }
            else if (PlayerMoney - cost >= 0)
            {
                if (CheckBagCapacity())
                {
                    AddItemInBagAtIndex(itemDetails.ItemID, index, amount);
                }

                PlayerMoney -= cost;
            }

            EventSystem.UpdateInventoryUI(InventoryLocation.Player, PlayerBag.ItemList);
        }

        public List<InventoryItem> GetBoxDataList(string key) =>
            m_BoxDataDict.ContainsKey(key) ? m_BoxDataDict[key] : null;

        public void AddBoxDataDict(Box box)
        {
            string key = box.name + box.Index;
            if (m_BoxDataDict.ContainsKey(key) == false)
            {
                m_BoxDataDict.Add(key, box.BoxBagData.ItemList);
            }

            Debug.Log(key);
        }

        public string GUID => GetComponent<DataGUID>().GUID;

        /// <summary>
        /// 保存玩家金钱数据、背包数据和储物箱数据
        /// </summary>
        /// <returns></returns>
        public GameSaveData GenerateSaveData()
        {
            GameSaveData saveData = new GameSaveData();
            saveData.PlayerMoney = PlayerMoney;

            saveData.InventoryDict = new Dictionary<string, List<InventoryItem>>();
            saveData.InventoryDict.Add(PlayerBag.name, PlayerBag.ItemList);

            foreach (var (key, value) in m_BoxDataDict)
            {
                saveData.InventoryDict.Add(key, value);
            }

            return saveData;
        }

        /// <summary>
        /// 恢复玩家金钱数据、背包数据和储物箱数据
        /// </summary>
        public void RestoreData(GameSaveData saveData)
        {
            PlayerMoney = saveData.PlayerMoney;
            PlayerBag = Instantiate(PlayerBagTemp);
            PlayerBag.ItemList = saveData.InventoryDict[PlayerBag.name];
            foreach (var (key, value) in saveData.InventoryDict)
            {
                if (m_BoxDataDict.ContainsKey(key))
                {
                    m_BoxDataDict[key] = value;
                }
            }

            EventSystem.CallUpdateInventoryUI(InventoryLocation.Player, PlayerBag.ItemList);
            // 刷新 PlayerMoney
        }
    }
}