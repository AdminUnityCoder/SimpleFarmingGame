using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace SimpleFarmingGame.Game
{
    // 需要在编辑器模式下运行，即 Application.IsPlaying(this) == false
    [ExecuteInEditMode]
    public class TileMapInformation : MonoBehaviour
    {
        public MapDataSO MapData;
        public TileType TileType;
        private Tilemap m_CurrentTileMap;

        private void OnEnable()
        {
            if (Application.IsPlaying(this)) return;
            m_CurrentTileMap = GetComponent<Tilemap>();
            if (MapData != null)
            {
                MapData.TilePropertyList.Clear();
            }
        }

        private void OnDisable()
        {
            if (Application.IsPlaying(this)) return;
            m_CurrentTileMap = GetComponent<Tilemap>();
            UpdateTileProperties();
#if UNITY_EDITOR
            if (MapData != null)
            {
                EditorUtility.SetDirty(MapData);
            }
#endif
        }

        private void UpdateTileProperties()
        {
            // CompressBounds(): 将 Tilemap 的 origin 和 size 压缩到瓦片所存在的边界。
            // origin: TileMap 的原点（以单元格位置为单位）。这仅考虑 Tilemap 中已放置的瓦片。
            // size: Tilemap 的大小（以单元格为单位）。这仅考虑 Tilemap 中已放置的瓦片。
            m_CurrentTileMap.CompressBounds();

            if (Application.IsPlaying(this)) return;
            if (MapData == null) return;

            // cellBounds: 以单元格大小返回 Tilemap 的边界
            BoundsInt cellBounds = m_CurrentTileMap.cellBounds;
            Vector3Int startPoint = cellBounds.min; // 获得已绘制范围的左下角位置
            Vector3Int endPoint = cellBounds.max;   // 获得已绘制范围的右上角位置

            for (int x = startPoint.x; x < endPoint.x; ++x)
            {
                for (int y = startPoint.y; y < endPoint.y; ++y)
                {
                    // GetTile: 根据给定的瓦片地图中某个单元格的 XYZ 坐标，获取瓦片。
                    TileBase tile = m_CurrentTileMap.GetTile(new Vector3Int(x, y, 0));

                    if (tile == null) continue;

                    TileProperty tileProperty = new TileProperty
                    {
                        TileType = this.TileType
                      , TileCoordinate = new Vector2Int(x, y)
                      , BoolTypeValue = true
                    };

                    MapData.TilePropertyList.Add(tileProperty);
                }
            }
        }
    }
}