using System;
using System.Collections.Generic;

namespace SimpleFarmingGame.Game
{
    public static partial class EventSystem
    {
        public static Action<InventoryLocation, List<InventoryItem>> RefreshInventoryUI;

        public static void CallRefreshInventoryUI(InventoryLocation location, List<InventoryItem> itemList)
        {
            RefreshInventoryUI?.Invoke(location, itemList);
        }

        public static event Action<ItemDetails, bool> ItemSelectedEvent;

        public static void CallItemSelectedEvent(ItemDetails itemDetails, bool isSelected)
        {
            ItemSelectedEvent?.Invoke(itemDetails, isSelected);
        }

        public static event Action<ItemDetails, bool> ShowTransactionUIEvent;

        /// <param name="itemDetails">物品详情</param>
        /// <param name="isSell">是否是卖</param>
        public static void CallShowTransactionUIEvent(ItemDetails itemDetails, bool isSell)
        {
            ShowTransactionUIEvent?.Invoke(itemDetails, isSell);
        }
    }
}