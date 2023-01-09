using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace SimpleFarmingGame.Game
{
    public class AudioManager : Singleton<AudioManager>
    {
        [Header("SoundDetails")] public SoundDetailsListSO SoundDetailsData;
        public SceneSoundListSO SceneSoundData;

        [Header("Audio Source")] public AudioSource AmbientSoundSource;
        public AudioSource BackgroundSoundSource;

        [Header("Audio Mixer")] public AudioMixer AudioMixer;

        [Header("Snapshot")] public AudioMixerSnapshot NormalSnapshot;
        public AudioMixerSnapshot AmbientSnapshot;
        public AudioMixerSnapshot MuteSnapshot;

        private Coroutine m_SoundCoroutine;
        private const float TimeToReach = 8f;
        public float MusicChangeTime => Random.Range(5f, 15f);

        private void OnEnable()
        {
            EventSystem.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
            EventSystem.PlaySoundEvent += OnPlaySoundEvent;
            EventSystem.EndGameEvent += OnEndGameEvent;
        }

        private void OnDisable()
        {
            EventSystem.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
            EventSystem.PlaySoundEvent -= OnPlaySoundEvent;
            EventSystem.EndGameEvent -= OnEndGameEvent;
        }

        private void OnAfterSceneLoadedEvent()
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            SceneSound sceneSound = SceneSoundData.GetSceneSound(currentSceneName);
            if (sceneSound == null) return;
            SoundDetails ambientSoundDetails = SoundDetailsData.GetSoundDetails(sceneSound.Ambient);
            SoundDetails backGroundSoundDetails = SoundDetailsData.GetSoundDetails(sceneSound.Music);

            if (m_SoundCoroutine != null)
            {
                StopCoroutine(m_SoundCoroutine);
            }
            else
            {
                m_SoundCoroutine = StartCoroutine(PlaySoundCoroutine(backGroundSoundDetails, ambientSoundDetails));
            }
        }

        private void OnPlaySoundEvent(SoundName soundName)
        {
            SoundDetails soundDetails = SoundDetailsData.GetSoundDetails(soundName);
            if (soundDetails != null)
            {
                EventSystem.CallInitSoundEvent(soundDetails);
            }
        }

        private void OnEndGameEvent()
        {
            if (m_SoundCoroutine != null)
            {
                StopCoroutine(m_SoundCoroutine);
            }

            MuteSnapshot.TransitionTo(1);
        }

        private void PlayBackgroundSoundClip(SoundDetails soundDetails, float transitionTime)
        {
            AudioMixer.SetFloat("MusicVolume", ConvertSoundVolume(soundDetails.SoundVolume));
            BackgroundSoundSource.clip = soundDetails.SoundClip;
            // isActiveAndEnabled: 报告一个游戏对象及其相关的行为是否处于激活和启用状态。
            if (BackgroundSoundSource.isActiveAndEnabled)
            {
                BackgroundSoundSource.Play();
            }

            NormalSnapshot.TransitionTo(transitionTime);
        }

        private void PlayAmbientSoundClip(SoundDetails soundDetails, float transitionTime)
        {
            AudioMixer.SetFloat("AmbientVolume", ConvertSoundVolume(soundDetails.SoundVolume));
            AmbientSoundSource.clip = soundDetails.SoundClip;
            // isActiveAndEnabled: 报告一个游戏对象及其相关的行为是否处于激活和启用状态。
            if (AmbientSoundSource.isActiveAndEnabled)
            {
                AmbientSoundSource.Play();
            }

            AmbientSnapshot.TransitionTo(transitionTime);
        }

        private IEnumerator PlaySoundCoroutine(SoundDetails backGround, SoundDetails ambient)
        {
            if (backGround != null && ambient != null)
            {
                PlayAmbientSoundClip(ambient, 1f);
                yield return new WaitForSeconds(MusicChangeTime);
                PlayBackgroundSoundClip(backGround, TimeToReach);
            }
        }

        private static float ConvertSoundVolume(float soundVolume) => soundVolume * 100 - 80;

        public void SetMasterVolume(float value)
        {
            AudioMixer.SetFloat("MasterVolume", ConvertSoundVolume(value));
        }
    }
}