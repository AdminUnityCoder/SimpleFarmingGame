using UnityEngine;
using UnityEngine.UI;

namespace SimpleFarmingGame.Game
{
    public class CursorManager : MonoBehaviour
    {
        [Header("Cursor")] [SerializeField] private Sprite Normal;
        [SerializeField] private Sprite Tool;
        [SerializeField] private Sprite Seed;
        [SerializeField] private Sprite Commodity;
        private Sprite m_CurrentCursorSprite;
        private RectTransform m_CursorCanvasTransform;
        private Image m_CursorImage;
        private bool m_CursorEnable;
        private bool m_CursorPositionAvailable; // Check whether the current mouse position is available
        private Color m_OpaqueColor = new(1f, 1f, 1f, 1f);
        private Color m_RedTranslucentColor = new(1f, 0f, 0f, 0.5f);

        [Header("Build")] private Image m_BuildImage;

        private Camera m_MainCamera; // ScreenCoordinate -> WorldCoordinate -> GridCoordinate
        private Grid m_CurrentGrid;
        private Vector3 m_MouseWorldPosition;   // Execute by mainCamera 
        private Vector3Int m_MouseGridPosition; // Execute by currentGrid

        private ItemDetails m_CurrentItemDetails;

        private void OnEnable()
        {
            EventSystem.ItemSelectedEvent += OnItemSelectedEvent;
            EventSystem.BeforeSceneUnloadedEvent += OnBeforeSceneUnloadedEvent;
            EventSystem.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
        }

        private void OnDisable()
        {
            EventSystem.ItemSelectedEvent -= OnItemSelectedEvent;
            EventSystem.BeforeSceneUnloadedEvent -= OnBeforeSceneUnloadedEvent;
            EventSystem.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
        }

        private void Start()
        {
            m_CursorCanvasTransform = GameObject.FindGameObjectWithTag("CursorCanvas").GetComponent<RectTransform>();
            m_CursorImage = m_CursorCanvasTransform.GetChild(0).GetComponent<Image>();
            m_BuildImage = m_CursorCanvasTransform.GetChild(1).GetComponent<Image>();
            m_BuildImage.gameObject.SetActive(false);

            m_CurrentCursorSprite = Normal;
            SetCursorImage(Normal);

            m_MainCamera = Camera.main;
        }

        private void Update()
        {
            // if(ReferenceEquals(m_CursorCanvasTransform,null) == true) return; 可以改但是没必要
            if (m_CursorCanvasTransform == null) return; // Comparison to 'null' is expensive
            m_CursorImage.transform.position = Input.mousePosition;
            if (InteractWithUI() == false && m_CursorEnable)
            {
                SetCursorImage(m_CurrentCursorSprite);
                CheckCursorAvailable(); // 如果跨场景之前不禁用掉这行代码会报错，因为该方法里的CurrentGrid在跨场景时会丢失
                CheckPlayerInput();
            }
            else
            {
                SetCursorImage(Normal);
                m_BuildImage.gameObject.SetActive(false);
            }
        }

        private void CheckPlayerInput()
        {
            if (Input.GetMouseButtonDown(0) && m_CursorPositionAvailable)
            {
                EventSystem.CallMouseClickedEvent(m_MouseWorldPosition, m_CurrentItemDetails);
            }
        }

        private void SetCursorImage(Sprite sprite)
        {
            m_CursorImage.sprite = sprite;
            m_CursorImage.color = m_OpaqueColor;
        }

        private void SetCursorAvailable()
        {
            m_CursorPositionAvailable = true;
            m_CursorImage.color = m_OpaqueColor;
            m_BuildImage.color = m_OpaqueColor;
        }

        private void SetCursorUnavailable()
        {
            m_CursorPositionAvailable = false;
            m_CursorImage.color = m_RedTranslucentColor;
            m_BuildImage.color = m_RedTranslucentColor;
        }

        private void OnItemSelectedEvent(ItemDetails itemDetails, bool isSelected)
        {
            if (isSelected == false && m_CursorEnable)
            {
                m_CurrentItemDetails = null;
                m_CursorEnable = false; // If cursorEnable == false, Can not check player input
                m_CurrentCursorSprite = Normal;
                m_BuildImage.gameObject.SetActive(false);
            }
            else
            {
                m_CurrentItemDetails = itemDetails;
                MatchCurrentCursorSprite(itemDetails);
                m_CursorEnable = true;

                // 显示建造物品图片
                if (itemDetails.ItemType == ItemType.Furniture)
                {
                    m_BuildImage.gameObject.SetActive(true);
                    m_BuildImage.sprite = itemDetails.ItemIconOnWorld;
                    m_BuildImage.SetNativeSize();
                }
            }
        }

        private void OnBeforeSceneUnloadedEvent()
        {
            m_CursorEnable = false;
        }

        private void OnAfterSceneLoadedEvent()
        {
            m_CurrentGrid = FindObjectOfType<Grid>();
        }

        private void CheckCursorAvailable()
        {
            // m_MouseWorldPosition = m_MainCamera.ScreenToWorldPoint(Input.mousePosition);
            m_MouseWorldPosition = m_MainCamera.ScreenToWorldPoint
            (
                new Vector3
                (
                    Input.mousePosition.x
                  , Input.mousePosition.y
                  , -m_MainCamera.transform.position.z
                )
            );

            m_MouseGridPosition = m_CurrentGrid.WorldToCell(m_MouseWorldPosition);

            Vector3Int playerGridPosition = m_CurrentGrid.WorldToCell(Player.Instance.Position);
            m_BuildImage.rectTransform.position = Input.mousePosition;

            if (Mathf.Abs(m_MouseGridPosition.x - playerGridPosition.x) > m_CurrentItemDetails.ItemUseRadius
             || Mathf.Abs(m_MouseGridPosition.y - playerGridPosition.y) > m_CurrentItemDetails.ItemUseRadius)
            {
                SetCursorUnavailable();
                // The following code is not executed when the item is out of use radius.
                return;
            }

            TileDetails currentTileDetails = TileMapManager.Instance.GetTileDetails(m_MouseGridPosition);

            if (currentTileDetails != null)
            {
                // Set whether the cursor is available by item type and tile details
                SetCursorAvailableByItemTypeAndTileDetails(m_CurrentItemDetails.ItemType, currentTileDetails);
            }
            else
            {
                SetCursorUnavailable();
            }
        }

