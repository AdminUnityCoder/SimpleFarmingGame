using System;

namespace SimpleFarmingGame.Game
{
    public enum GameState { Gameplay, Pause }

    public static partial class EventSystem
    {
        public static event Action<GameState> UpdateGameStateEvent;

        public static void CallUpdateGameStateEvent(GameState gameState)
        {
            UpdateGameStateEvent?.Invoke(gameState);
        }
    }
}