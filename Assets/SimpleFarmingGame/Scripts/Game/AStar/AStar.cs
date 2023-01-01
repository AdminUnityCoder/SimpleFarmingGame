using System.Collections.Generic;
using UnityEngine;

namespace SimpleFarmingGame.Game
{
    public class AStar : Singleton<AStar>
    {
        private GridNodes m_GridNodes;
        private Node m_StartNode;

        private Node m_TargetNode;

        // MapData
        private int m_MapWidth;
        private int m_MapHeight;
        private int m_OriginX;
        private int m_OriginY;

        private int m_CurrentNodeSurroundingNodeX;
        private int m_CurrentNodeSurroundingNodeY;

        [Tooltip("存放当前选中Node的周围8个Node")] private List<Node> m_OpenNodeList;
        [Tooltip("最终被选中的Node")] private HashSet<Node> m_CloseNodeList; // Contain快，Add慢

        private bool m_IsFoundPath;

        /// <summary>
        /// 构建路径更新 NPC MovementStep Stack 中的每一步
        /// </summary>
        /// <param name="sceneName">场景名字</param>
        /// <param name="startPosition">起始坐标</param>
        /// <param name="targetPosition">目标坐标</param>
        /// <param name="npcMovementSteps">NPC 的 MovementStep 堆栈</param>
        public void BuildPath
        (
            string sceneName
          , Vector2Int startPosition
          , Vector2Int targetPosition
          , Stack<MovementStep> npcMovementSteps
        )
        {
            m_IsFoundPath = false;

            if (GenerateGridNodes(sceneName, startPosition, targetPosition))
            {
                // 查找路径
                if (FindShortestPath())
                {
                    // 构建 NPC 移动路径
                    UpdatePathOnMovementStepStack(sceneName, npcMovementSteps);
                }
            }
        }

