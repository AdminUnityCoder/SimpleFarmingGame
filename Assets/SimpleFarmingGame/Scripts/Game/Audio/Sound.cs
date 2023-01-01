using UnityEngine;

namespace SimpleFarmingGame.Game
{
    [RequireComponent(typeof(AudioSource))]
    public class Sound : MonoBehaviour
    {
        [SerializeField] private AudioSource AudioSource;

        public void SetSound(SoundDetails soundDetails)
        {
            AudioSource.clip = soundDetails.SoundClip;
            AudioSource.volume = soundDetails.SoundVolume;
            AudioSource.pitch = Random.Range(soundDetails.MinSoundPitch, soundDetails.MaxSoundPitch);
        }
    }
}