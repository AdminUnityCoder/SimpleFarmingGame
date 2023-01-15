using System.Collections.Generic;
using UnityEngine;

namespace SimpleFarmingGame.Game
{
    public class InventoryManager : Singleton<InventoryManager>, ISavable
    {
        [SerializeField, Tooltip("物品数据")] private ItemDataListSO ItemData;
        [SerializeField, Tooltip("玩家背包数据模板")] private InventoryBagSO PlayerBagDataTemplate;
        [SerializeField, Tooltip("玩家背包数据")] private InventoryBagSO PlayerBagData;
        [SerializeField, Tooltip("蓝图数据")] private BluePrintDataListSO m_BluePrintData;
        [SerializeField, Tooltip("玩家金钱数")] private int m_PlayerMoney;
        private InventoryBagSO m_CurrentStorageBoxData;

        /// <summary>
        /// 储物箱数据字典，key：储物箱名字 + 储物箱序号，value：库存物品列表
        /// </summary>
        private Dictionary<string, List<InventoryItem>> m_StorageBoxDataDict = new();

        public BluePrintDataListSO BluePrintData => m_BluePrintData;
        public int PlayerMoney => m_PlayerMoney;
        public int BoxDataCount => m_StorageBoxDataDict.Count;

        private void Start()
        {
            ISavable savable = this;
            savable.RegisterSavable();
        }

        private void OnEnable()
        {
            EventSystem.OnDropItemEvent += DropItem;
            EventSystem.OnSpawnFruitAtPlayerPosition += SpawnFruitAtPlayerBag;
            EventSystem.OnBuildFurnitureEvent += RemoveRequiredResources;
            EventSystem.OnBaseBagOpenEvent += SetBagData;
            EventSystem.OnStartNewGameEvent += InitialInventory;
        }

        private void OnDisable()
        {
            EventSystem.OnDropItemEvent -= DropItem;
            EventSystem.OnSpawnFruitAtPlayerPosition -= SpawnFruitAtPlayerBag;
            EventSystem.OnBuildFurnitureEvent -= RemoveRequiredResources;
            EventSystem.OnBaseBagOpenEvent -= SetBagData;
            EventSystem.OnStartNewGameEvent -= InitialInventory;
        }

        public ItemDetails GetItemDetails(int itemID)
        {
            return ItemData.GetItemDetails(itemID);
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
                InventoryLocation.PlayerBag => PlayerBagData.ItemList
              , InventoryLocation.StorageBox => m_CurrentStorageBoxData.ItemList
              , _ => null
            };
        }

        #region Building

        /// <summary>
        /// 根据传入的<paramref name="buildingPaperID"/>检查建筑资源库存是否满足需求
        /// </summary>
        /// <param name="buildingPaperID">建造图纸ID</param>
        /// <returns>库存均满足需求返回true，反之返回false</returns>
        public bool CheckBuildingResourcesStock(int buildingPaperID)
        {
            BluePrintDetails bluePrintDetails = m_BluePrintData.GetBluePrintDetails(buildingPaperID);
            foreach (InventoryItem requireResourceItem in bluePrintDetails.RequireResourceItems)
            {
                int playerBagItemAmount = PlayerBagData.GetInventoryItem(requireResourceItem.ItemID).ItemAmount;
                if (playerBagItemAmount < requireResourceItem.ItemAmount) return false;
            }

            return true;
        }

        #endregion

        #region Transaction

        /// <summary>
        /// 交易物品
        /// </summary>
        /// <param name="itemDetails">交易物品的物品详情</param>
        /// <param name="transactionAmount">交易数量</param>
        /// <param name="isSell">是否为卖</param>
        public void TransactionItem(ItemDetails itemDetails, int transactionAmount, bool isSell)
        {
            int cost = itemDetails.ItemPrice * transactionAmount; // 计算金额
            GetItemIndexInBag(itemDetails.ItemID, out var index);
            // 卖
            if (isSell)
            {
                if (PlayerBagData.ItemList[index].ItemAmount >= transactionAmount)
                {
                    RemoveItem(itemDetails.ItemID, transactionAmount);
                    // 卖出总价
                    cost = (int)(cost * itemDetails.SellPercentage);
                    m_PlayerMoney += cost;
                }
            }
            // 买 -> 是否够钱买
            else if (m_PlayerMoney - cost >= 0)
            {
                // 有空位
                if (CheckBagForEmptySpace() || (!CheckBagForEmptySpace() && index != -1))
                {
                    AddItemInBag(itemDetails.ItemID, index, transactionAmount);
                }

                m_PlayerMoney -= cost;
            }

            EventSystem.RefreshInventoryUI(InventoryLocation.PlayerBag, PlayerBagData.ItemList);
        }

        #endregion

        #region Inventory

        /// <summary>
        /// 添加物品进背包
        /// </summary>
        /// <param name="item">添加的物品</param>
        public void AddItemToBag(Item item)
        {
            GetItemIndexInBag(item.ItemID, out int index);
            AddItemInBag(item.ItemID, index, 1);
            EventSystem.CallRefreshInventoryUI(InventoryLocation.PlayerBag, PlayerBagData.ItemList);
        }

