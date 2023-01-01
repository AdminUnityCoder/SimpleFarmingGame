using System;

namespace SimpleFarmingGame.Game
{
    public static partial class EventSystem
    {
        public static event Action<SlotType, InventoryBagSO> BaseBagOpenEvent;

        public static void CallBaseBagOpenEvent(SlotType slotType, InventoryBagSO bagData)
        {
            BaseBagOpenEvent?.Invoke(slotType, bagData);
        }

        public static event Action<SlotType, InventoryBagSO> BaseBagCloseEvent;

        public static void CallBaseBagCloseEvent(SlotType slotType, InventoryBagSO bagData)
        {
            BaseBagCloseEvent?.Invoke(slotType, bagData);
        }
    }
}