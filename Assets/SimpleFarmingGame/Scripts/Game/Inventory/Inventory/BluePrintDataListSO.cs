using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace SFG.InventorySystem
{
    [CreateAssetMenu(fileName = "BluePrintDataListSO", menuName = "ScriptableObject/BluePrint")]
    public class BluePrintDataListSO : ScriptableObject
    {
        public List<BluePrintDetails> BluePrintDataList;

        public BluePrintDetails GetBluePrintDetails(int buildingPaperID)
        {
            return BluePrintDataList.Find(bluePrintDetails => bluePrintDetails.BuildingPaperID == buildingPaperID);
        }
    }

    [Serializable]
    public class BluePrintDetails
    {
        [Header("建造图纸ID")] public int BuildingPaperID;
        [Header("建造家具所需要的资源")] public InventoryItem[] RequireResourceItems;
        public GameObject BuildItemPrefab;
    }
}