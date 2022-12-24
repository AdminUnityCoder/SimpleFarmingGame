using System.Collections.Generic;
using UnityEngine;

namespace SFG.MapSystem
{
    /// <summary>
    /// MapDataSO Contains "SceneName" and "TilePropertyList"
    /// </summary>
    [CreateAssetMenu(menuName = "ScriptableObject/Map/MapData", fileName = "MapDataSO")]
    public class MapDataSO : ScriptableObject
    {
        public string SceneName;
        [Header("地图信息")]
        public int MapWidth;
        public int MapHeight;
        [Header("原点坐标(左下角原点坐标)")] 
        public int OriginX;
        public int OriginY;
        public List<TileProperty> TilePropertyList;
    }
}