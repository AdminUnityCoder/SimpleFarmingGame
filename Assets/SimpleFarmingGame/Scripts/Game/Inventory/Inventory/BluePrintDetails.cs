using System;
using UnityEngine;

namespace SimpleFarmingGame.Game
{
    /// <summary>
    /// 建造物品蓝图详情
    /// </summary>
    [Serializable]
    public class BluePrintDetails
    {
        [Header("建造图纸ID")] public int BuildingPaperID;
        [Header("建造家具所需要的资源")] public InventoryItem[] RequireResourceItems;
        [Header("建造成果预制体")] public GameObject BuildItemPrefab;
    }
}