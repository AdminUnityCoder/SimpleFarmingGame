using System;

namespace SFG.Game
{
    public enum GameState { Gameplay, Pause }

    public static class EventSystem
    {
        public static event Action<GameState> UpdateGameStateEvent;

        public static void CallUpdateGameStateEvent(GameState gameState)
        {
            UpdateGameStateEvent?.Invoke(gameState);
        }
    }
}