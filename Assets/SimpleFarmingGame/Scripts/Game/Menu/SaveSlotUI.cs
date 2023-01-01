using System;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleFarmingGame.Game
{
    public static partial class EventSystem
    {
        public static event Action<int> StartNewGameEvent;

        public static void CallStartNewGameEvent(int index)
        {
            StartNewGameEvent?.Invoke(index);
        }

        public static event Action EndGameEvent;

        public static void CallEndGameEvent()
        {
            EndGameEvent?.Invoke();
        }
    }

    public class SaveSlotUI : MonoBehaviour
    {
        public Text TimeText;
        public Text SceneText;
        private Button m_CurrentButton;
        private DataSlot m_CurrentData;
        private int Index => transform.GetSiblingIndex();

        private void Awake()
        {
            m_CurrentButton = GetComponent<Button>();
            m_CurrentButton.onClick.AddListener(LoadGameData);
        }

        private void OnEnable()
        {
            SetupSlotUI();
        }

        private void LoadGameData()
        {
            if (m_CurrentData != null)
            {
                SaveLoadManager.Instance.Load(Index);
            }
            else
            {
                Debug.Log("NEW GAME");
                EventSystem.CallStartNewGameEvent(Index);
            }
        }

        private void SetupSlotUI()
        {
            m_CurrentData = SaveLoadManager.Instance.DataSlotList[Index];

            if (m_CurrentData != null)
            {
                TimeText.text = m_CurrentData.GameTime;
                SceneText.text = m_CurrentData.GameScene;
            }
            else
            {
                TimeText.text = "当前存档尚未开启";
                SceneText.text = "当前存档尚未开启";
            }
        }
    }
}