        /// <summary>
        /// 生成网格节点信息
        /// </summary>
        /// <param name="sceneName">场景名字</param>
        /// <param name="startPosition">起始坐标</param>
        /// <param name="targetPosition">目标坐标</param>
        /// <returns></returns>
        private bool GenerateGridNodes(string sceneName, Vector2Int startPosition, Vector2Int targetPosition)
        {
            if (TileMapManager.Instance.GetMapDimensions
            (
                sceneName
              , out Vector2Int mapDimensions
              , out Vector2Int mapOrigin
            ))
            {
                // 根据瓦片地图范围构建网格移动节点范围数组
                m_GridNodes = new GridNodes(mapDimensions.x, mapDimensions.y);
                m_MapWidth = mapDimensions.x;
                m_MapHeight = mapDimensions.y;
                m_OriginX = mapOrigin.x;
                m_OriginY = mapOrigin.y;
                m_OpenNodeList = new List<Node>();
                m_CloseNodeList = new HashSet<Node>();
            }
            else
            {
                return false;
            }

            // 由于GridNodes数组是从 [0, 0] 开始的，所以要将原点（Origin）坐标减掉才能得到实际的坐标
            m_StartNode = m_GridNodes.GetGridNode(startPosition.x   - m_OriginX, startPosition.y  - m_OriginY);
            m_TargetNode = m_GridNodes.GetGridNode(targetPosition.x - m_OriginX, targetPosition.y - m_OriginY);

            for (int x = 0; x < m_MapWidth; ++x)
            {
                for (int y = 0; y < m_MapHeight; ++y)
                {
                    Vector3Int tilePosition = new Vector3Int(x + m_OriginX, y + m_OriginY, 0);
                    string key = tilePosition.x + "x" + tilePosition.y + "y" + sceneName;
                    TileDetails tileDetails = TileMapManager.Instance.GetTileDetailsFromDictionary(key);

                    if (tileDetails == null) continue;
                    Node node = m_GridNodes.GetGridNode(x, y);
                    if (tileDetails.IsNPCObstacle)
                    {
                        node.IsNPCObstacle = true;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 查找最短的路径并将符合的 Node 添加到 CloseNodeList
        /// </summary>
        /// <returns></returns>
        private bool FindShortestPath()
        {
            m_OpenNodeList.Add(m_StartNode); // 添加起点

            while (m_OpenNodeList.Count > 0)
            {
                m_OpenNodeList.Sort(); // 节点排序，Node 内含 CompareTo 比较方法
                Node closeNode = m_OpenNodeList[0];
                m_OpenNodeList.RemoveAt(0);
                m_CloseNodeList.Add(closeNode);
                if (closeNode == m_TargetNode)
                {
                    m_IsFoundPath = true;
                    break;
                }

                // 将周围 8 个 Node 添加进 m_OpenNodeList
                EvaluateSurroundingNodes(closeNode);
            }

            return m_IsFoundPath;
        }

        /// <summary>
        /// 评估 currentNode 周围的 8 个节点，并生成对应的消耗指
        /// </summary>
        /// <param name="currentNode">当前节点</param>
        private void EvaluateSurroundingNodes(Node currentNode)
        {
            Vector2Int currentNodeGridPos = currentNode.GridPosition;
            for (int x = -1; x <= 1; ++x)
            {
                for (int y = -1; y <= 1; ++y)
                {
                    if (x == 0 && y == 0)
                    {
                        continue;
                    }

                    // 应传入当前节点的GridPosition的x和y
                    Node validSurroundingNode = GetValidSurroundingNode(currentNodeGridPos, x, y);
                    if (validSurroundingNode != null)
                    {
                        if (m_OpenNodeList.Contains(validSurroundingNode) == false)
                        {
                            validSurroundingNode.GCost
                                = currentNode.GCost + GetDistance(currentNode, validSurroundingNode);
                            validSurroundingNode.HCost = GetDistance(validSurroundingNode, m_TargetNode);
                            // 链接父节点
                            validSurroundingNode.ParentNode = currentNode;
                            m_OpenNodeList.Add(validSurroundingNode);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取当前节点周围有效的 Node
        /// </summary>
        /// <param name="currentNodeGridPos">当前节点的网格坐标</param>
        /// <param name="x">当前节点的x轴坐标 + for循环中的x</param>
        /// <param name="y">当前节点的y轴坐标 + for循环中的y</param>
        /// <returns>返回一个有效的 Node</returns>
        /// <remarks>有效的 Node：非障碍，非已经加入 CloseNodeList 的Node</remarks>
        private Node GetValidSurroundingNode(Vector2Int currentNodeGridPos, int x, int y)
        {
            m_CurrentNodeSurroundingNodeX = currentNodeGridPos.x + x;
            m_CurrentNodeSurroundingNodeY = currentNodeGridPos.y + y;

            // 先判断 x，y 是否有效
            if (m_CurrentNodeSurroundingNodeX >= m_MapWidth
             || m_CurrentNodeSurroundingNodeY >= m_MapHeight
             || m_CurrentNodeSurroundingNodeX < 0
             || m_CurrentNodeSurroundingNodeY < 0)
            {
                return null;
            }

            Node validNode = m_GridNodes.GetGridNode(m_CurrentNodeSurroundingNodeX, m_CurrentNodeSurroundingNodeY);
            // 再判断 Node是否有效
            if (validNode.IsNPCObstacle || m_CloseNodeList.Contains(validNode))
            {
                return null;
            }

            return validNode;
        }

        /// <summary>
        /// 返回任意两个节点的距离值
        /// </summary>
        /// <param name="nodeA">节点A</param>
        /// <param name="nodeB">节点B</param>
        /// <returns>返回 14 的倍数 + 10 的倍数</returns>
        private int GetDistance(Node nodeA, Node nodeB)
        {
            int xDistance = Mathf.Abs(nodeA.GridPosition.x - nodeB.GridPosition.x);
            int yDistance = Mathf.Abs(nodeA.GridPosition.y - nodeB.GridPosition.y);

            if (xDistance > yDistance)
            {
                return 14 * yDistance + 10 * (xDistance - yDistance);
            }

            return 14 * xDistance + 10 * (yDistance - xDistance);
        }

        /// <summary>
        /// 更新 NPC 路径每一步的坐标和场景名字
        /// </summary>
        /// <param name="sceneName">场景名字</param>
        /// <param name="npcMovementSteps">NPC 的 MovementStep 堆栈</param>
        private void UpdatePathOnMovementStepStack(string sceneName, Stack<MovementStep> npcMovementSteps)
        {
            // 由于 NPC 路径是反向生成的，所以先将 TargetNode 压进堆栈，这样 TargetNode 第一个 Pop
            Node nextNode = m_TargetNode;
            while (nextNode != null)
            {
                MovementStep newMovementStep = new MovementStep
                {
                    SceneName = sceneName
                  , GridCoordinate = new Vector2Int
                    (
                        nextNode.GridPosition.x + m_OriginX
                      , nextNode.GridPosition.y + m_OriginY
                    )
                };
                npcMovementSteps.Push(newMovementStep);
                nextNode = nextNode.ParentNode;
            }
        }
    }
}