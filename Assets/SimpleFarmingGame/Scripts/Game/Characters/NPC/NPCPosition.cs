using System;
using UnityEngine;

namespace SimpleFarmingGame.Game
{
    [Serializable]
    public class NPCPosition
    {
        public Transform NPCTransform;
        public string StartScene;
        [Tooltip("初始坐标")] public Vector3 InitialPosition;
    }
}