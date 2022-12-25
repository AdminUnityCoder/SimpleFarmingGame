using UnityEngine;

namespace SimpleFarmingGame.Game
{
    internal interface IPlayerInput
    {
        float InputX { get; }
        float InputY { get; }
        bool IsMoving { get; }
        Vector3 Position { get; }
        bool InputDisable { set; }
    }
}