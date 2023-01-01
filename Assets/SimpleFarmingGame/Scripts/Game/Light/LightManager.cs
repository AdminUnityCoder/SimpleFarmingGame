using System;
using UnityEngine;

namespace SimpleFarmingGame.Game
{
    public static class LightModel
    {
        public static TimeSpan MorningTime = new TimeSpan(5, 0, 0);
        public static TimeSpan NightTime = new TimeSpan(19, 0, 0);
        public const float LightChangeDuration = 25f;
    }

    public class LightManager : MonoBehaviour
    {
        private LightController[] m_SceneLights;
        private LightShift m_CurrentLightShift;
        private Season m_CurrentSeason;
        private float m_TimeDifference = LightModel.LightChangeDuration;

        private void OnEnable()
        {
            EventSystem.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
            EventSystem.LightShiftChangeEvent += OnLightShiftChangeEvent;
            EventSystem.StartNewGameEvent += OnStartNewGameEvent;
        }

        private void OnDisable()
        {
            EventSystem.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
            EventSystem.LightShiftChangeEvent -= OnLightShiftChangeEvent;
            EventSystem.StartNewGameEvent -= OnStartNewGameEvent;
        }

        private void OnAfterSceneLoadedEvent()
        {
            m_SceneLights = FindObjectsOfType<LightController>();
            foreach (LightController lightController in m_SceneLights)
            {
                // lightController改变灯光的方法
                lightController.ChangeLightShift(m_CurrentSeason, m_CurrentLightShift, m_TimeDifference);
            }
        }

        private void OnLightShiftChangeEvent(Season season, LightShift lightShift, float timeDifference)
        {
            m_CurrentSeason = season;
            m_TimeDifference = timeDifference;
            if (m_CurrentLightShift != lightShift)
            {
                m_CurrentLightShift = lightShift;
                foreach (LightController lightController in m_SceneLights)
                {
                    // lightController改变灯光的方法
                    lightController.ChangeLightShift(m_CurrentSeason, m_CurrentLightShift, m_TimeDifference);
                }
            }
        }

        private void OnStartNewGameEvent(int obj)
        {
            m_CurrentLightShift = LightShift.Morning;
        }
    }
}