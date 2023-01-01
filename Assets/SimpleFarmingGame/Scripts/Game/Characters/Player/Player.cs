using UnityEngine;

namespace SimpleFarmingGame.Game
{
    public sealed class Player : MonoBehaviour
    {
        public const float SPEED = 5f;
        private static Player s_Instance;
        private IPlayer m_Player;
        private IPlayerInput m_PlayerInput;
        public static Player Instance => s_Instance;
        public Vector3 Position => transform.position;
        public bool IsMoving => m_Player.IsMoving;
        public float InputX => m_PlayerInput.InputX;
        public float InputY => m_PlayerInput.InputY;

        private void Awake()
        {
            m_PlayerInput = GetComponent<PlayerController>();
            m_Player = GetComponent<PlayerController>();
            s_Instance = this;
        }

        public void DisableInput() => m_PlayerInput.InputDisable = true;
        public void EnableInput() => m_PlayerInput.InputDisable = false;
    }
}