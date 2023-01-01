using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleFarmingGame.Game
{
    /// <summary>
    /// OldName:SceneRoute<br/>
    /// 路线图
    /// </summary>
    [Serializable]
    public class RouteMap
    {
        [SerializeField] private string WhereToWhere;
        public string FromSceneName;
        public string GotoSceneName;
        public List<Route> RouteList;
    }

    /// <summary>
    /// OldName:ScenePath<br/>
    /// 一条路线
    /// </summary>
    [Serializable]
    public class Route
    {
        public string SceneName;
        [Header("99999代表从任意地方去GotoGridCell")] public Vector2Int FromGridCell; // 来的坐标

        [Header("99999代表从任意地方去Schedule里面的TargetPosition")]
        public Vector2Int GotoGridCell; // 去的坐标
    }
}