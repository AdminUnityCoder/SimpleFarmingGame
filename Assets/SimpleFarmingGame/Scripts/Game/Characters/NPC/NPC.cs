using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SimpleFarmingGame.Game
{
    public static class GridModel
    {
        [Tooltip("网格单元格大小")] public const float GridCellSize = 1f;
        [Tooltip("网格单元对角线大小")] public const float GridCellDiagonalSize = 1.41f;
        public const float PixelSize = 0.05f;
        public const int MaxGridSize = 9999;
    }

    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    public class NPC : MonoBehaviour, ISavable
    {
        [Header("NPC时间表")] public ScheduleDataListSO ScheduleData;
        private SortedSet<ScheduleDetails> m_ScheduleDetailsSet;
        private ScheduleDetails m_CurrentScheduleDetails;

        [SerializeField] private string m_CurrentScene;
        private string m_TargetScene;

        public string StartScene
        {
            set => m_CurrentScene = value;
        }

        private Grid m_CurrentGrid;
        private Vector3Int m_CurrentGridPosition;
        private Vector3Int m_TargetGridPosition;
        private Vector3Int m_NextGridPosition;
        private Vector3 m_NextGridWorldPosition;

        [Header("NPC Basic Properties")] public float NormalSpeed = 2f;
        private float m_MinSpeed = 1f;
        private float m_MaxSpeed = 2f;
        private Vector2 m_Direction;
        public bool IsMoving; // Animation

        private bool m_NPCMove; // Coroutine
        private bool m_IsInitializedNPC;
        private bool m_IsSceneLoaded;
        public bool CanInteractable;
        private bool m_IsFirstLoaded; // 判断是否是第一次加载这个NPC

        private Stack<MovementStep> m_MovementStepStack;
        private Season m_CurrentSeason;
        private Coroutine m_NPCMoveCoroutine;
        private Rigidbody2D m_Rigidbody2D;
        private SpriteRenderer m_SpriteRenderer;
        private BoxCollider2D m_BoxCollider2D;
        private Animator m_Animator;
        private const float AnimationIntervalTime = 5f;
        private float m_AnimationTimer; // 计时器
        private bool m_CanPlayStopAnimation;
        private AnimationClip m_StopAnimationClip;
        [SerializeField] private AnimationClip BlankAnimationClip;
        private AnimatorOverrideController m_OverrideController;

        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        private static readonly int DirX = Animator.StringToHash("DirX");
        private static readonly int DirY = Animator.StringToHash("DirY");
        private static readonly int Exit = Animator.StringToHash("Exit");
        private static readonly int EventAnimation = Animator.StringToHash("EventAnimation");

        private TimeSpan GameTime => TimeManager.Instance.GameTime;

        private void Awake()
        {
            m_Rigidbody2D = GetComponent<Rigidbody2D>();
            m_SpriteRenderer = GetComponent<SpriteRenderer>();
            m_BoxCollider2D = GetComponent<BoxCollider2D>();
            m_Animator = GetComponent<Animator>();
            m_MovementStepStack = new Stack<MovementStep>();
            m_OverrideController = new AnimatorOverrideController(m_Animator.runtimeAnimatorController);
            m_Animator.runtimeAnimatorController = m_OverrideController;
            m_ScheduleDetailsSet = new SortedSet<ScheduleDetails>();
            foreach (ScheduleDetails scheduleDetails in ScheduleData.ScheduleDetailsList)
            {
                m_ScheduleDetailsSet.Add(scheduleDetails);
            }
        }

        private void OnEnable()
        {
            EventSystem.OnBeforeSceneUnloadedEvent += OnBeforeSceneUnloadedEvent;
            EventSystem.OnAfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
            EventSystem.GameHourMinuteChangeEvent += OnGameHourMinuteChangeEvent;
            EventSystem.OnStartNewGameEvent += OnStartNewGameEvent;
            EventSystem.EndGameEvent += OnEndGameEvent;
        }

        private void OnDisable()
        {
            EventSystem.OnBeforeSceneUnloadedEvent -= OnBeforeSceneUnloadedEvent;
            EventSystem.OnAfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
            EventSystem.GameHourMinuteChangeEvent -= OnGameHourMinuteChangeEvent;
            EventSystem.OnStartNewGameEvent -= OnStartNewGameEvent;
            EventSystem.EndGameEvent += OnEndGameEvent;
        }

        private void Start()
        {
            ISavable savable = this;
            savable.RegisterSavable();
        }

        private void Update()
        {
            if (m_IsSceneLoaded)
            {
                SwitchAnimation();
            }

            m_AnimationTimer -= Time.deltaTime;
            m_CanPlayStopAnimation = m_AnimationTimer <= 0;
        }

        private void FixedUpdate()
        {
            if (m_IsSceneLoaded)
            {
                NPCMovement();
            }
        }

        private void OnBeforeSceneUnloadedEvent()
        {
            m_IsSceneLoaded = false;
        }

        private void OnAfterSceneLoadedEvent()
        {
            SetNPCVisible();
            m_CurrentGrid = FindObjectOfType<Grid>();
            if (m_IsInitializedNPC == false)
            {
                InitializeNPC();
                m_IsInitializedNPC = true;
            }

            m_IsSceneLoaded = true;

            if (m_IsFirstLoaded == false)
            {
                m_CurrentGridPosition = m_CurrentGrid.WorldToCell(transform.position);
                ScheduleDetails newScheduleDetails = new ScheduleDetails
                (
                    0
                  , 0
                  , 0
                  , 0
                  , m_CurrentSeason
                  , m_TargetScene
                  , (Vector2Int)m_TargetGridPosition
                  , m_StopAnimationClip
                  , CanInteractable
                );
                BuildPath(newScheduleDetails);
                m_IsFirstLoaded = true;
            }
        }

        private void OnGameHourMinuteChangeEvent(int minute, int hour, int day, Season season)
        {
            m_CurrentSeason = season;
            int time = hour * 100 + minute;

            ScheduleDetails matchScheduleDetails = null;
            foreach (ScheduleDetails schedule in m_ScheduleDetailsSet)
            {
                if (schedule.Time == time)
                {
                    if (schedule.Day != day && schedule.Day != 0) continue;

                    if (schedule.Season != season) continue;

                    matchScheduleDetails = schedule;
                }
                else if (schedule.Time > time) break;
            }

            if (matchScheduleDetails != null)
            {
                BuildPath(matchScheduleDetails);
            }
        }

        private void OnStartNewGameEvent(int obj)
        {
            m_IsInitializedNPC = false;
            m_IsFirstLoaded = true;
        }

        private void OnEndGameEvent()
        {
            m_IsSceneLoaded = false;
            m_NPCMove = false;
            if (m_NPCMoveCoroutine != null)
            {
                StopCoroutine(m_NPCMoveCoroutine);
            }
        }

        private void InitializeNPC()
        {
            m_TargetScene = m_CurrentScene; // 防止 NPC 移动
            m_CurrentGridPosition = m_CurrentGrid.WorldToCell(transform.position);

            // 将　NPC　移到当前网格的中心点
            transform.position = new Vector3
            (
                m_CurrentGridPosition.x + GridModel.GridCellSize / 2f
              , m_CurrentGridPosition.y + GridModel.GridCellSize / 2f
              , 0
            );

            m_TargetGridPosition = m_CurrentGridPosition; // 防止 NPC 移动
        }

        private void NPCMovement()
        {
            if (m_NPCMove == false)
            {
                if (m_MovementStepStack.Count > 0)
                {
                    MovementStep step = m_MovementStepStack.Pop();
                    m_CurrentScene = step.SceneName;
                    SetNPCVisible();
                    m_NextGridPosition = (Vector3Int)step.GridCoordinate;
                    TimeSpan stepTime = new TimeSpan(step.Hour, step.Minute, step.Second);
                    MoveToNextGridPosition(m_NextGridPosition, stepTime);
                }
                else if (IsMoving == false && m_CanPlayStopAnimation)
                {
                    StartCoroutine(SetStopAnimationCoroutine());
                }
            }
        }

        private void MoveToNextGridPosition(Vector3Int nextGridPosition, TimeSpan stepTime)
        {
            m_NPCMoveCoroutine = StartCoroutine(MoveCoroutine(nextGridPosition, stepTime));
        }

        private IEnumerator MoveCoroutine(Vector3Int nextGridPosition, TimeSpan stepTime)
        {
            m_NPCMove = true;
            m_NextGridWorldPosition = GetGridWorldPosition(nextGridPosition);

            // 还有时间用来移动
            if (stepTime > GameTime)
            {
                // 用来移动的时间差
                float timeToMove = (float)(stepTime.TotalSeconds - GameTime.TotalSeconds);
                // 实际移动距离
                float distance = Vector3.Distance(transform.position, m_NextGridWorldPosition);
                float speed = Mathf.Max(m_MinSpeed, distance / timeToMove / TimeModel.SecondThreshold);
                if (speed <= m_MaxSpeed)
                {
                    while (Vector3.Distance(transform.position, m_NextGridWorldPosition)
                      > GridModel.PixelSize)
                    {
                        m_Direction = (m_NextGridWorldPosition - transform.position).normalized;
                        Vector2 positionOffset = new Vector2
                        (
                            m_Direction.x * speed * Time.fixedDeltaTime
                          , m_Direction.y * speed * Time.fixedDeltaTime
                        ); // FIXME: 缓存起来
                        m_Rigidbody2D.MovePosition(m_Rigidbody2D.position + positionOffset);
                        yield return new WaitForFixedUpdate(); // FIXME: 缓存起来
                    }
                }
            }

            // FIXME: 这里不能加else 否则不会移动
            // 如果时间到了就瞬移过去
            m_Rigidbody2D.position = m_NextGridWorldPosition;
            m_CurrentGridPosition = nextGridPosition;
            m_NextGridPosition = m_CurrentGridPosition;
            m_NPCMove = false;
        }

        /// <summary>
        /// 根据 Schedule 构建路径
        /// </summary>
        /// <param name="scheduleDetails">NPC 时间表</param>
        public void BuildPath(ScheduleDetails scheduleDetails)
        {
            m_MovementStepStack.Clear();
            m_CurrentScheduleDetails = scheduleDetails;
            m_TargetScene = scheduleDetails.TargetSceneName;
            m_TargetGridPosition = (Vector3Int)scheduleDetails.TargetGridPosition;
            m_StopAnimationClip = scheduleDetails.StopAnimationClip;
            CanInteractable = scheduleDetails.CanInteractable;

            // 同场景
            if (scheduleDetails.TargetSceneName == m_CurrentScene)
            {
                AStar.Instance.BuildPath
                (
                    scheduleDetails.TargetSceneName
                  , (Vector2Int)m_CurrentGridPosition
                  , scheduleDetails.TargetGridPosition
                  , m_MovementStepStack
                );
            }
            else if (scheduleDetails.TargetSceneName != m_CurrentScene) // TODO: 跨场景移动
            {
                RouteMap routeMap = NPCManager.Instance.GetRouteMap
                (
                    fromSceneName: m_CurrentScene
                  , gotoSceneName: scheduleDetails.TargetSceneName
                );

                if (routeMap != null)
                {
                    // History: For -> Foreach
                    foreach (Route route in routeMap.RouteList)
                    {
                        Vector2Int fromPos, gotoPos;
                        // Route route = route;
                        if (route.FromGridCell.x >= GridModel.MaxGridSize
                         || route.FromGridCell.y >= GridModel.MaxGridSize)
                        {
                            // 如果 FromGridCell >9999，则将当前位置设置为 fromPos 位置
                            fromPos = (Vector2Int)m_CurrentGridPosition;
                        }
                        else
                        {
                            // 否则将 route 里的 FromGridCell 设置为 fromPos
                            fromPos = route.FromGridCell;
                        }

                        if (route.GotoGridCell.x >= GridModel.MaxGridSize
                         || route.GotoGridCell.y >= GridModel.MaxGridSize)
                        {
                            // 如果 GotoGridCell > 9999，则将时间表中的 TargetGridPosition 设置为 targetPos
                            gotoPos = scheduleDetails.TargetGridPosition;
                        }
                        else
                        {
                            // 否则将 route 里的 GotoGridCell 设置为 targetPos
                            gotoPos = route.GotoGridCell;
                        }

                        AStar.Instance.BuildPath(route.SceneName, fromPos, gotoPos, m_MovementStepStack);
                    }
                }
            }

            if (m_MovementStepStack.Count > 1)
            {
                // 更新NPC每一步对应的时间戳
                UpdateTimeSpanToEachStep();
            }
        }

        /// <summary>
        /// 更新 NPC 每一步对应的时间戳
        /// </summary>
        private void UpdateTimeSpanToEachStep()
        {
            MovementStep previousStep = null;
            TimeSpan currentGameTime = GameTime;
            foreach (MovementStep step in m_MovementStepStack)
            {
                // FIXME: 可能会出现 bug
                if (previousStep == null)
                {
                    previousStep = step;
                }
                // previousStep ??= step;

                step.Hour = currentGameTime.Hours;
                step.Minute = currentGameTime.Minutes;
                step.Second = currentGameTime.Seconds;

                TimeSpan passByGridSpanTime; // 走过一个格子所需要的时间长度

                if (IsMoveInObliqueDirection(step, previousStep))
                {
                    passByGridSpanTime = new TimeSpan
                    (
                        0
                      , 0
                      , (int)(GridModel.GridCellDiagonalSize / NormalSpeed / TimeModel.SecondThreshold)
                    );
                }
                else
                {
                    passByGridSpanTime = new TimeSpan
                    (
                        0
                      , 0
                      , (int)(GridModel.GridCellSize / NormalSpeed / TimeModel.SecondThreshold)
                    );
                }

                currentGameTime = currentGameTime.Add(passByGridSpanTime); // 获取下一步的时间戳
                previousStep = step;                                       // 循环下一步
            }
        }

        /// <summary>
        /// 是否斜方向移动
        /// </summary>
        /// <param name="currentStep"></param>
        /// <param name="previousStep"></param>
        /// <returns></returns>
        private bool IsMoveInObliqueDirection(MovementStep currentStep, MovementStep previousStep)
        {
            return currentStep.GridCoordinate.x != previousStep.GridCoordinate.x
             && currentStep.GridCoordinate.y    != previousStep.GridCoordinate.y;
        }

        /// <summary>
        /// 设置NPC是否可见
        /// </summary>
        private void SetNPCVisible()
        {
            if (m_CurrentScene == SceneManager.GetActiveScene().name)
            {
                SetNPCActiveInScene();
            }
            else
            {
                SetNPCInactiveInScene();
            }
        }

        private void SetNPCActiveInScene()
        {
            m_SpriteRenderer.enabled = true;
            m_BoxCollider2D.enabled = true;
            // FIXME:后期缓存一下transform.GetChild(0).gameObject
            transform.GetChild(0).gameObject.SetActive(true);
        }

        private void SetNPCInactiveInScene()
        {
            m_SpriteRenderer.enabled = false;
            m_BoxCollider2D.enabled = false;
            // FIXME:后期缓存一下transform.GetChild(0).gameObject
            transform.GetChild(0).gameObject.SetActive(false);
        }

        /// <summary>
        /// 通过网格坐标获取网格世界坐标
        /// </summary>
        /// <param name="gridPosition">网格坐标</param>
        /// <returns>网格世界坐标</returns>
        private Vector3 GetGridWorldPosition(Vector3Int gridPosition)
        {
            Vector3 worldPosition = m_CurrentGrid.CellToWorld(gridPosition);
            return new Vector3
            (
                worldPosition.x + GridModel.GridCellSize / 2f
              , worldPosition.y + GridModel.GridCellSize / 2f
              , 0
            );
        }

        private void SwitchAnimation()
        {
            IsMoving = transform.position != GetGridWorldPosition(m_TargetGridPosition);
            m_Animator.SetBool(IsMovingHash, IsMoving);
            if (IsMoving)
            {
                m_Animator.SetBool(Exit, true);
                m_Animator.SetFloat(DirX, m_Direction.x);
                m_Animator.SetFloat(DirY, m_Direction.y);
            }
            else
            {
                m_Animator.SetBool(Exit, false);
            }
        }

        private IEnumerator SetStopAnimationCoroutine()
        {
            // 强制 NPC 面向镜头
            m_Animator.SetFloat(DirX, 0);
            m_Animator.SetFloat(DirY, -1);

            m_AnimationTimer = AnimationIntervalTime;
            if (m_StopAnimationClip != null)
            {
                m_OverrideController[BlankAnimationClip] = m_StopAnimationClip;
                m_Animator.SetBool(EventAnimation, true);
                yield return null;
                m_Animator.SetBool(EventAnimation, false);
            }
            else
            {
                m_OverrideController[m_StopAnimationClip] = BlankAnimationClip;
                m_Animator.SetBool(EventAnimation, false);
            }
        }

        public string GUID => GetComponent<DataGUID>().GUID;

        /// <summary>
        /// 保存 NPC 数据
        /// </summary>
        /// <returns></returns>
        public GameSaveData GenerateSaveData()
        {
            GameSaveData saveData = new GameSaveData();

            saveData.CharactersPosDict = new Dictionary<string, SerializableVector3>();
            saveData.CharactersPosDict.Add("m_TargetGridPosition", new SerializableVector3(m_TargetGridPosition));
            saveData.CharactersPosDict.Add("m_CurrentPosition", new SerializableVector3(transform.position));
            saveData.DataSceneName = m_CurrentScene;
            saveData.TargetScene = m_TargetScene;
            if (m_StopAnimationClip != null)
            {
                saveData.AnimationInstanceID = m_StopAnimationClip.GetInstanceID();
            }

            saveData.CanInteractable = CanInteractable;
            saveData.TimeDict = new Dictionary<string, int>();
            saveData.TimeDict.Add("m_CurrentSeason", (int)m_CurrentSeason);

            return saveData;
        }

        /// <summary>
        /// 恢复 NPC 数据
        /// </summary>
        /// <param name="saveData"></param>
        public void RestoreData(GameSaveData saveData)
        {
            m_IsInitializedNPC = true; // 已经初始化过 NPC 了，如果不设置为 true，会再次初始化一次 NPC
            m_IsFirstLoaded = false;
            m_CurrentScene = saveData.DataSceneName;
            m_TargetScene = saveData.TargetScene;
            Vector3 position = saveData.CharactersPosDict["m_CurrentPosition"].ToVector3();
            Vector3Int targetGridPosition =
                (Vector3Int)saveData.CharactersPosDict["m_TargetGridPosition"].ToVector2Int();
            transform.position = position;
            m_TargetGridPosition = targetGridPosition;
            if (saveData.AnimationInstanceID != 0)
            {
                m_StopAnimationClip = Resources.InstanceIDToObject(saveData.AnimationInstanceID) as AnimationClip;
            }

            CanInteractable = saveData.CanInteractable;
            m_CurrentSeason = (Season)saveData.TimeDict["m_CurrentSeason"];
        }
    }
}