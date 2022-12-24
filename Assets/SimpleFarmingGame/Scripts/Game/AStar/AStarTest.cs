using System.Collections.Generic;
using SFG.Characters.NPC;
using SFG.TimeSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace SFG.AStar
{
    public class AStarTest : MonoBehaviour
    {
        private AStar m_AStar;
        [Header("用于测试")] [SerializeField] private Vector2Int StartPoint;
        [SerializeField] private Vector2Int TargetPoint;
        [SerializeField] private Tilemap DisplayMap;
        [SerializeField] private TileBase DisplayTile;
        [SerializeField] private bool IsDisplayStartPointAndTargetPoint;
        [SerializeField] private bool IsDisplayPath;
        private Stack<MovementStep> m_NPCMovementStepStack;

        [Header("测试移动NPC")] public NPC NPC;
        public bool MoveNPC;
        public string TargetScene;
        public Vector2Int TargetPosition;
        public AnimationClip StopClip;

        private void Awake()
        {
            m_AStar = GetComponent<AStar>();
            m_NPCMovementStepStack = new Stack<MovementStep>();
        }

        private void Update()
        {
            DisplayPathOnGridMap();

            if (MoveNPC)
            {
                MoveNPC = false;
                ScheduleDetails scheduleDetails = new ScheduleDetails(0, 0, 0, 0, Season.春天, TargetScene, TargetPosition
                  , StopClip, true);
                NPC.BuildPath(scheduleDetails);
            }
        }

        /// <summary>
        /// 在网格地图上显示路径
        /// </summary>
        private void DisplayPathOnGridMap()
        {
            if (DisplayMap != null && DisplayTile != null)
            {
                if (IsDisplayStartPointAndTargetPoint)
                {
                    DisplayMap.SetTile((Vector3Int)StartPoint, DisplayTile);
                    DisplayMap.SetTile((Vector3Int)TargetPoint, DisplayTile);
                }
                else
                {
                    DisplayMap.SetTile((Vector3Int)StartPoint, null);
                    DisplayMap.SetTile((Vector3Int)TargetPoint, null);
                }

                if (IsDisplayPath)
                {
                    string sceneName = SceneManager.GetActiveScene().name;
                    m_AStar.BuildPath(sceneName, StartPoint, TargetPoint, m_NPCMovementStepStack);

                    foreach (MovementStep step in m_NPCMovementStepStack)
                    {
                        DisplayMap.SetTile((Vector3Int)step.GridCoordinate, DisplayTile);
                    }
                }
                else
                {
                    if (m_NPCMovementStepStack.Count > 0)
                    {
                        foreach (MovementStep step in m_NPCMovementStepStack)
                        {
                            DisplayMap.SetTile((Vector3Int)step.GridCoordinate, null);
                        }

                        m_NPCMovementStepStack.Clear();
                    }
                }
            }
        }
    }
}