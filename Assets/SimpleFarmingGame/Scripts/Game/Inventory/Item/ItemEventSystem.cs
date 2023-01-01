using System;
using UnityEngine;

namespace SimpleFarmingGame.Game
{
    public static partial class EventSystem
    {
        public static event Action<int, Vector3> InstantiateItemInScene;

        public static void CallInstantiateItemInScene(int itemID, Vector3 position)
        {
            InstantiateItemInScene?.Invoke(itemID, position);
        }

        public static event Action<int, Vector3, ItemType> DropItemEvent;

        public static void CallDropItemEvent(int itemID, Vector3 position, ItemType itemType)
        {
            DropItemEvent?.Invoke(itemID, position, itemType);
        }
    }
}