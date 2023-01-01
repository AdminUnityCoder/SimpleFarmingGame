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

        /// <summary>
        /// 根据传入的物品详情的物品类型匹配玩家动作
        /// </summary>
        /// <param name="itemDetails">物品详情</param>
        /// <returns>返回玩家动作</returns>
        public PlayerActionEnum MatchPlayerAction(ItemDetails itemDetails)
        {
            PlayerActionEnum retAction = itemDetails.ItemType switch
            {
                ItemType.Seed => PlayerActionEnum.Carry
              , ItemType.Commodity => PlayerActionEnum.Carry
              , ItemType.HoeTool => PlayerActionEnum.Hoe
              , ItemType.WaterTool => PlayerActionEnum.Water
              , ItemType.CollectTool => PlayerActionEnum.Collect
              , ItemType.AxeTool => PlayerActionEnum.Chop
              , ItemType.PickAxeTool => PlayerActionEnum.Break
              , ItemType.SickleTool => PlayerActionEnum.Reap
              , ItemType.Furniture => PlayerActionEnum.None
              , _ => PlayerActionEnum.None
            };
            return retAction;
        }
    }

    [CreateAssetMenu(menuName = "ScriptableObject/Inventory/ItemData", fileName = "ItemDataListSO")]
    public class ItemDataListSO : ScriptableObject
    {
        public List<ItemDetails> ItemDetailsList;

        public ItemDetails GetItemDetails(int itemID) =>
            ItemDetailsList.Find(itemDetails => itemDetails.ItemID == itemID);
    }
}