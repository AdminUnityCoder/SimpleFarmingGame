using System;
using UnityEngine;

namespace SimpleFarmingGame.Game
{
    public static partial class EventSystem
    {
        public static event Action<Vector3, ItemDetails> MouseClickedEvent;

        public static void CallMouseClickedEvent(Vector3 mousePosition, ItemDetails itemDetails)
        {
            MouseClickedEvent?.Invoke(mousePosition, itemDetails);
        }

        public static event Action<Vector3, ItemDetails> ExecuteActionAfterAnimation;

        public static void CallExecuteActionAfterAnimation(Vector3 mousePosition, ItemDetails itemDetails)
        {
            ExecuteActionAfterAnimation?.Invoke(mousePosition, itemDetails);
        }
    }
}