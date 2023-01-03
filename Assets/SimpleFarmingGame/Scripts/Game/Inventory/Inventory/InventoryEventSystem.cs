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
    }
}