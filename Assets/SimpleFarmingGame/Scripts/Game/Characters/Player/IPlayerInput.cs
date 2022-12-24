using UnityEngine;

namespace SFG.Characters.Player
{
    internal interface IPlayerInput
    {
        float InputX { get; }
        float InputY { get; }
        bool IsMoving { get; }
        Vector3 Position { get; }
        bool InputDisable { get; set; }
    }
}