        /// <summary>
        /// 根据传入的物品详情的物品类型匹配鼠标图标
        /// </summary>
        /// <param name="itemDetails">物品详情</param>
        private void MatchCurrentCursorSprite(ItemDetails itemDetails)
        {
            m_CurrentCursorSprite = itemDetails.ItemType switch
            {
                ItemType.Seed => Seed
              , ItemType.Commodity => Commodity
              , ItemType.AxeTool => Tool
              , ItemType.CollectTool => Tool
              , ItemType.HoeTool => Tool
              , ItemType.SickleTool => Tool
              , ItemType.WaterTool => Tool
              , ItemType.PickAxeTool => Tool
              , ItemType.Furniture => Tool
              , ItemType.Reapable => Tool
              , _ => Normal
            };
        }

        /// <summary>
        /// 通过传入的物品类型以及瓦片详情设置鼠标是否可用
        /// </summary>
        /// <param name="itemType">ItemDetails.ItemType</param>
        /// <param name="tileDetails">currentTileDetails</param>
        private void SetCursorAvailableByItemTypeAndTileDetails(ItemType itemType, TileDetails tileDetails)
        {
            CropDetails currentCropDetails = CropManager.Instance.GetCropDetails(tileDetails.SeedItemID);
            Crop crop = TileMapManager.Instance.GetCropInstance(m_MouseWorldPosition);
            switch (itemType)
            {
                case ItemType.Seed:
                    if (tileDetails.DaysSinceDug > -1 && tileDetails.SeedItemID == -1)
                    {
                        SetCursorAvailable();
                    }
                    else
                    {
                        SetCursorUnavailable();
                    }

                    break;
                case ItemType.Commodity:
                    if (tileDetails.CanDropItem && m_CurrentItemDetails.CanDropped)
                    {
                        SetCursorAvailable();
                    }
                    else
                    {
                        SetCursorUnavailable();
                    }

                    break;
                case ItemType.HoeTool:
                    if (tileDetails.CanDig)
                    {
                        SetCursorAvailable();
                    }
                    else
                    {
                        SetCursorUnavailable();
                    }

                    break;
                case ItemType.WaterTool:
                    // 已经挖了坑但是没有浇水才可以浇水
                    if (tileDetails.DaysSinceDug > -1 && tileDetails.DaysSinceWatered == -1)
                    {
                        SetCursorAvailable();
                    }
                    else
                    {
                        SetCursorUnavailable();
                    }

                    break;
                case ItemType.PickAxeTool:
                case ItemType.AxeTool:
                    if (crop != null)
                    {
                        if (crop.CanHarvest && crop.CropDetails.CheckToolIsAvailable(m_CurrentItemDetails.ItemID))
                        {
                            SetCursorAvailable();
                        }
                        else
                        {
                            SetCursorUnavailable();
                        }
                    }
                    else
                    {
                        SetCursorUnavailable();
                    }

                    break;
                case ItemType.CollectTool:
                    if (currentCropDetails != null)
                    {
                        if (currentCropDetails.CheckToolIsAvailable(m_CurrentItemDetails.ItemID))
                        {
                            if (tileDetails.HaveGrownDays >= currentCropDetails.TotalGrowthDays)
                            {
                                SetCursorAvailable();
                            }
                            else
                            {
                                SetCursorUnavailable();
                            }
                        }
                    }
                    else // currentCropDetails == null, can not collect
                    {
                        SetCursorUnavailable();
                    }

                    break;
                case ItemType.SickleTool:
                    if (TileMapManager.Instance.HaveReapItemInAround(m_MouseWorldPosition, m_CurrentItemDetails))
                    {
                        SetCursorAvailable();
                    }
                    else
                    {
                        SetCursorUnavailable();
                    }

                    break;
                case ItemType.Furniture:
                    m_BuildImage.gameObject.SetActive(true);
                    BluePrintDetails bluePrintDetails =
                        InventoryManager.Instance.BluePrintData.GetBluePrintDetails(m_CurrentItemDetails.ItemID);
                    if (tileDetails.CanPlaceFurniture
                     && InventoryManager.Instance.CheckStorage(m_CurrentItemDetails.ItemID)
                     && !CheckForFurnitureNearby(bluePrintDetails))
                    {
                        SetCursorAvailable();
                    }
                    else
                    {
                        SetCursorUnavailable();
                    }

                    break;
                case ItemType.Reapable: break;
            }
        }

        private bool CheckForFurnitureNearby(BluePrintDetails bluePrintDetails)
        {
            GameObject buildItem = bluePrintDetails.BuildItemPrefab;
            Vector2 point = m_MouseWorldPosition;
            Vector2 size = buildItem.GetComponent<BoxCollider2D>().size;

            var othersColl = Physics2D.OverlapBox(point, size, 0);
            return othersColl != null ? othersColl.GetComponent<Furniture>() : false;
        }

        private static bool InteractWithUI()
        {
            return UnityEngine.EventSystems.EventSystem.current != null
             && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
            // IsPointerOverGameObject() 判断鼠标是否点击在UI上
        }
    }
}