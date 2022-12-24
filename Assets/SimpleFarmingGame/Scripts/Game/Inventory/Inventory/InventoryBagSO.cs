using System;
using System.Collections.Generic;
using UnityEngine;

namespace SFG.InventorySystem
{
    [Serializable]
    public struct InventoryItem
    {
        public int ItemID;
        public int ItemAmount;
    }

    [CreateAssetMenu(menuName = "ScriptableObject/Inventory/InventoryBag", fileName = "InventoryBagSO")]
    public class InventoryBagSO : ScriptableObject
    {
        public List<InventoryItem> ItemList;

        public InventoryItem GetInventoryItem(int id)
        {
            return ItemList.Find(inventoryItem => inventoryItem.ItemID == id);
        }
    }
}