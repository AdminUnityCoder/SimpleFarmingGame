using System;

namespace SimpleFarmingGame.Game
{
    public static partial class EventSystem
    {
        public static event Action<SlotType, InventoryBagSO> OnBaseBagOpenEvent;

        public static void CallBaseBagOpenEvent(SlotType slotType, InventoryBagSO bagData)
        {
            OnBaseBagOpenEvent?.Invoke(slotType, bagData);
        }

        public static event Action<SlotType, InventoryBagSO> OnBaseBagCloseEvent;

        public static void CallBaseBagCloseEvent(SlotType slotType, InventoryBagSO bagData)
        {
            OnBaseBagCloseEvent?.Invoke(slotType, bagData);
        }
    }
}