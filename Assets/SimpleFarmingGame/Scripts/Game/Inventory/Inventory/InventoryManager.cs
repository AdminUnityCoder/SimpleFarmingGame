using System.Collections.Generic;
using UnityEngine;

namespace SimpleFarmingGame.Game
{
    public class InventoryManager : Singleton<InventoryManager>, ISavable
    {
        [Tooltip("物品数据")] public ItemDataListSO ItemData;
        [SerializeField, Tooltip("玩家背包数据模板")] private InventoryBagSO PlayerBagTemp;
        [SerializeField, Tooltip("玩家背包数据")] private InventoryBagSO PlayerBag;

        [Tooltip("蓝图数据")] public BluePrintDataListSO BluePrintData;

        public int PlayerMoney;

        private Dictionary<string, List<InventoryItem>> m_BoxDataDict = new();
        private InventoryBagSO m_CurrentBoxBag;
        public int BoxDataCount => m_BoxDataDict.Count;

        private void Start()
        {
            ISavable savable = this;
            savable.RegisterSavable();
        }

        private void OnEnable()
        {
            EventSystem.DropItemEvent += OnDropItemEvent;
            EventSystem.SpawnFruitAtPlayerPosition += SpawnFruitAtPlayerBag;
            EventSystem.BuildFurnitureEvent += OnBuildFurnitureEvent;
            EventSystem.BaseBagOpenEvent += OnBaseBagOpenEvent;
            EventSystem.StartNewGameEvent += OnStartNewGameEvent;
        }

        private void OnDisable()
        {
            EventSystem.DropItemEvent -= OnDropItemEvent;
            EventSystem.SpawnFruitAtPlayerPosition -= SpawnFruitAtPlayerBag;
            EventSystem.BuildFurnitureEvent -= OnBuildFurnitureEvent;
            EventSystem.BaseBagOpenEvent -= OnBaseBagOpenEvent;
            EventSystem.StartNewGameEvent -= OnStartNewGameEvent;
        }

        public ItemDetails GetItemDetails(int itemID) => ItemData.GetItemDetails(itemID);

        /// <summary>
        /// 添加物品进背包
        /// </summary>
        /// <param name="item">添加的物品</param>
        public void AddItemToBag(Item item)
        {
            GetItemIndexInBag(item.ItemID, out int index);
            AddItemInBagAtIndex(item.ItemID, index, 1);
            EventSystem.CallRefreshInventoryUI(InventoryLocation.PlayerBag, PlayerBag.ItemList);
        }

        /// <summary>
        /// 检测背包是否有空位
        /// </summary>
        /// <returns></returns>
        private bool CheckBagForEmptySpace()
        {
            for (int i = 0; i < PlayerBag.ItemList.Count; ++i)
            {
                if (PlayerBag.ItemList[i].ItemID == 0) return true;
            }

            return false;
        }

        /// <summary>
        /// 通过物品的<paramref name="itemID"/>获取物品在背包中的索引
        /// </summary>
        /// <param name="itemID">物品ID</param>
        /// <param name="index">索引</param>
        /// <returns>如果获取不到，则返回 index = -1</returns>
        private void GetItemIndexInBag(int itemID, out int index)
        {
            for (index = 0; index < PlayerBag.ItemList.Count; ++index)
            {
                if (PlayerBag.ItemList[index].ItemID == itemID) return;
            }

            index = -1;
        }

        /// <summary>
        /// 将指定<paramref name="itemID"/>物品添加到背包指定索引处
        /// </summary>
        /// <param name="itemID">物品ID</param>
        /// <param name="index">索引</param>
        /// <param name="amount">数量</param>
        private void AddItemInBagAtIndex(int itemID, int index, int amount)
        {
            InventoryItem inventoryItem = new InventoryItem { ItemID = itemID };

            if (index == -1 && CheckBagForEmptySpace()) // 背包没有该物品但背包有空位
            {
                inventoryItem.ItemAmount = amount;
                for (int i = 0; i < PlayerBag.ItemList.Count; ++i)
                {
                    if (PlayerBag.ItemList[i].ItemID != 0) continue; // 找背包空位
                    PlayerBag.ItemList[i] = inventoryItem;
                    break;
                }
            }
            else // 背包已经有这个物品
            {
                int newAmount = PlayerBag.ItemList[index].ItemAmount + amount;
                inventoryItem.ItemAmount = newAmount;
                PlayerBag.ItemList[index] = inventoryItem;
            }
        }

        /// <summary>
        /// 通过索引在玩家背包内交换物品位置（物品数据）
        /// </summary>
        /// <param name="fromIndex">当前索引</param>
        /// <param name="targetIndex">目标索引</param>
        public void SwapItemsWithinPlayerBag(int fromIndex, int targetIndex)
        {
            InventoryItem currentItem = PlayerBag.ItemList[fromIndex];
            InventoryItem targetItem = PlayerBag.ItemList[targetIndex];

            if (targetItem.ItemID == 0) // 目标位置没有物品
            {
                PlayerBag.ItemList[targetIndex] = currentItem;
                PlayerBag.ItemList[fromIndex] = new InventoryItem();
            }
            else // 目标位置已经有物品
            {
                PlayerBag.ItemList[targetIndex] = currentItem;
                PlayerBag.ItemList[fromIndex] = targetItem;
            }

            EventSystem.CallRefreshInventoryUI(InventoryLocation.PlayerBag, PlayerBag.ItemList);
        }

