using System.Collections.Generic;
using SFG.Game;
using SFG.InventorySystem;
using SFG.Save;
using UnityEngine;
using EventSystem = SFG.TransitionSystem.EventSystem;

namespace SFG.Characters.Player
{
    // Control player input and movement
    internal sealed class PlayerInput : MonoBehaviour, IPlayerInput, ISavable
    {
        private Rigidbody2D m_Rigidbody2D; // component

        private Vector2 m_MovementInput;
        private float m_InputX;
        private float m_InputY;
        private bool m_IsMoving;
        private bool m_InputDisable;
        private const float Speed = 5f;
        float IPlayerInput.InputX => m_InputX;
        float IPlayerInput.InputY => m_InputY;
        bool IPlayerInput.IsMoving => m_IsMoving;
        Vector3 IPlayerInput.Position => transform.position;

        bool IPlayerInput.InputDisable
        {
            get => m_InputDisable;
            set => m_InputDisable = value;
        }

        private void Awake()
        {
            m_Rigidbody2D = GetComponent<Rigidbody2D>();
            m_InputDisable = true;
        }

        private void Start()
        {
            ISavable savable = this;
            savable.RegisterSavable();
        }

        private void OnEnable()
        {
            EventSystem.BeforeSceneUnloadedEvent += OnBeforeSceneUnloadedEvent; // TransitionManager
            EventSystem.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;       // TransitionManager
            EventSystem.MoveToPositionEvent += OnMoveToPositionEvent;           // TransitionManager
            Game.EventSystem.UpdateGameStateEvent += OnUpdateGameStateEvent;    // Game
            UI.EventSystem.StartNewGameEvent += OnStartNewGameEvent;            // UI
            UI.EventSystem.EndGameEvent += OnEndGameEvent;
        }

        private void OnDisable()
        {
            EventSystem.BeforeSceneUnloadedEvent -= OnBeforeSceneUnloadedEvent; // TransitionManager
            EventSystem.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;       // TransitionManager
            EventSystem.MoveToPositionEvent -= OnMoveToPositionEvent;           // TransitionManager
            Game.EventSystem.UpdateGameStateEvent -= OnUpdateGameStateEvent;
            UI.EventSystem.StartNewGameEvent -= OnStartNewGameEvent; // UI
            UI.EventSystem.EndGameEvent -= OnEndGameEvent;
        }

        private void Update()
        {
            if (m_InputDisable == false)
            {
                MoveInput();
            }
            else
            {
                m_IsMoving = false;
            }
        }

        private void FixedUpdate()
        {
            if (m_InputDisable == false)
            {
                Movement();
            }
        }

        private void MoveInput()
        {
            m_InputX = Input.GetAxisRaw("Horizontal");
            m_InputY = Input.GetAxisRaw("Vertical");

            if (m_InputX != 0 && m_InputY != 0)
            {
                m_InputX *= 0.6f;
                m_InputY *= 0.6f;
            }

            if (Input.GetKey(KeyCode.LeftShift))
            {
                m_InputX *= 0.5f;
                m_InputY *= 0.5f;
            }

            m_MovementInput = new Vector2(m_InputX, m_InputY);
            m_IsMoving = m_MovementInput != Vector2.zero;
        }

        private void Movement() =>
            m_Rigidbody2D.MovePosition(m_Rigidbody2D.position + Speed * Time.deltaTime * m_MovementInput);

        private void OnBeforeSceneUnloadedEvent() => m_InputDisable = true; // Disable input, player can not move.

        private void OnAfterSceneLoadedEvent() => m_InputDisable = false; // Enable input, player can move.

        private void OnMoveToPositionEvent(Vector3 targetPosition) => transform.position = targetPosition;

        private void OnUpdateGameStateEvent(GameState gameState)
        {
            switch (gameState)
            {
                case GameState.Gameplay:
                    m_InputDisable = false;
                    break;
                case GameState.Pause:
                    m_InputDisable = true;
                    break;
            }
        }

        private void OnStartNewGameEvent(int obj)
        {
            m_InputDisable = false;
            transform.position = new Vector3(0f, -3f, 0f);
        }

        private void OnEndGameEvent()
        {
            m_InputDisable = true;
        }

        public string GUID => GetComponent<DataGUID>().GUID;

        /// <summary>
        /// 保存玩家坐标
        /// </summary>
        /// <returns></returns>
        public GameSaveData GenerateSaveData()
        {
            GameSaveData saveData = new GameSaveData();
            saveData.CharactersPosDict = new Dictionary<string, SerializableVector3>();
            saveData.CharactersPosDict.Add(name, new SerializableVector3(transform.position));

            return saveData;
        }

        /// <summary>
        /// 恢复玩家坐标
        /// </summary>
        /// <param name="saveData"></param>
        public void RestoreData(GameSaveData saveData)
        {
            Vector3 targetPosition = saveData.CharactersPosDict[name].ToVector3();
            transform.position = targetPosition;
        }
    }
}