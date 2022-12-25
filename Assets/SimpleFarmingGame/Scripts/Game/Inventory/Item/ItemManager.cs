using System;
using System.Collections.Generic;
using SimpleFarmingGame.Game;
using SFG.Save;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SFG.InventorySystem
{
    public static partial class EventSystem
    {
        public static event Action<int, Vector3> InstantiateItemInScene;

        public static void CallInstantiateItemInScene(int itemID, Vector3 position)
        {
            InstantiateItemInScene?.Invoke(itemID, position);
        }

        public static event Action<int, Vector3, ItemType> DropItemEvent;

        public static void CallDropItemEvent(int itemID, Vector3 position, ItemType itemType)
        {
            DropItemEvent?.Invoke(itemID, position, itemType);
        }
    }

    [Serializable]
    public class SerializableVector3
    {
        public float X, Y, Z;

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

    [Serializable]
    public class SceneItem
    {
        public int ItemID;
        public SerializableVector3 Coordinate;
    }

    public class ItemManager : MonoBehaviour, ISavable
    {
        public Item ItemBasePrefab;
        public Item BounceItemPrefab;
        private Transform m_ItemParent;

        private Dictionary<string, List<SceneItem>> m_SceneItemDict = new();
        private Dictionary<string, List<SceneFurniture>> m_SceneFurnitureDict = new();

        private void OnEnable()
        {
            EventSystem.InstantiateItemInScene += OnInstantiateItemInScene;
            EventSystem.DropItemEvent += OnDropItemEvent;
            TransitionSystem.EventSystem.BeforeSceneUnloadedEvent += OnBeforeSceneUnloadedEvent;
            TransitionSystem.EventSystem.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
            MapSystem.EventSystem.BuildFurnitureEvent += OnBuildFurnitureEvent;
            UI.EventSystem.StartNewGameEvent += OnStartNewGameEvent; // UI
        }

        private void OnDisable()
        {
            EventSystem.InstantiateItemInScene -= OnInstantiateItemInScene;
            EventSystem.DropItemEvent -= OnDropItemEvent;
            TransitionSystem.EventSystem.BeforeSceneUnloadedEvent -= OnBeforeSceneUnloadedEvent;
            TransitionSystem.EventSystem.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
            MapSystem.EventSystem.BuildFurnitureEvent -= OnBuildFurnitureEvent;
            UI.EventSystem.StartNewGameEvent -= OnStartNewGameEvent; // UI
        }
        
        private void Start()
        {
            ISavable savable = this;
            savable.RegisterSavable();
        }

        private void OnInstantiateItemInScene(int itemID, Vector3 position)
        {
            // FIXME: 此处可以使用工厂模式减少耦合
            Item item = Instantiate(BounceItemPrefab, position, Quaternion.identity, m_ItemParent);
            item.ItemID = itemID;
            item.GetComponent<ItemBounce>().InitBounceItem(position, Vector2.up);
        }

        private void OnDropItemEvent(int itemID, Vector3 mousePosition, ItemType itemType)
        {
            if (itemType == ItemType.Seed) return;
            Vector3 PlayerPosition = Player.Instance.Position;
            Item item = Instantiate(BounceItemPrefab, PlayerPosition, Quaternion.identity, m_ItemParent);
            item.ItemID = itemID;
            Vector3 direction = (mousePosition - PlayerPosition).normalized;
            item.GetComponent<ItemBounce>().InitBounceItem(mousePosition, direction);
        }

        private void OnBeforeSceneUnloadedEvent()
        {
            GetAllItemsInScene();
            GetAllFurnitureInScene();
        }

        private void OnAfterSceneLoadedEvent()
        {
            m_ItemParent = GameObject.FindGameObjectWithTag("ItemParent").transform;
            RegenerateAllItemsInScene();
            RebuildFurnitureIsScene();
        }

        // 点击鼠标在地图上生成家具
        private void OnBuildFurnitureEvent(int buildingPaperID, Vector3 mouseWorldPosition)
        {
            BluePrintDetails bluePrintDetails
                = InventoryManager.Instance.BluePrintData.GetBluePrintDetails(buildingPaperID);
            GameObject furniture =
                Instantiate(bluePrintDetails.BuildItemPrefab, mouseWorldPosition, Quaternion.identity, m_ItemParent);
            if (furniture.TryGetComponent(out Box box))
            {
                box.Index = InventoryManager.Instance.BoxDataCount;
                box.InitializeBox(box.Index);
            }
        }

        private void OnStartNewGameEvent(int obj)
        {
            m_SceneItemDict.Clear();
            m_SceneFurnitureDict.Clear();
        }

        private void GetAllItemsInScene()
        {
            #region Foreach all items in the current scene and add them to the "currentSceneItemList"

            List<SceneItem> currentSceneItemList = new List<SceneItem>();

            Item[] type = FindObjectsOfType<Item>();
            for (var i = 0; i < type.Length; ++i)
            {
                Item item = type[i];
                SceneItem sceneItem = new SceneItem
                {
                    ItemID = item.ItemID
                  , Coordinate = new SerializableVector3(item.transform.position)
                };

                currentSceneItemList.Add(sceneItem);
            }

            #endregion

            #region Update or add "currentSceneItemList" to the "sceneItemDict"

            if (m_SceneItemDict.ContainsKey(SceneManager.GetActiveScene().name))
            {
                // Update the "SceneItemList" in the current active scene
                m_SceneItemDict[SceneManager.GetActiveScene().name] = currentSceneItemList;
            }
            else
            {
                m_SceneItemDict.Add(SceneManager.GetActiveScene().name, currentSceneItemList);
            }

            #endregion
        }

        private void RegenerateAllItemsInScene()
        {
            // List<SceneItem> currentSceneItemList = new List<SceneItem>();
            if (m_SceneItemDict.TryGetValue
            (
                SceneManager.GetActiveScene().name
              , out List<SceneItem> currentSceneItemList
            ))
            {
                if (currentSceneItemList != null)
                {
                    DestroyAllItemsInScene();

                    GenerateItemsInTheList(currentSceneItemList);
                }
            }
        }

        private static void DestroyAllItemsInScene()
        {
            Item[] items = FindObjectsOfType<Item>();
            for (int i = 0; i < items.Length; ++i)
            {
                Destroy(items[i].gameObject);
            }
        }

        private void GenerateItemsInTheList(IReadOnlyList<SceneItem> currentSceneItemList)
        {
            for (int i = 0; i < currentSceneItemList.Count; ++i)
            {
                SceneItem sceneItem = currentSceneItemList[i];

                Item newItem = Instantiate
                (
                    ItemBasePrefab
                  , sceneItem.Coordinate.ToVector3()
                  , Quaternion.identity
                  , m_ItemParent
                );

                newItem.Init(sceneItem.ItemID);
            }
        }

        private void GetAllFurnitureInScene()
        {
            #region Foreach all furnitures in the current scene and add them to the "currentSceneFurnitureList"

            List<SceneFurniture> currentSceneFurnitureList = new List<SceneFurniture>();
            foreach (Furniture furniture in FindObjectsOfType<Furniture>())
            {
                SceneFurniture sceneFurniture = new SceneFurniture
                {
                    FurnitureID = furniture.ID
                  , Coordinate = new SerializableVector3(furniture.transform.position)
                };

                if (furniture.TryGetComponent(out Box box))
                {
                    sceneFurniture.BoxIndex = box.Index;
                }

                currentSceneFurnitureList.Add(sceneFurniture);
            }

            #endregion

            #region Update or add "currentSceneFurnitureList" to the "sceneFurnitureDict"

            if (m_SceneFurnitureDict.ContainsKey(SceneManager.GetActiveScene().name))
            {
                // Update the "SceneFurnitureList" in the current active scene
                m_SceneFurnitureDict[SceneManager.GetActiveScene().name] = currentSceneFurnitureList;
            }
            else
            {
                m_SceneFurnitureDict.Add(SceneManager.GetActiveScene().name, currentSceneFurnitureList);
            }

            #endregion
        }

        private void RebuildFurnitureIsScene()
        {
            List<SceneFurniture> currentSceneFurnitureList = new List<SceneFurniture>();
            if (m_SceneFurnitureDict.TryGetValue(SceneManager.GetActiveScene().name, out currentSceneFurnitureList))
            {
                if (currentSceneFurnitureList != null)
                {
                    foreach (SceneFurniture sceneFurniture in currentSceneFurnitureList)
                    {
                        BluePrintDetails bluePrintDetails
                            = InventoryManager.Instance.BluePrintData.GetBluePrintDetails(sceneFurniture.FurnitureID);
                        GameObject furniture =
                            Instantiate
                            (
                                bluePrintDetails.BuildItemPrefab
                              , sceneFurniture.Coordinate.ToVector3()
                              , Quaternion.identity
                              , m_ItemParent
                            );
                        if (furniture.TryGetComponent(out Box box))
                        {
                            box.InitializeBox(sceneFurniture.BoxIndex);
                        }
                    }
                }
            }
        }

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

            RegenerateAllItemsInScene();
            RebuildFurnitureIsScene();
        }
    }
}