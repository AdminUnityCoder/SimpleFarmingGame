using System.Collections.Generic;
using UnityEngine;

namespace SimpleFarmingGame.Game
{
    [CreateAssetMenu(menuName = "ScriptableObject/Inventory/InventoryBag", fileName = "InventoryBagSO")]
    public class InventoryBagSO : ScriptableObject
    {
        public List<InventoryItem> ItemList;

        public InventoryItem GetInventoryItem(int itemID)
        {
            return ItemList.Find(inventoryItem => inventoryItem.ItemID == itemID);
        }
    }
}