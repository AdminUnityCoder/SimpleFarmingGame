using System.Collections.Generic;
using UnityEngine;

namespace SFG.Characters.NPC
{
    [CreateAssetMenu(fileName = "RouteMapListSO", menuName = "ScriptableObject/RouteMapDataList")]
    public class RouteMapDataListSO : ScriptableObject
    {
        public List<RouteMap> RouteMapList;
    }
}