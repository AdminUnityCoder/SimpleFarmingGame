using System.Collections;
using SFG.Save;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SFG.TransitionSystem
{
    public class TransitionManager : Singleton<TransitionManager>, ISavable
    {
        public string StartSceneName = string.Empty;
        private CanvasGroup m_FadeCanvasGroup;
        private bool m_IsFinishFadeAnim;
        private const float FadeDuration = 1.5f; // TODO: Move to TransitionModel Script.

        private void OnEnable()
        {
            EventSystem.TransitionEvent += OnTransitionEvent;
            UI.EventSystem.StartNewGameEvent += OnStartNewGameEvent;
            UI.EventSystem.EndGameEvent += OnEndGameEvent;
        }

        private void OnDisable()
        {
            EventSystem.TransitionEvent -= OnTransitionEvent;
            UI.EventSystem.StartNewGameEvent -= OnStartNewGameEvent;
            UI.EventSystem.EndGameEvent -= OnEndGameEvent;
        }

        protected override void Awake()
        {
            base.Awake();
            SceneManager.LoadScene("UI", LoadSceneMode.Additive);
        }

        private void Start()
        {
            ISavable savable = this;
            savable.RegisterSavable();

            m_FadeCanvasGroup = GameObject.Find("Fade Panel").GetComponent<CanvasGroup>();
        }

        private void OnTransitionEvent(string sceneName, Vector3 position)
        {
            if (m_IsFinishFadeAnim == false)
            {
                StartCoroutine(TransitionScene(sceneName, position));
            }
        }

        private void OnStartNewGameEvent(int obj)
        {
            StartCoroutine(LoadSaveDataSceneCoroutine(StartSceneName));
        }

        private void OnEndGameEvent()
        {
            StartCoroutine(UnloadSceneCoroutine());
        }

        private IEnumerator TransitionScene(string sceneName, Vector3 targetPosition)
        {
            EventSystem.CallBeforeSceneUnloadedEvent();
            yield return Fade(1f);
            yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
            yield return LoadSceneAndSetActive(sceneName);
            EventSystem.CallMoveToPositionEvent(targetPosition);
            EventSystem.CallAfterSceneLoadedEvent();
            yield return Fade(0f);
        }

        private IEnumerator LoadSceneAndSetActive(string sceneName)
        {
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            Scene newScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            SceneManager.SetActiveScene(newScene);
        }

        private IEnumerator Fade(float targetAlpha)
        {
            m_IsFinishFadeAnim = true;
            m_FadeCanvasGroup.blocksRaycasts = true;

            float speed = Mathf.Abs(m_FadeCanvasGroup.alpha - targetAlpha) / FadeDuration; // speed  = distance / time

            while (!Mathf.Approximately(m_FadeCanvasGroup.alpha, targetAlpha))
            {
                m_FadeCanvasGroup.alpha
                    = Mathf.MoveTowards(m_FadeCanvasGroup.alpha, targetAlpha, speed * Time.deltaTime);
                yield return null;
            }

            m_FadeCanvasGroup.blocksRaycasts = false;
            m_IsFinishFadeAnim = false;
        }

        public string GUID => GetComponent<DataGUID>().GUID;

        public GameSaveData GenerateSaveData()
        {
            GameSaveData saveData = new GameSaveData();
            saveData.DataSceneName = SceneManager.GetActiveScene().name;
            return saveData;
        }

        public void RestoreData(GameSaveData saveData)
        {
            StartCoroutine(LoadSaveDataSceneCoroutine(saveData.DataSceneName));
        }

        private IEnumerator LoadSaveDataSceneCoroutine(string sceneName)
        {
            yield return Fade(1f);
            if (SceneManager.GetActiveScene().name != "PersistentScene") // 在游戏过程中，加载另外的游戏进度
            {
                EventSystem.CallBeforeSceneUnloadedEvent();
                // 在 BuildSettings 中返回场景的索引。
                yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
            }

            yield return LoadSceneAndSetActive(sceneName);
            EventSystem.CallAfterSceneLoadedEvent();
            yield return Fade(0);
        }

        private IEnumerator UnloadSceneCoroutine()
        {
            EventSystem.CallBeforeSceneUnloadedEvent();
            yield return Fade(1f);
            yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
            yield return Fade(0f);
        }
    }
}