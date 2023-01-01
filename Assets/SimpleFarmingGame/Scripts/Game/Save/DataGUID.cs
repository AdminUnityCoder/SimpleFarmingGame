using System;
using UnityEngine;

namespace SimpleFarmingGame.Game
{
    [ExecuteAlways]
    public class DataGUID : MonoBehaviour
    {
        /// <summary>
        /// 表示全局唯一标识符 (GUID)。
        /// GUID 是一个 128 位整数 (16 字节) ，可在需要唯一标识符的所有计算机和网络中使用。 此类标识符重复的可能性非常低。
        /// </summary>
        public string GUID;

        private void Awake()
        {
            if (GUID == string.Empty)
            {
                // NewGuid(): 初始化 Guid 结构的新实例。返回一个新的 GUID 对象
                GUID = Guid.NewGuid().ToString();
            }
        }
    }
}