using System.Collections.Generic;
using UnityEngine;

namespace SimpleFarmingGame.Game
{
    [CreateAssetMenu(fileName = "RouteMapListSO", menuName = "ScriptableObject/RouteMapDataList")]
    public class RouteMapDataListSO : ScriptableObject
    {
        public List<RouteMap> RouteMapList;
    }
}