using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleFarmingGame.Game
{
    public class UIManager : MonoBehaviour
    {
        private GameObject m_MenuCanvas;
        public GameObject MenuPanelPrefab;

        public Button SettingsButton;
        public GameObject PausePanel;
        public Slider VolumeSlider;

        private void Awake()
        {
            SettingsButton.onClick.AddListener(TogglePausePanel);
            VolumeSlider.onValueChanged.AddListener(AudioManager.Instance.SetMasterVolume);
        }

        private void OnEnable()
        {
            EventSystem.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
        }

        private void OnDisable()
        {
            EventSystem.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
        }

        private void Start()
        {
            m_MenuCanvas = GameObject.FindWithTag("MenuCanvas");
            Instantiate(MenuPanelPrefab, m_MenuCanvas.transform);
        }

        private void OnAfterSceneLoadedEvent()
        {
            if (m_MenuCanvas.transform.childCount > 0)
            {
                Destroy(m_MenuCanvas.transform.GetChild(0).gameObject);
            }
        }

        private void TogglePausePanel()
        {
            bool isOpen = PausePanel.activeInHierarchy;
            if (isOpen)
            {
                PausePanel.SetActive(false);
                Time.timeScale = 1;
            }
            else
            {
                GC.Collect();
                PausePanel.SetActive(true);
                Time.timeScale = 0;
            }
        }

        public void ReturnMainMenu()
        {
            Time.timeScale = 1;
            StartCoroutine(ReturnMainMenuCoroutine());
        }

        private IEnumerator ReturnMainMenuCoroutine()
        {
            PausePanel.SetActive(false);
            EventSystem.CallEndGameEvent();
            yield return new WaitForSeconds(1f);
            Instantiate(MenuPanelPrefab, m_MenuCanvas.transform);
        }
    }
}