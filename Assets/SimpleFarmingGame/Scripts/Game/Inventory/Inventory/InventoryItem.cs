using System;
using UnityEngine;

namespace SimpleFarmingGame.Game
{
    /// <summary>
    /// 库存物品
    /// </summary>
    [Serializable]
    public struct InventoryItem
    {
        [Tooltip("物品ID")] public int ItemID;
        [Tooltip("物品数量")] public int ItemAmount;
    }

    /// <summary>
    /// 库存位置
    /// </summary>
    public enum InventoryLocation
    {
        [Tooltip("玩家背包")] PlayerBag
      , [Tooltip("储物箱")] StorageBox
    }
}