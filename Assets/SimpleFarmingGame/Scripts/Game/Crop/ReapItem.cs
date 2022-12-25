using SimpleFarmingGame.Game;
using UnityEngine;

namespace SFG.CropSystem
{
    public class ReapItem : MonoBehaviour
    {
        private CropDetails m_CropDetails;
        private Vector3 m_CropPosition = Vector3.zero;

        public void InitCropDetails(int cropSeedID)
        {
            m_CropDetails = CropManager.Instance.GetCropDetails(cropSeedID);
        }

        /// <summary>
        /// 生成农作物果实
        /// </summary>
        public void ProduceCropFruit()
        {
            for (int i = 0; i < m_CropDetails.HarvestFruitID.Length; ++i)
            {
                // 生成果实的数量
                int produceFruitCount = m_CropDetails.FruitMinAmount[i] == m_CropDetails.FruitMaxAmount[i]
                    ? m_CropDetails.FruitMaxAmount[i]
                    : Random.Range(m_CropDetails.FruitMinAmount[i], m_CropDetails.FruitMaxAmount[i] + 1);

                for (int j = 0; j < produceFruitCount; ++j)
                {
                    if (m_CropDetails.IsGeneratedOnPlayerPosition)
                    {
                        EventSystem.CallSpawnFruitAtPlayerPosition(m_CropDetails.HarvestFruitID[i]);
                    }
                    else // 在世界地图上生成物品
                    {
                        // 判断物品应该生成的方向
                        m_CropPosition = transform.position;
                        int directionX = m_CropPosition.x > Player.Instance.Position.x ? 1 : -1;
                        // 物品生成的位置
                        Vector3 spawnPosition = new Vector3(
                            m_CropPosition.x + Random.Range(directionX, m_CropDetails.SpawnRadius.x * directionX)
                          , m_CropPosition.y + Random.Range(-m_CropDetails.SpawnRadius.y, m_CropDetails.SpawnRadius.y)
                          , 0);
                        InventorySystem.EventSystem.CallInstantiateItemInScene(
                            m_CropDetails.HarvestFruitID[i]
                          , spawnPosition);
                    }
                }
            }
        }
    }
}