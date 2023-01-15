using System;
using UnityEngine;

namespace SimpleFarmingGame.Game
{
    public static partial class EventSystem
    {
        public static event Action RefreshCurrentSceneMap;

        public static void CallRefreshCurrentSceneMap()
        {
            RefreshCurrentSceneMap?.Invoke();
        }

        public static event Action<int, Vector3> OnBuildFurnitureEvent;

        public static void CallBuildFurnitureEvent(int buildingPaperID, Vector3 mouseWorldPosition)
        {
            OnBuildFurnitureEvent?.Invoke(buildingPaperID, mouseWorldPosition);
        }
    }
}