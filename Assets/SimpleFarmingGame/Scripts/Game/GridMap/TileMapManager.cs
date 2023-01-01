using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace SimpleFarmingGame.Game
{
    public class TileMapManager : Singleton<TileMapManager>, ISavable
    {
        [Header("地图信息")] public List<MapDataSO> MapDataList;
        [Header("种地浇水瓦片")] [SerializeField] private RuleTile DigTile;
        [SerializeField] private RuleTile WaterTile;
        private Tilemap m_DigTilemap;
        private Tilemap m_WaterTilemap;
        private Season m_CurrentSeason;
        private Grid m_CurrentGrid;
        private const int MaxColliders = 5; // 不知道该 Point 除了 Crop 的 Collider 还有谁的 Collider
        private Collider2D[] m_Collider2Ds = new Collider2D[MaxColliders];

        /// <summary>
        /// key: coordinate + sceneName <br/>
        /// value: TileDetails
        /// </summary>
        private Dictionary<string, TileDetails> m_TileDetailsDict = new();

        private Dictionary<string, bool> m_HasBeenLoadedSceneDict = new(); // 已经被加载过一次的场景
        private List<ReapItem> m_GrassList;

        private const int ReapGrassCount = 2; // 一次性可以割草的数量

        private void OnEnable()
        {
            EventSystem.ExecuteActionAfterAnimation += OnExecuteActionAfterAnimation;
            EventSystem.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
            EventSystem.GameDayChangeEvent += OnGameDayChangeEvent;
            EventSystem.RefreshCurrentSceneMap += RefreshMap;
        }

        private void OnDisable()
        {
            EventSystem.ExecuteActionAfterAnimation -= OnExecuteActionAfterAnimation;
            EventSystem.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
            EventSystem.GameDayChangeEvent -= OnGameDayChangeEvent;
            EventSystem.RefreshCurrentSceneMap -= RefreshMap;
        }

        private void Start()
        {
            ISavable savable = this;
            savable.RegisterSavable();
            foreach (var mapData in MapDataList)
            {
                m_HasBeenLoadedSceneDict.Add(mapData.SceneName, true);
                InitTileDetailsDict(mapData);
            }
        }

        private void OnExecuteActionAfterAnimation(Vector3 mouseWorldPosition, ItemDetails itemDetails)
        {
            Vector3Int mouseGridPosition = m_CurrentGrid.WorldToCell(mouseWorldPosition);
            TileDetails currentTileDetails = GetTileDetails(mouseGridPosition);

            if (currentTileDetails != null)
            {
                Crop currentCrop = GetCropInstance(mouseWorldPosition);
                switch (itemDetails.ItemType)
                {
                    case ItemType.Seed:
                        EventSystem.CallUpdateSceneCropEvent(itemDetails.ItemID, currentTileDetails);
                        EventSystem.CallDropItemEvent
                        (
                            itemDetails.ItemID
                          , mouseWorldPosition
                          , itemDetails.ItemType
                        );
                        EventSystem.CallPlaySoundEvent(SoundName.PlantSeed);
                        break;
                    case ItemType.Commodity:
                        EventSystem.CallDropItemEvent
                        (
                            itemDetails.ItemID
                          , mouseWorldPosition
                          , itemDetails.ItemType
                        );
                        break;
                    case ItemType.HoeTool:
                        SetDigGround(currentTileDetails);
                        currentTileDetails.DaysSinceDug = 0; // -1 -> 0, it means the tile was dug today
                        currentTileDetails.CanDig = false;
                        currentTileDetails.CanDropItem = false;
                        // TODO: Sound Effect
                        EventSystem.CallPlaySoundEvent(SoundName.Hoe);
                        break;
                    case ItemType.WaterTool:
                        SetWaterGround(currentTileDetails);
                        currentTileDetails.DaysSinceWatered = 0;
                        // TODO: Sound Effect
                        EventSystem.CallPlaySoundEvent(SoundName.WateringCan);
                        break;
                    case ItemType.PickAxeTool:
                    case ItemType.AxeTool:
                        if (currentCrop != null)
                        {
                            currentCrop.Harvest(itemDetails, currentCrop.TileDetails);
                        }

                        break;
                    case ItemType.CollectTool:
                        if (currentCrop != null)
                        {
                            // Execute collect function
                            currentCrop.Harvest(itemDetails, currentTileDetails);
                        }

                        break;
                    case ItemType.SickleTool:
                        int reapGrassCount = 0;
                        foreach (ReapItem grass in m_GrassList)
                        {
                            EventSystem.CallParticleEffectEvent
                            (
                                ParticleEffectType.Grass
                              , grass.transform.position + Vector3.up
                            );
                            grass.ProduceCropFruit();
                            Destroy(grass.gameObject);
                            ++reapGrassCount;
                            if (reapGrassCount == ReapGrassCount)
                            {
                                break;
                            }
                        }

                        EventSystem.CallPlaySoundEvent(SoundName.Scythe);

                        break;
                    case ItemType.Furniture:
                        // 在地图上生成家具 ItemManager
                        // 在背包当中移除图纸 InventoryManager
                        // 移除建造家具所需资源物品 InventoryManager
                        EventSystem.CallBuildFurnitureEvent(itemDetails.ItemID, mouseWorldPosition);
                        break;
                    case ItemType.Reapable: break;
                }

                UpdateDataInTileDetailsDict(currentTileDetails);
            }
        }

        private void OnAfterSceneLoadedEvent()
        {
            m_CurrentGrid = FindObjectOfType<Grid>();
            m_DigTilemap = GameObject.FindWithTag("Dig").GetComponent<Tilemap>();
            m_WaterTilemap = GameObject.FindWithTag("Water").GetComponent<Tilemap>();

            if (m_HasBeenLoadedSceneDict[SceneManager.GetActiveScene().name] == true)
            {
                EventSystem.CallPreGeneratedCropsEvent(); // 预先生成农作物
                m_HasBeenLoadedSceneDict[SceneManager.GetActiveScene().name] = false;
            }

            RefreshMap();
        }

        private void OnGameDayChangeEvent(int day, Season season)
        {
            m_CurrentSeason = season;

            foreach (KeyValuePair<string, TileDetails> pair in m_TileDetailsDict)
            {
                TileDetails tileDetails = pair.Value;
                if (tileDetails.DaysSinceWatered > -1)
                {
                    tileDetails.DaysSinceWatered = -1;
                }

                if (tileDetails.DaysSinceDug > -1)
                {
                    tileDetails.DaysSinceDug++;
                }

                // 超期消除挖坑
                if (tileDetails.DaysSinceDug > 5 && tileDetails.SeedItemID == -1)
                {
                    tileDetails.DaysSinceDug = -1;
                    tileDetails.CanDig = true;
                    tileDetails.HaveGrownDays = -1;
                }

                if (tileDetails.SeedItemID != -1)
                {
                    tileDetails.HaveGrownDays++;
                }
            }

            RefreshMap();
        }

        private void InitTileDetailsDict(MapDataSO mapData)
        {
            foreach (TileProperty tileProperty in mapData.TilePropertyList)
            {
                TileDetails tileDetails = new TileDetails
                {
                    GridX = tileProperty.TileCoordinate.x
                  , GridY = tileProperty.TileCoordinate.y
                };

                string key = tileDetails.GridX + "x" + tileDetails.GridY + "y" + mapData.SceneName;

                if (GetTileDetailsFromDictionary(key) != null)
                {
                    tileDetails = GetTileDetailsFromDictionary(key);
                }

                MatchesBoolValueByTileType(tileProperty, tileDetails);

                if (GetTileDetailsFromDictionary(key) != null)
                {
                    m_TileDetailsDict[key] = tileDetails;
                }
                else
                {
                    m_TileDetailsDict.Add(key, tileDetails);
                }
            }
        }

        /// <summary>
        /// 根据字典的key值（坐标+场景名）返回瓦片详情。
        /// </summary>
        /// <param name="key">字典的Key值</param>
        /// <returns>瓦片详情（TileDetails）。</returns>
        public TileDetails GetTileDetailsFromDictionary(string key) =>
            m_TileDetailsDict.ContainsKey(key) ? m_TileDetailsDict[key] : null;

        /// <summary>
        /// 获取瓦片信息
        /// </summary>
        /// <param name="mouseGridPosition">鼠标网格坐标</param>
        /// <returns>返回鼠标网格位置处的瓦片信息</returns>
        /// <remarks>必须使用鼠标的网格坐标</remarks>
        public TileDetails GetTileDetails(Vector3Int mouseGridPosition)
        {
            string key = mouseGridPosition.x + "x" + mouseGridPosition.y + "y" + SceneManager.GetActiveScene().name;

            return GetTileDetailsFromDictionary(key);
        }

        /// <summary>
        /// 刷新地图，先清空地图信息再从瓦片详情字典中加载回来
        /// </summary>
        private void RefreshMap()
        {
            if (m_DigTilemap != null) m_DigTilemap.ClearAllTiles();

            if (m_WaterTilemap != null) m_WaterTilemap.ClearAllTiles();

            Crop[] crops = FindObjectsOfType<Crop>();
            foreach (Crop crop in crops) Destroy(crop.gameObject);

            ReloadSceneMapInfo(SceneManager.GetActiveScene().name);
        }

        private void SetDigGround(TileDetails tileDetails)
        {
            Vector3Int tileCoordinate = new Vector3Int(tileDetails.GridX, tileDetails.GridY, 0);
            if (m_DigTilemap != null)
            {
                m_DigTilemap.SetTile(position: tileCoordinate, tile: DigTile);
            }
        }

        private void SetWaterGround(TileDetails tileDetails)
        {
            Vector3Int tileCoordinate = new Vector3Int(tileDetails.GridX, tileDetails.GridY, 0);
            if (m_WaterTilemap != null)
            {
                m_WaterTilemap.SetTile(position: tileCoordinate, tile: WaterTile);
            }
        }

        /// <summary>
        /// 更新瓦片详情字典中的数据。<br/>
        /// Update the data in tile details dictionary
        /// </summary>
        /// <param name="tileDetails">瓦片详情</param>
        public void UpdateDataInTileDetailsDict(TileDetails tileDetails)
        {
            string key = tileDetails.GridX + "x" + tileDetails.GridY + "y" + SceneManager.GetActiveScene().name;
            if (m_TileDetailsDict.ContainsKey(key))
            {
                m_TileDetailsDict[key] = tileDetails;
            }
            else
            {
                m_TileDetailsDict.Add(key, tileDetails);
            }
        }

        /// <summary>
        /// 从瓦片详情字典中加载场景地图信息（挖坑、浇水、种子、等信息）<br/>
        /// Load scene map info from tileDetailsDict.
        /// </summary>
        /// <param name="sceneName">需要加载信息的场景名</param>
        private void ReloadSceneMapInfo(string sceneName)
        {
            foreach (var (key, tileDetails) in m_TileDetailsDict)
            {
                if (key.Contains(sceneName))
                {
                    if (tileDetails.DaysSinceDug > -1) SetDigGround(tileDetails);

                    if (tileDetails.DaysSinceWatered > -1) SetWaterGround(tileDetails);

                    if (tileDetails.SeedItemID > -1)
                        EventSystem.CallUpdateSceneCropEvent(tileDetails.SeedItemID, tileDetails);
                }
            }
        }

        public Crop GetCropInstance(Vector3 mouseWorldPosition)
        {
            Crop currentCrop = null;
            int collidersSize = Physics2D.OverlapPointNonAlloc(mouseWorldPosition, m_Collider2Ds);
            // FIXME: Collider2D[] -> Collider of Bounds、Collider of Crop，后续可能还会有，最后检测完修改MaxColliders
            // for (int i = 0; i < collidersSize; ++i) Debug.Log(m_Collider2Ds[i].name);

            for (int i = 0; i < collidersSize; ++i)
            {
                if (m_Collider2Ds[i].GetComponent<Crop>())
                {
                    currentCrop = m_Collider2Ds[i].GetComponent<Crop>();
                }
            }

            return currentCrop;
        }

        /// <summary>
        /// 检测 Player 周围是否有可收割的杂草
        /// </summary>
        /// <param name="mouseWorldPosition">鼠标的世界坐标</param>
        /// <param name="pickAxe">镰刀</param>
        /// <returns></returns>
        /// <remarks>ItemDetails 请传入镰刀的 ItemDetails</remarks>
        public bool HaveReapItemInAround(Vector3 mouseWorldPosition, ItemDetails pickAxe)
        {
            m_GrassList = new List<ReapItem>();

            Collider2D[] collider2Ds = new Collider2D[20]; // FIXME: 后续考虑是否需要改小数组的大小

            Physics2D.OverlapCircleNonAlloc(mouseWorldPosition, pickAxe.ItemUseRadius, collider2Ds);

            int length = collider2Ds.Length;
            if (length > 0)
            {
                for (int i = 0; i < length; ++i)
                {
                    if (collider2Ds[i] != null)
                    {
                        if (collider2Ds[i].TryGetComponent(out ReapItem reapItem))
                        {
                            m_GrassList.Add(reapItem);
                        }
                    }
                }
            }

            return m_GrassList.Count > 0;
        }

        /// <summary>
        /// Matches each "BoolTypeValue" of "TileProperty" and "TileDetails" based on "TileType". <br/>
        /// 根据 "瓦片类型" 匹配 "瓦片属性" 和 "瓦片详情" 的 每一个 "布尔值"。
        /// </summary>
        /// <param name="tileProperty">瓦片属性</param>
        /// <param name="tileDetails">瓦片详情</param>
        private static void MatchesBoolValueByTileType(TileProperty tileProperty, TileDetails tileDetails)
        {
            switch (tileProperty.TileType)
            {
                case TileType.Diggable:
                    tileDetails.CanDig = tileProperty.BoolTypeValue;
                    break;
                case TileType.Droppable:
                    tileDetails.CanDropItem = tileProperty.BoolTypeValue;
                    break;
                case TileType.PlaceFurniture:
                    tileDetails.CanPlaceFurniture = tileProperty.BoolTypeValue;
                    break;
                case TileType.NPCObstacle:
                    tileDetails.IsNPCObstacle = tileProperty.BoolTypeValue;
                    break;
                default:
                    Debug.Log("未给TileMap设置TileType");
                    break;
            }
        }

        /// <summary>
        /// 根据传入的场景名字查找地图信息，并输出地图信息（地图尺寸、地图原点）
        /// </summary>
        /// <param name="sceneName">场景名字</param>
        /// <param name="mapDimensions">地图信息：地图尺寸</param>
        /// <param name="mapOrigin">地图信息：地图原点</param>
        /// <returns>是否有当前场景的信息</returns>
        public bool GetMapDimensions(string sceneName, out Vector2Int mapDimensions, out Vector2Int mapOrigin)
        {
            mapDimensions = Vector2Int.zero;
            mapOrigin = Vector2Int.zero;

            foreach (MapDataSO mapData in MapDataList)
            {
                if (mapData.SceneName == sceneName)
                {
                    mapDimensions.x = mapData.MapWidth;
                    mapDimensions.y = mapData.MapHeight;
                    mapOrigin.x = mapData.OriginX;
                    mapOrigin.y = mapData.OriginY;

                    return true;
                }
            }

            return false;
        }

        public string GUID => GetComponent<DataGUID>().GUID;

        public GameSaveData GenerateSaveData()
        {
            GameSaveData saveData = new GameSaveData();
            saveData.TileDetailsDict = m_TileDetailsDict;
            saveData.HasBeenLoadedSceneDict = m_HasBeenLoadedSceneDict;
            return saveData;
        }

        public void RestoreData(GameSaveData saveData)
        {
            m_TileDetailsDict = saveData.TileDetailsDict;
            m_HasBeenLoadedSceneDict = saveData.HasBeenLoadedSceneDict;
        }
    }
}