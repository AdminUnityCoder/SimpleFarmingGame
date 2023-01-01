using System;
using System.Collections.Generic;
using SimpleFarmingGame.Game;
using UnityEngine;

namespace SFG.InventorySystem
{
    public enum ItemType
    {
        Seed        // 种子
      , Commodity   // 商品
      , Furniture   // 家具
      , HoeTool     // 锄头
      , AxeTool     // 斧头
      , PickAxeTool // 十字镐
      , SickleTool  // 镰刀
      , WaterTool   // 水壶
      , CollectTool // 菜篮
      , Reapable    //可收获的
    }

    [Serializable]
    public class ItemDetails
    {
        public string ItemName;
        public int ItemID;
        public ItemType ItemType;
        public Sprite ItemIcon;
        public Sprite ItemIconOnWorld;
        public string ItemDescription;
        public int ItemUseRadius;
        public bool CanPickedUp;
        public bool CanDropped;
        public bool CanCarried;
        public int ItemPrice;
        [Range(0, 1)] public float SellPercentage;
    }

    [CreateAssetMenu(menuName = "ScriptableObject/Inventory/ItemData", fileName = "ItemDataListSO")]
    public class ItemDataListSO : ScriptableObject
    {
        public List<ItemDetails> ItemDetailsList;

        public ItemDetails GetItemDetails(int itemID) =>
            ItemDetailsList.Find(itemDetails => itemDetails.ItemID == itemID);
    }
}