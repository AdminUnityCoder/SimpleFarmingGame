using System;
using UnityEngine;
using UnityEngine.Playables;

namespace SimpleFarmingGame.Game
{
    [Serializable]
    public class DialogueBehaviour : PlayableBehaviour
    {
        private PlayableDirector m_Director;
        public Dialogue Dialogue;

        public override void OnPlayableCreate(Playable playable)
        {
            m_Director = playable.GetGraph().GetResolver() as PlayableDirector;
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            EventSystem.CallShowDialogueBoxEvent(this.Dialogue);
            if (Application.isPlaying)
            {
                if (this.Dialogue.NeedToPause)
                {
                    // 暂停Timeline
                    TimelineManager.Instance.PauseTimeline(this.m_Director);
                }
                else
                {
                    EventSystem.CallShowDialogueBoxEvent(null);
                }
            }
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (Application.isPlaying)
            {
                TimelineManager.Instance.IsDialogueFinished = this.Dialogue.IsFinished;
            }
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            EventSystem.CallShowDialogueBoxEvent(null);
        }

        public override void OnGraphStart(Playable playable)
        {
            EventSystem.CallUpdateGameStateEvent(GameState.Pause);
        }

        public override void OnGraphStop(Playable playable)
        {
            EventSystem.CallUpdateGameStateEvent(GameState.Gameplay);
        }
    }
}