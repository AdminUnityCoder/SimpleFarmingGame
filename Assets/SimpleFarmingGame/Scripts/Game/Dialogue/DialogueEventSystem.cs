using System;

namespace SimpleFarmingGame.Game
{
    public static partial class EventSystem
    {
        public static event Action<Dialogue> ShowDialogueBoxEvent;

        public static void CallShowDialogueBoxEvent(Dialogue dialogue)
        {
            ShowDialogueBoxEvent?.Invoke(dialogue);
        }
    }
}