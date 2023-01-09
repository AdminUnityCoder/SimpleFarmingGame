using UnityEngine;

namespace SimpleFarmingGame.Game
{
    public class ActionBarButton : MonoBehaviour
    {
        [SerializeField] private KeyCode Key;
        private SlotUI m_SlotUI;
        private bool m_CanUseKey;

        private void Awake()
        {
            m_SlotUI = GetComponent<SlotUI>();
        }

        private void OnEnable()
        {
            EventSystem.UpdateGameStateEvent += OnUpdateGameStateEvent;
        }

        private void OnDisable()
        {
            EventSystem.UpdateGameStateEvent -= OnUpdateGameStateEvent;
        }

        private void OnUpdateGameStateEvent(GameState gameState)
        {
            m_CanUseKey = gameState == GameState.Gameplay;
        }

        private void Update()
        {
            if (Input.GetKeyDown(Key) && m_CanUseKey)
            {
                if (m_SlotUI.ItemDetails != null)
                {
                    m_SlotUI.IsSelected = !m_SlotUI.IsSelected;
                    if (m_SlotUI.IsSelected)
                    {
                        m_SlotUI.m_InventoryUI.DisplaySlotHighlight(m_SlotUI.SlotIndex, m_SlotUI.SlotType);
                    }
                    else
                    {
                        m_SlotUI.m_InventoryUI.CancelDisplayAllSlotHighlight();
                    }

                    EventSystem.CallItemSelectedEvent(m_SlotUI.ItemDetails, m_SlotUI.IsSelected);
                }
            }
        }
    }
}