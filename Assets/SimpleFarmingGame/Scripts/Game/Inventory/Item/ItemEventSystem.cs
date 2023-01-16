using System;
using UnityEngine;

namespace SimpleFarmingGame.Game
{
    public static partial class EventSystem
    {
        public static event Action<int, Vector3> OnInstantiateItemInScene;

        public static void CallInstantiateItemInScene(int itemID, Vector3 position)
        {
            OnInstantiateItemInScene?.Invoke(itemID, position);
        }

        public static event Action<int, Vector3, ItemType> OnDropItemEvent;

        public static void CallDropItemEvent(int itemID, Vector3 position, ItemType itemType)
        {
            OnDropItemEvent?.Invoke(itemID, position, itemType);
        }
    }
}