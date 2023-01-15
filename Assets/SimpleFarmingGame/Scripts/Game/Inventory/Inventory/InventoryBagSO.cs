using System.Collections.Generic;
using UnityEngine;

namespace SimpleFarmingGame.Game
{
    [CreateAssetMenu(menuName = "ScriptableObject/Inventory/InventoryBag", fileName = "InventoryBagSO")]
    public class InventoryBagSO : ScriptableObject
    {
        public List<InventoryItem> ItemList;

        /// <summary>
        /// 根据传入的<paramref name="itemID"/>返回对应库存物品
        /// </summary>
        /// <param name="itemID">物品ID</param>
        /// <returns>库存物品</returns>
        public InventoryItem GetInventoryItem(int itemID)
        {
            return ItemList.Find(inventoryItem => inventoryItem.ItemID == itemID);
        }
    }
}