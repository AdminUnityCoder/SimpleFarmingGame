using System.Collections.Generic;
using MyFramework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SimpleFarmingGame.Game
{
    public class ItemManager : MonoBehaviour, ISavable
    {
        [SerializeField] private Item ItemBasePrefab;
        [SerializeField] private Item BounceItemPrefab;
        private Transform m_ItemParent;

        /// <summary>
        /// 场景家具字典<br/>key：场景名字，value：场景家具列表
        /// </summary>
        private Dictionary<string, List<SceneFurniture>> m_SceneFurnitureDict = new();

        /// <summary>
        /// 场景物品字典<br/>key：场景名字，value：场景家具列表
        /// </summary>
        private Dictionary<string, List<SceneItem>> m_SceneItemDict = new();

        private void Start()
        {
            ISavable savable = this;
            savable.RegisterSavable();
        }

        private void OnEnable()
        {
            EventSystem.OnInstantiateItemInScene += InstantiateItem;
            EventSystem.OnDropItemEvent += DropItem;
            EventSystem.OnBeforeSceneUnloadedEvent += GetAllObjectsInScene;
            EventSystem.OnAfterSceneLoadedEvent += RegenerateObjectsInScene;
            EventSystem.OnBuildFurnitureEvent += BuildFurniture;
            EventSystem.OnStartNewGameEvent += ClearAllObjectsDictionary;
        }

        private void OnDisable()
        {
            EventSystem.OnInstantiateItemInScene -= InstantiateItem;
            EventSystem.OnDropItemEvent -= DropItem;
            EventSystem.OnBeforeSceneUnloadedEvent -= GetAllObjectsInScene;
            EventSystem.OnAfterSceneLoadedEvent -= RegenerateObjectsInScene;
            EventSystem.OnBuildFurnitureEvent -= BuildFurniture;
            EventSystem.OnStartNewGameEvent -= ClearAllObjectsDictionary;
        }

        #region Event

        /// <summary>
        /// 生成物品
        /// </summary>
        /// <param name="itemID">物品ID</param>
        /// <param name="position">物品位置</param>
        private void InstantiateItem(int itemID, Vector3 position)
        {
            Item item = Instantiate(BounceItemPrefab, position, Quaternion.identity, m_ItemParent);
            item.ItemID = itemID;
            if (item.TryGetComponent<ItemBounce>(out var itemBounce))
            {
                itemBounce.InitBounceItem(position, Vector2.up);
            }
        }

        /// <summary>
        /// 丢弃物品 —> 视觉上
        /// </summary>
        /// <param name="itemID">丢弃物品的ID</param>
        /// <param name="mouseWorldPos">鼠标世界坐标</param>
        /// <param name="itemType">物品类型</param>
        private void DropItem(int itemID, Vector3 mouseWorldPos, ItemType itemType)
        {
            if (itemType == ItemType.Seed) return;
            var playerPosition = Player.Instance.Position;
            var direction = (mouseWorldPos - playerPosition).normalized;
            var item = Instantiate(BounceItemPrefab, playerPosition, Quaternion.identity, m_ItemParent);
            item.ItemID = itemID;
            // TODO: 待添加功能 -> 扔东西同时改变玩家朝向
            if (item.TryGetComponent<ItemBounce>(out var itemBounce))
            {
                itemBounce.InitBounceItem(mouseWorldPos, direction);
            }
        }

        /// <summary>
        /// 获取场景中所有对象
        /// </summary>
        private void GetAllObjectsInScene()
        {
            GetAllItemsInScene();
            GetAllFurnitureInScene();
        }

        /// <summary>
        /// 重新生成场景中所有对象
        /// </summary>
        private void RegenerateObjectsInScene()
        {
            m_ItemParent = GameObject.FindGameObjectWithTag("ItemParent").transform;
            RegenerateItemsInScene();
            RegenerateFurnitureIsScene();
        }

        /// <summary>
        /// 点击鼠标在地图上生成家具
        /// </summary>
        /// <param name="buildingPaperID">家具建造图纸ID</param>
        /// <param name="mouseWorldPos">鼠标世界坐标</param>
        private void BuildFurniture(int buildingPaperID, Vector3 mouseWorldPos)
        {
            var bluePrintDetails = InventoryManager.Instance.GetBluePrintDetails(buildingPaperID);
            GameObject furniture = Instantiate
            (
                original: bluePrintDetails.BuildItemPrefab
              , position: mouseWorldPos
              , rotation: Quaternion.identity
              , parent: m_ItemParent
            );
            if (!furniture.TryGetComponent(out Box box)) return;
            box.BoxIndex = InventoryManager.Instance.BoxDataCount;
            box.InitializeBox(box.BoxIndex);
        }

        /// <summary>
        /// 清空 m_SceneItemDict 和 m_SceneFurnitureDict
        /// </summary>
        private void ClearAllObjectsDictionary(int obj)
        {
            m_SceneItemDict.Clear();
            m_SceneFurnitureDict.Clear();
        }

        #endregion

        #region Item

        /// <summary>
        /// 获取场景中所有物品
        /// </summary>
        private void GetAllItemsInScene()
        {
            // 创建一个列表存储当前场景中的物品
            List<SceneItem> currentSceneItemList = new List<SceneItem>();
            Item[] items = FindObjectsOfType<Item>();
            foreach (Item item in items)
            {
                SceneItem sceneItem = new SceneItem();
                sceneItem.ItemID = item.ItemID;
                sceneItem.Coordinate = new SerializableVector3(item.GetPosition());

                currentSceneItemList.Add(sceneItem);
            }

            if (!m_SceneItemDict.ContainsKey(SceneManager.GetActiveScene().name))
            {
                m_SceneItemDict.Add(SceneManager.GetActiveScene().name, currentSceneItemList);
            }
            else
            {
                m_SceneItemDict[SceneManager.GetActiveScene().name] = currentSceneItemList;
            }
        }

        /// <summary>
        /// 获取场景中所有家具类型的物体
        /// </summary>
        private void GetAllFurnitureInScene()
        {
            List<SceneFurniture> curSceneFurnitureList = new List<SceneFurniture>();
            Furniture[] furnitures = FindObjectsOfType<Furniture>();
            foreach (Furniture furniture in furnitures)
            {
                SceneFurniture sceneFurniture = new SceneFurniture();
                sceneFurniture.FurnitureID = furniture.ID;
                sceneFurniture.Coordinate = new SerializableVector3(furniture.GetPosition());

                if (furniture.TryGetComponent(out Box box))
                {
                    sceneFurniture.BoxIndex = box.BoxIndex;
                }

                curSceneFurnitureList.Add(sceneFurniture);
            }

            if (!m_SceneFurnitureDict.ContainsKey(SceneManager.GetActiveScene().name))
            {
                m_SceneFurnitureDict.Add(SceneManager.GetActiveScene().name, curSceneFurnitureList);
            }
            else
            {
                m_SceneFurnitureDict[SceneManager.GetActiveScene().name] = curSceneFurnitureList;
            }
        }

        /// <summary>
        /// 重新生成场景中的物体
        /// </summary>
        private void RegenerateItemsInScene()
        {
            if (!m_SceneItemDict.TryGetValue(SceneManager.GetActiveScene().name, out var curSceneItemList)) return;
            if (curSceneItemList == null) return;

            #region 删除场景中的所有物体

            Item[] items = FindObjectsOfType<Item>();

            foreach (Item item in items)
            {
                item.DestroyGameObj();
            }

            #endregion

            #region 重新生成列表中的物体

            foreach (SceneItem sceneItem in curSceneItemList)
            {
                Item newItem = Instantiate
                (
                    original: ItemBasePrefab
                  , position: sceneItem.Coordinate.ToVector3()
                  , rotation: Quaternion.identity
                  , parent: m_ItemParent
                );

                newItem.Init(sceneItem.ItemID);
            }

            #endregion
        }

        /// <summary>
        /// 重新生成场景中的家具
        /// </summary>
        private void RegenerateFurnitureIsScene()
        {
            if (!m_SceneFurnitureDict.TryGetValue(SceneManager.GetActiveScene().name, out var curSceneFurnitureList))
                return;
            if (curSceneFurnitureList == null) return;

            foreach (SceneFurniture sceneFurniture in curSceneFurnitureList)
            {
                var bluePrintDetails = InventoryManager.Instance.GetBluePrintDetails(sceneFurniture.FurnitureID);

                GameObject furniture = Instantiate
                (
                    original: bluePrintDetails.BuildItemPrefab
                  , position: sceneFurniture.Coordinate.ToVector3()
                  , rotation: Quaternion.identity
                  , parent: m_ItemParent
                );

                if (furniture.TryGetComponent(out Box box))
                {
                    box.InitializeBox(sceneFurniture.BoxIndex);
                }
            }
        }

        #endregion

        #region Save

        public string GUID => GetComponent<DataGUID>().GUID;

        public GameSaveData GenerateSaveData()
        {
            GetAllItemsInScene();
            GetAllFurnitureInScene();
            GameSaveData saveData = new GameSaveData();
            saveData.SceneItemDict = m_SceneItemDict;
            saveData.SceneFurnitureDict = m_SceneFurnitureDict;
            return saveData;
        }

        public void RestoreData(GameSaveData saveData)
        {
            m_SceneItemDict = saveData.SceneItemDict;
            m_SceneFurnitureDict = saveData.SceneFurnitureDict;

            RegenerateItemsInScene();
            RegenerateFurnitureIsScene();
        }

        #endregion
    }
}