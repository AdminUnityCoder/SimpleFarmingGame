using System.Collections.Generic;
using UnityEngine;

namespace SimpleFarmingGame.Game
{
    [CreateAssetMenu(fileName = "BluePrintDataListSO", menuName = "ScriptableObject/BluePrint")]
    public class BluePrintDataListSO : ScriptableObject
    {
        /// <summary>
        /// 蓝图数据列表
        /// </summary>
        public List<BluePrintDetails> BluePrintDataList;

        /// <summary>
        /// 根据传入的<paramref name="buildingPaperID"/>返回蓝图数据列表中对应的蓝图详情
        /// </summary>
        /// <param name="buildingPaperID">建造图纸ID</param>
        /// <returns>返回蓝图详情</returns>
        public BluePrintDetails GetBluePrintDetails(int buildingPaperID)
        {
            return BluePrintDataList.Find(bluePrintDetails => bluePrintDetails.BuildingPaperID == buildingPaperID);
        }
    }
}