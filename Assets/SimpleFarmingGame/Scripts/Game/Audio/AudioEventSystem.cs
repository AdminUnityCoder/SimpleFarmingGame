using System;

namespace SimpleFarmingGame.Game
{
    public static partial class EventSystem
    {
        public static event Action<SoundDetails> InitSoundEffectEvent;

        public static void CallInitSoundEvent(SoundDetails soundDetails)
        {
            InitSoundEffectEvent?.Invoke(soundDetails);
        }

        public static event Action<SoundName> PlaySoundEvent;

        public static void CallPlaySoundEvent(SoundName soundName)
        {
            PlaySoundEvent?.Invoke(soundName);
        }
    }
}