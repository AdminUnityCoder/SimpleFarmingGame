using System;
using System.Collections.Generic;

namespace SimpleFarmingGame.Game
{
    public static partial class EventSystem
    {
        public static Action<InventoryLocation, List<InventoryItem>> UpdateInventoryUI;

        public static void CallUpdateInventoryUI(InventoryLocation location, List<InventoryItem> itemList)
        {
            UpdateInventoryUI?.Invoke(location, itemList);
        }
    }
}