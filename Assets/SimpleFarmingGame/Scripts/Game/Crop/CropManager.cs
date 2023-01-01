using System.Linq;
using UnityEngine;

namespace SimpleFarmingGame.Game
{
    public class CropManager : Singleton<CropManager>
    {
        public CropDataListSO CropData;
        private Transform m_CropParent;
        private Grid m_CurrentGrid;
        private Season m_CurrentSeason;

        private void OnEnable()
        {
            EventSystem.UpdateSceneCropEvent += OnUpdateSceneCropEvent;
            EventSystem.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
            EventSystem.GameDayChangeEvent += OnGameDayChangeEvent;
        }

        private void OnDisable()
        {
            EventSystem.UpdateSceneCropEvent -= OnUpdateSceneCropEvent;
            EventSystem.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
            EventSystem.GameDayChangeEvent -= OnGameDayChangeEvent;
        }

        private void OnAfterSceneLoadedEvent()
        {
            m_CurrentGrid = FindObjectOfType<Grid>();
            m_CropParent = GameObject.FindWithTag("CropParent").transform;
        }

        private void OnUpdateSceneCropEvent(int cropSeedID, TileDetails tileDetails)
        {
            CropDetails currentCropDetails = GetCropDetails(cropSeedID);
            // 种子不为空，并且当前季节可以耕种这个农作物，并且地上的坑未种植任何东西
            if (currentCropDetails != null             &&
                IsSeasonCultivable(currentCropDetails) &&
                tileDetails.SeedItemID == -1) // 用于第一次种植农作物
            {
                tileDetails.SeedItemID = cropSeedID;
                tileDetails.HaveGrownDays = 0;
                DisplayCrop(tileDetails, currentCropDetails);
            }
            // 地上的坑已经有农作物了
            else if (tileDetails.SeedItemID != -1)
            {
                // 刷新地图上的农作物
                DisplayCrop(tileDetails, currentCropDetails);
            }
        }

        private void DisplayCrop(TileDetails tileDetails, CropDetails cropDetails)
        {
            int growthStage = cropDetails.GrowthDays.Length; // 成长阶段，例如土豆种子五个阶段
            int currentStage = 0;                            // 当前阶段
            int dayCounter = cropDetails.TotalGrowthDays;    // 农作物生长总天数
            // 倒序计算当前的成长阶段
            for (int i = growthStage - 1; i >= 0; --i) // growthStage - 1 是因为从0开始
            {
                if (tileDetails.HaveGrownDays < dayCounter)
                {
                    // 求 currentStage，当 HaveGrownDays > dayCounter 时可以求出当前阶段
                    dayCounter -= cropDetails.GrowthDays[i];
                }
                else
                {
                    currentStage = i;
                    break;
                }
            }

            //  在场景中显示当前农作物
            GameObject cropPrefab = cropDetails.GrowthPrefabs[currentStage];
            Sprite cropSprite = cropDetails.GrowthSprites[currentStage];
            Vector3 gridCenterPos = new(tileDetails.GridX + 0.5f, tileDetails.GridY + 0.5f, 0);
            GameObject crop = Instantiate(cropPrefab, gridCenterPos, Quaternion.identity, m_CropParent);
            crop.GetComponentInChildren<SpriteRenderer>().sprite = cropSprite;
            crop.GetComponent<Crop>().CropDetails = cropDetails;
            crop.GetComponent<Crop>().TileDetails = tileDetails;
        }

        private void OnGameDayChangeEvent(int day, Season season)
        {
            m_CurrentSeason = season;
        }

        private bool IsSeasonCultivable(CropDetails cropDetails)
        {
            // FIXME: Linq是否会产生GC，后续需要测试得知，如果产生GC需要修改所有使用了Linq语句的代码
            return cropDetails.Seasons.Any(season => season == m_CurrentSeason);
        }

        public CropDetails GetCropDetails(int cropSeedID) => CropData.GetCropDetails(cropSeedID);
    }
}