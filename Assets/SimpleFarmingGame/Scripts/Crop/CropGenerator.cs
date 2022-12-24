using System;
using SFG.MapSystem;
using UnityEngine;

namespace SFG.CropSystem
{
    public static partial class EventSystem
    {
        public static event Action PreGeneratedCropsEvent;

        public static void CallPreGeneratedCropsEvent()
        {
            PreGeneratedCropsEvent?.Invoke();
        }
    }

    public class CropGenerator : MonoBehaviour
    {
        private Grid m_CurrentGrid;
        [SerializeField] private int CropSeedID;     // 农作物种子ID
        [SerializeField] private int HaveGrowthDays; // 农作物已经成长的天数

        private void Awake()
        {
            m_CurrentGrid = FindObjectOfType<Grid>();
        }

        private void OnEnable()
        {
            EventSystem.PreGeneratedCropsEvent += PreGeneratedCrops;
        }

        private void OnDisable()
        {
            EventSystem.PreGeneratedCropsEvent -= PreGeneratedCrops;
        }

        /// <summary>
        /// 预先生成农作物
        /// </summary>
        private void PreGeneratedCrops()
        {
            Vector3Int cropGridPosition = m_CurrentGrid.WorldToCell(transform.position);

            if (CropSeedID != 0)
            {
                TileDetails tileDetails = TileMapManager.Instance.GetTileDetails(cropGridPosition);
                if (tileDetails == null)
                {
                    tileDetails = new TileDetails
                    {
                        GridX = cropGridPosition.x
                      , GridY = cropGridPosition.y
                    };
                }

                tileDetails.DaysSinceWatered = -1;
                tileDetails.SeedItemID = this.CropSeedID;
                tileDetails.HaveGrownDays = this.HaveGrowthDays;

                TileMapManager.Instance.UpdateDataInTileDetailsDict(tileDetails);
            }

            // 如果左操作数的值不为 null，则 null 合并运算符 ?? 返回该值；否则，它会计算右操作数并返回其结果。
        }
    }
}