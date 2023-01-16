using System;
using UnityEngine;

namespace SimpleFarmingGame.Game
{
    [Serializable]
    public class SerializableVector3
    {
        // 如果字段不是公开的，将无法保存，会导致场景中的物品的坐标全为 (0, 0, 0)
        public float X;
        public float Y;
        public float Z;

        /// <summary>
        /// 序列化Vector3
        /// </summary>
        /// <param name="position">三维坐标</param>
        public SerializableVector3(Vector3 position)
        {
            X = position.x;
            Y = position.y;
            Z = position.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(X, Y, Z);
        }

        public Vector2Int ToVector2Int()
        {
            return new Vector2Int((int)X, (int)Y);
        }
    }
}