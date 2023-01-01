using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace SimpleFarmingGame.Game
{
    public static partial class EventSystem
    {
        public static event Action<Season, LightShift, float> LightShiftChangeEvent;

        public static void CallLightShiftChangeEvent(Season season, LightShift lightShift, float timeDifference)
        {
            LightShiftChangeEvent?.Invoke(season, lightShift, timeDifference);
        }
    }

    public class LightController : MonoBehaviour
    {
        public LightData LightData;
        private Light2D m_CurrentLight;
        private LightDetails m_CurrentLightDetails;

        private void Awake()
        {
            m_CurrentLight = GetComponent<Light2D>();
        }

        // 实际切换灯光
        public void ChangeLightShift(Season season, LightShift lightShift, float timeDifference)
        {
            m_CurrentLightDetails = LightData.GetLightDetails(season, lightShift);

            if (timeDifference < LightModel.LightChangeDuration)
            {
                Color colorOffset = (m_CurrentLightDetails.LightColor - m_CurrentLight.color)
                  / LightModel.LightChangeDuration
                  * timeDifference;
                m_CurrentLight.color += colorOffset;

                DOTween.To
                (
                    getter: () => m_CurrentLight.color
                  , setter: color => m_CurrentLight.color = color
                  , endValue: m_CurrentLightDetails.LightColor
                  , duration: LightModel.LightChangeDuration - timeDifference
                );

                DOTween.To
                (
                    getter: () => m_CurrentLight.intensity
                  , setter: intensity => m_CurrentLight.intensity = intensity
                  , endValue: m_CurrentLightDetails.LightIntensity
                  , duration: LightModel.LightChangeDuration - timeDifference
                );
            }

            if (timeDifference >= LightModel.LightChangeDuration)
            {
                m_CurrentLight.color = m_CurrentLightDetails.LightColor;
                m_CurrentLight.intensity = m_CurrentLightDetails.LightIntensity;
            }
        }
    }
}