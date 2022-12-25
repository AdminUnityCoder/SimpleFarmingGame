using UnityEngine;

namespace SimpleFarmingGame.Game
{
    public sealed class PlayerModel : MonoBehaviour
    {
        private IPlayerInput m_PlayerInput;

        private static PlayerModel s_Instance;
        public static PlayerModel Instance => s_Instance;

        private void Awake()
        {
            m_PlayerInput = GetComponent<IPlayerInput>();
            s_Instance = this;
        }

        public Vector3 GetPosition => m_PlayerInput.Position;
        public float GetInputX => m_PlayerInput.InputX;
        public float GetInputY => m_PlayerInput.InputY;
        public bool GetIsMoving => m_PlayerInput.IsMoving;
        public void DisableInput() => m_PlayerInput.InputDisable = true;
        public void EnableInput() => m_PlayerInput.InputDisable = false;
    }
}