        /// <summary>
        /// 跨背包交换数据
        /// </summary>
        /// <param name="fromLocation">当前库存位置</param>
        /// <param name="fromIndex">当前索引</param>
        /// <param name="targetLocation">目标库存位置</param>
        /// <param name="targetIndex">目标索引</param>
        public void SwapItem
            (InventoryLocation fromLocation, int fromIndex, InventoryLocation targetLocation, int targetIndex)
        {
            List<InventoryItem> currentItemList = GetItemList(fromLocation);
            List<InventoryItem> targetItemList = GetItemList(targetLocation);

            InventoryItem currentItem = currentItemList[fromIndex]; // 通过物品列表找到指定索引物品

            if (targetIndex < targetItemList.Count) // 不做这个判断可能会导致索引越界
            {
                InventoryItem targetItem = targetItemList[targetIndex];

                // // 目标位置不为空 并且 当前物品与目标位置物品不相同
                // if (targetItem.ItemID != 0 && currentItem.ItemID != targetItem.ItemID)
                // {
                //     currentItemList[fromIndex] = targetItem;
                //     targetItemList[targetIndex] = currentItem;
                // }
                // // 目标位置不为空 并且 当前物品与目标位置物品相同
                // else if (targetItem.ItemID != 0 && currentItem.ItemID == targetItem.ItemID)
                // {
                //     targetItem.ItemAmount += currentItem.ItemAmount;
                //     targetItemList[targetIndex] = targetItem;
                //     currentItemList[fromIndex] = new InventoryItem();
                // }
                // // 目标位置为空
                // else
                // {
                //     targetItemList[targetIndex] = currentItem;
                //     currentItemList[fromIndex] = new InventoryItem();
                // }

                #region TEST

                // 目标位置不为空
                if (targetItem.ItemID != 0)
                {
                    // 当前物品与目标位置物品不相同
                    if (currentItem.ItemID != targetItem.ItemID)
                    {
                        currentItemList[fromIndex] = targetItem;
                        targetItemList[targetIndex] = currentItem;
                    }
                    // 当前物品与目标位置物品相同
                    else
                    {
                        targetItem.ItemAmount += currentItem.ItemAmount;
                        targetItemList[targetIndex] = targetItem;
                        currentItemList[fromIndex] = new InventoryItem();
                    }
                }
                // 目标位置为空
                else
                {
                    targetItemList[targetIndex] = currentItem;
                    currentItemList[fromIndex] = new InventoryItem();
                }

                #endregion
            }

            EventSystem.RefreshInventoryUI(fromLocation, currentItemList);
            EventSystem.RefreshInventoryUI(targetLocation, targetItemList);
        }

        /// <summary>
        /// 通过库存位置获取物品列表
        /// </summary>
        /// <param name="inventoryLocation">库存位置</param>
        /// <returns>根据库存位置返回库存物品列表</returns>
        private List<InventoryItem> GetItemList(InventoryLocation inventoryLocation)
        {
            return inventoryLocation switch
            {
                InventoryLocation.PlayerBag => PlayerBag.ItemList
              , InventoryLocation.StorageBox => m_CurrentBoxBag.ItemList
              , _ => null
            };
        }

        private void RemoveItem(int itemID, int removeAmount)
        {
            GetItemIndexInBag(itemID, out int index);
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

            EventSystem.RefreshInventoryUI(InventoryLocation.PlayerBag, PlayerBag.ItemList);
        }

        private void OnDropItemEvent(int itemID, Vector3 position, ItemType itemType)
        {
            RemoveItem(itemID, 1);
        }

        private void SpawnFruitAtPlayerBag(int cropID)
        {
            GetItemIndexInBag(cropID, out int index);
            AddItemInBagAtIndex(cropID, index, 1);
            EventSystem.CallRefreshInventoryUI(InventoryLocation.PlayerBag, PlayerBag.ItemList);
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
            EventSystem.CallRefreshInventoryUI(InventoryLocation.PlayerBag, PlayerBag.ItemList);
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
            GetItemIndexInBag(itemDetails.ItemID, out int index);
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
                if (CheckBagForEmptySpace())
                {
                    AddItemInBagAtIndex(itemDetails.ItemID, index, amount);
                }

                PlayerMoney -= cost;
            }

            EventSystem.RefreshInventoryUI(InventoryLocation.PlayerBag, PlayerBag.ItemList);
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

        #region Save

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

            // 刷新UI
            EventSystem.CallRefreshInventoryUI(InventoryLocation.PlayerBag, PlayerBag.ItemList);
        }

        #endregion
    }
}