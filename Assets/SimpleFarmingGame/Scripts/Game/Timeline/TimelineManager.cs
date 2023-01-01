using UnityEngine;
using UnityEngine.Playables;

namespace SimpleFarmingGame.Game
{
    public class TimelineManager : Singleton<TimelineManager>
    {
        public PlayableDirector StartDirector;
        private PlayableDirector m_CurrentDirector;

        private bool m_IsPause;
        private bool m_IsDialogueFinished;

        public bool IsDialogueFinished
        {
            set => m_IsDialogueFinished = value;
        }

        protected override void Awake()
        {
            base.Awake();
            m_CurrentDirector = StartDirector;
        }

        private void OnEnable()
        {
            // m_CurrentDirector.played += OnTimelinePlayed;
            // m_CurrentDirector.stopped += OnTimelineStopped;
            EventSystem.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
        }

        private void OnDisable()
        {
            EventSystem.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
        }

        private void Update()
        {
            if (m_IsPause && Input.GetKeyDown(KeyCode.Space) && m_IsDialogueFinished)
            {
                m_CurrentDirector.playableGraph.GetRootPlayable(0).SetSpeed(1d);
                m_IsPause = false;
            }
        }

        public void PauseTimeline(PlayableDirector director)
        {
            m_CurrentDirector = director;
            m_CurrentDirector.playableGraph.GetRootPlayable(0).SetSpeed(0d);
            m_IsPause = true;
        }

        // private void OnTimelinePlayed(PlayableDirector director)
        // {
        //     if (director != null)
        //     {
        //         Game.EventSystem.CallUpdateGameStateEvent(GameState.Pause);
        //     }
        // }
        //
        // private void OnTimelineStopped(PlayableDirector director)
        // {
        //     if (director != null)
        //     {
        //         Game.EventSystem.CallUpdateGameStateEvent(GameState.Gameplay);
        //         director.gameObject.SetActive(false);
        //     }
        // }

        private void OnAfterSceneLoadedEvent()
        {
            m_CurrentDirector = FindObjectOfType<PlayableDirector>();
            if (m_CurrentDirector != null)
            {
                m_CurrentDirector.Play();
            }
        }
    }
}