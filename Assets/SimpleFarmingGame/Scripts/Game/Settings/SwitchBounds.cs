using Cinemachine;
using UnityEngine;

namespace SimpleFarmingGame.Game
{
    public class SwitchBounds : MonoBehaviour
    {
        private CinemachineConfiner m_Confiner;

        private void Awake()
        {
            m_Confiner = GetComponent<CinemachineConfiner>();
        }

        private void OnEnable()
        {
            EventSystem.AfterSceneLoadedEvent += SwitchConfinerShape;
        }

        private void OnDisable()
        {
            EventSystem.AfterSceneLoadedEvent -= SwitchConfinerShape;
        }

        private void SwitchConfinerShape()
        {
            var boundingShape = GameObject.FindGameObjectWithTag("BoundsConfiner").GetComponent<PolygonCollider2D>();
            m_Confiner.m_BoundingShape2D = boundingShape;
            m_Confiner.InvalidatePathCache();
        }
    }
}