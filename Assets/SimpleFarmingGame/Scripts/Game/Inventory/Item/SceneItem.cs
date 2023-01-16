using System;
using UnityEngine;

namespace SimpleFarmingGame.Game
{
    /// <summary>
    /// 场景中的物体
    /// </summary>
    [Serializable]
    public class SceneItem
    {
        [Tooltip("物品ID")]public int ItemID;
        [Tooltip("坐标")] public SerializableVector3 Coordinate;
    }
}