        /// <summary>
        /// 将指定<paramref name="itemID"/>物品添加到背包指定索引处
        /// </summary>
        /// <param name="itemID">物品ID</param>
        /// <param name="index">索引</param>
        /// <param name="amount">数量</param>
        private void AddItemInBag(int itemID, int index, int amount)
        {
            InventoryItem inventoryItem = new InventoryItem { ItemID = itemID };

            if (index == -1 && CheckBagForEmptySpace())
            {
                inventoryItem.ItemAmount = amount;
                for (int i = 0; i < PlayerBagData.ItemList.Count; ++i)
                {
                    if (PlayerBagData.ItemList[i].ItemID != 0) continue; // 找背包空位
                    PlayerBagData.ItemList[i] = inventoryItem;
                    break;
                }
            }
            else // 背包已经有这个物品
            {
                int newAmount = PlayerBagData.ItemList[index].ItemAmount + amount;
                inventoryItem.ItemAmount = newAmount;
                PlayerBagData.ItemList[index] = inventoryItem;
            }
        }

        /// <summary>
        /// 检测背包是否有空位
        /// </summary>
        /// <returns></returns>
        private bool CheckBagForEmptySpace()
        {
            for (int i = 0; i < PlayerBagData.ItemList.Count; ++i)
            {
                if (PlayerBagData.ItemList[i].ItemID == 0) return true;
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
            for (index = 0; index < PlayerBagData.ItemList.Count; ++index)
            {
                if (PlayerBagData.ItemList[index].ItemID == itemID) return;
            }

            index = -1;
        }

        /// <summary>
        /// 通过索引在玩家背包内交换物品位置（物品数据）
        /// </summary>
        /// <param name="curIndex">当前索引</param>
        /// <param name="targetIndex">目标索引</param>
        public void SwapItemsWithinPlayerBag(int curIndex, int targetIndex)
        {
            InventoryItem curItem = PlayerBagData.ItemList[curIndex];
            InventoryItem targetItem = PlayerBagData.ItemList[targetIndex];

            if (targetItem.ItemID != 0) // 目标位置不为空
            {
                PlayerBagData.ItemList[targetIndex] = curItem;
                PlayerBagData.ItemList[curIndex] = targetItem;
            }
            else // 目标位置为空
            {
                PlayerBagData.ItemList[targetIndex] = curItem;
                PlayerBagData.ItemList[curIndex] = new InventoryItem();
            }

            EventSystem.CallRefreshInventoryUI(InventoryLocation.PlayerBag, PlayerBagData.ItemList);
        }

        /// <summary>
        /// 跨背包交换数据
        /// </summary>
        /// <param name="curLocation">当前库存位置</param>
        /// <param name="curIndex">当前索引</param>
        /// <param name="targetLocation">目标库存位置</param>
        /// <param name="targetIndex">目标索引</param>
        public void SwapItem
            (InventoryLocation curLocation, int curIndex, InventoryLocation targetLocation, int targetIndex)
        {
            // 获取物品列表
            List<InventoryItem> curItemList = GetItemList(curLocation);
            List<InventoryItem> targetItemList = GetItemList(targetLocation);

            InventoryItem curInventoryItem = curItemList[curIndex]; // 通过物品列表找到指定索引物品

            if (targetIndex < targetItemList.Count)
            {
                InventoryItem targetInventoryItem = targetItemList[targetIndex];

                // 目标位置不为空
                if (targetInventoryItem.ItemID != 0)
                {
                    if (curLocation == targetLocation && curIndex == targetIndex)
                    {
                        targetItemList[targetIndex] = curInventoryItem;
                        return;
                    }

                    // 当前物品与目标位置物品不相同
                    if (curInventoryItem.ItemID != targetInventoryItem.ItemID)
                    {
                        curItemList[curIndex] = targetInventoryItem;
                        targetItemList[targetIndex] = curInventoryItem;
                    }
                    // 当前物品与目标位置物品相同
                    else
                    {
                        targetInventoryItem.ItemAmount += curInventoryItem.ItemAmount;
                        targetItemList[targetIndex] = targetInventoryItem; // 更新数据
                        curItemList[curIndex] = new InventoryItem();
                    }
                }
                // 目标位置为空
                else
                {
                    targetItemList[targetIndex] = curInventoryItem;
                    curItemList[curIndex] = new InventoryItem();
                }

                EventSystem.RefreshInventoryUI(curLocation, curItemList);
                EventSystem.RefreshInventoryUI(targetLocation, targetItemList);
            }
        }

        /// <summary>
        /// 从背包中移除物品
        /// </summary>
        /// <param name="itemID">移除物品的ID</param>
        /// <param name="removeAmount">移除数量</param>
        private void RemoveItem(int itemID, int removeAmount)
        {
            // 获取物品在背包中的索引
            GetItemIndexInBag(itemID, out var index);
            // 获取物品数量
            int itemAmount = PlayerBagData.ItemList[index].ItemAmount;
            if (itemAmount > removeAmount)
            {
                // 计算剩余数量
                int remainAmount = itemAmount - removeAmount;
                // 更新物品数据 -> 更新数量
                InventoryItem inventoryItem = new InventoryItem
                {
                    ItemID = itemID
                  , ItemAmount = remainAmount
                };
                PlayerBagData.ItemList[index] = inventoryItem;
            }
            else if (itemAmount == removeAmount)
            {
                // 更新物品数据 -> 置空
                InventoryItem inventoryItem = new InventoryItem();
                PlayerBagData.ItemList[index] = inventoryItem;
            }
            // else { TODO: 显示材料不足或者移除物品不存在 }

            EventSystem.RefreshInventoryUI(InventoryLocation.PlayerBag, PlayerBagData.ItemList);
        }

        #endregion

        #region Event

        /// <summary>
        /// 丢弃物品
        /// </summary>
        /// <param name="itemID">丢弃物品ID</param>
        /// <param name="position">丢弃位置</param>
        /// <param name="itemType">丢弃物品类型</param>
        private void DropItem(int itemID, Vector3 position, ItemType itemType)
        {
            RemoveItem(itemID, 1);
        }

        /// <summary>
        /// 在玩家背包处生成果实
        /// </summary>
        /// <param name="fruitID">果实ID</param>
        private void SpawnFruitAtPlayerBag(int fruitID)
        {
            GetItemIndexInBag(fruitID, out int index);
            AddItemInBag(fruitID, index, 1); // BUG：如果背包满了则无法生成果实，应该使用UI提示玩家背包已经满了
            EventSystem.CallRefreshInventoryUI(InventoryLocation.PlayerBag, PlayerBagData.ItemList);
        }

        /// <summary>
        /// 移除建造图纸和建造家具所需资源
        /// </summary>
        /// <param name="buildingPaperID">建造图纸ID</param>
        /// <param name="mouseWorldPosition">鼠标世界坐标</param>
        private void RemoveRequiredResources(int buildingPaperID, Vector3 mouseWorldPosition)
        {
            // 移除建造图纸
            RemoveItem(buildingPaperID, 1);
            // 移除所需资源
            BluePrintDetails bluePrintDetails = m_BluePrintData.GetBluePrintDetails(buildingPaperID);
            foreach (InventoryItem resourceItem in bluePrintDetails.RequireResourceItems)
            {
                RemoveItem(resourceItem.ItemID, resourceItem.ItemAmount);
            }
        }

        /// <summary>
        /// 设置背包数据
        /// </summary>
        /// <param name="slotType">格子类型</param>
        /// <param name="boxData">背包数据</param>
        private void SetBagData(SlotType slotType, InventoryBagSO boxData)
        {
            m_CurrentStorageBoxData = boxData;
        }

        /// <summary>
        /// 初始化库存
        /// </summary>
        private void InitialInventory(int obj)
        {
            PlayerBagData = Instantiate(PlayerBagDataTemplate);
            m_PlayerMoney = 300;
            m_StorageBoxDataDict.Clear();
            EventSystem.CallRefreshInventoryUI(InventoryLocation.PlayerBag, PlayerBagData.ItemList);
        }

        #endregion

        #region StorageBox

        /// <summary>
        /// 通过传进来的<paramref name="key"/>获取对应的储物箱数据列表
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>储物箱数据列表</returns>
        public List<InventoryItem> GetBoxDataList(string key)
        {
            return m_StorageBoxDataDict.ContainsKey(key) ? m_StorageBoxDataDict[key] : null;
        }

        /// <summary>
        /// 往储物箱数据字典中添加数据
        /// </summary>
        /// <param name="box"></param>
        public void AddDataToStorageBoxDataDict(Box box)
        {
            var key = box.name + box.BoxIndex;
            if (!m_StorageBoxDataDict.ContainsKey(key))
            {
                m_StorageBoxDataDict.Add(key, box.BoxBagData.ItemList);
            }
        }

        #endregion

        #region Save

        public string GUID => GetComponent<DataGUID>().GUID;

        /// <summary>
        /// 保存玩家金钱数据、背包数据和储物箱数据
        /// </summary>
        /// <returns></returns>
        public GameSaveData GenerateSaveData()
        {
            GameSaveData saveData = new GameSaveData();
            saveData.PlayerMoney = m_PlayerMoney;

            saveData.InventoryDict = new Dictionary<string, List<InventoryItem>>();
            saveData.InventoryDict.Add(PlayerBagData.name, PlayerBagData.ItemList);

            foreach (var (key, value) in m_StorageBoxDataDict)
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
            m_PlayerMoney = saveData.PlayerMoney;

            PlayerBagData = Instantiate(PlayerBagDataTemplate);
            PlayerBagData.ItemList = saveData.InventoryDict[PlayerBagData.name];

            foreach (var (key, value) in saveData.InventoryDict)
            {
                if (m_StorageBoxDataDict.ContainsKey(key))
                {
                    m_StorageBoxDataDict[key] = value;
                }
            }

            // 刷新UI
            EventSystem.CallRefreshInventoryUI(InventoryLocation.PlayerBag, PlayerBagData.ItemList);
        }

        #endregion
    }
}