using System;
using UnityEngine;

namespace SFG.AStar
{
    public class Node : IComparable<Node>
    {
        public Vector2Int GridPosition;
        [Tooltip("到起点的距离")] public int GCost;
        [Tooltip("到终点的距离")] public int HCost;
        [Tooltip("格子的权重，该值越低权重越高")] public int FCost => GCost + HCost;
        [Tooltip("当前格子是否是障碍")] public bool IsNPCObstacle;
        [Tooltip("父节点")] public Node ParentNode;

        public Node(Vector2Int gridPosition)
        {
            GridPosition = gridPosition;
            ParentNode = null;
        }

        /// <summary>
        /// 比较两个 Node 节点之间的 FCost
        /// </summary>
        /// <param name="other">另一个节点</param>
        /// <returns>等于返回0，大于返回1，小于返回-1</returns>
        public int CompareTo(Node other)
        {
            int result = this.FCost.CompareTo(other.FCost);
            if (result == 0) // FCost 相等
            {
                // FCost 相等比较 HCost（到终点的距离）
                result = this.HCost.CompareTo(other.HCost);
            }

            return result;
        }
    }
}