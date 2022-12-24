using System;
using UnityEngine;

namespace SFG.MapSystem
{
    /// <summary>
    /// 瓦片信息
    /// </summary>
    [Serializable]
    public class TileDetails
    {
        public int GridX, GridY;          // 坐标
        public bool CanDig;               // 是否可以挖坑
        public bool CanDropItem;          // 是否可以扔东西
        public bool CanPlaceFurniture;    // 是否可以放置家具
        public bool IsNPCObstacle;        // 是否是NPC障碍物
        public int DaysSinceDug = -1;     // 距离上一次挖坑已经过了多少天
        public int DaysSinceWatered = -1; // 距离上一次浇水已经过了多少天
        public int SeedItemID = -1;       // 种子ID
        [Tooltip("已经成长天数")] public int HaveGrownDays = -1;
        [Tooltip("已经收割过几次")] public int HasHarvestTimes = -1;
    }
}