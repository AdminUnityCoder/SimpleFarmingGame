using System;
using UnityEngine;

namespace SimpleFarmingGame.Game
{
    /// <summary>
    /// 瓦片（格子）属性，包含瓦片类型（TileType）、瓦片坐标（TileCoordinate）
    /// </summary>
    [Serializable]
    public class TileProperty
    {
        /// <summary>
        /// 瓦片（格子）类型
        /// </summary>
        public TileType TileType;
        /// <summary>
        /// 瓦片坐标
        /// </summary>
        public Vector2Int TileCoordinate;
        public bool BoolTypeValue;
    }
}