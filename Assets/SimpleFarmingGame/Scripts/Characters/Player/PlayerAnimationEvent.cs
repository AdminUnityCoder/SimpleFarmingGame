using SFG.AudioSystem;
using UnityEngine;

namespace SFG.Characters.Player
{
    /// <summary>
    /// 给 Animation 添加 Event，用于播放 Sound
    /// </summary>
    public class PlayerAnimationEvent : MonoBehaviour
    {
        public void FootStepSoftSound()
        {
            EventSystem.CallPlaySoundEvent(SoundName.FootStepSoft);
        }

        // public void FootStepHardSound()
        // {
        //     EventSystem.CallPlaySoundEvent(SoundName.FootStepHard);
        // }
    }
}