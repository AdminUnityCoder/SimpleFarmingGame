using System;
using System.Collections;
using SFG.AudioSystem;
using SFG.Characters.Player;
using SFG.InventorySystem;
using SFG.MapSystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SFG.CropSystem
{
    public static partial class EventSystem
    {
        public static event Action<int> SpawnFruitAtPlayerPosition;

        public static void CallSpawnFruitAtPlayerPosition(int itemID)
        {
            SpawnFruitAtPlayerPosition?.Invoke(itemID);
        }

        public static event Action<ParticleEffectType, Vector3> ParticleEffectEvent;

        public static void CallParticleEffectEvent(ParticleEffectType type, Vector3 position)
        {
            ParticleEffectEvent?.Invoke(type, position);
        }
    }

    // 执行收割逻辑
    public class Crop : MonoBehaviour
    {
        public CropDetails CropDetails;
        public TileDetails TileDetails;
        private Animator m_Animator;
        private Vector3 m_CropPosition = Vector3.zero;
        private int m_ToolAlreadyUsedCount;
        public bool CanHarvest => TileDetails.HaveGrownDays >= CropDetails.TotalGrowthDays;

        private static readonly int RotateRight = Animator.StringToHash("RotateRight");
        private static readonly int RotateLeft = Animator.StringToHash("RotateLeft");
        private static readonly int FallingRight = Animator.StringToHash("FallingRight");
        private static readonly int FallingLeft = Animator.StringToHash("FallingLeft");

        public void Harvest(ItemDetails tool, TileDetails tileDetails)
        {
            TileDetails = tileDetails;
            m_ToolAlreadyUsedCount++; // 工具已经使用的次数

            int toolRequireUsedCount = CropDetails.GetToolRequireUsedCount(tool.ItemID);
            if (toolRequireUsedCount == -1) return;

            m_Animator = GetComponentInChildren<Animator>();

            if (m_ToolAlreadyUsedCount >= toolRequireUsedCount)
            {
                // 在玩家身上生成并且没有动画
                if (CropDetails.IsGeneratedOnPlayerPosition || !CropDetails.HasAnimation)
                {
                    // 生成果实
                    ProduceCropFruit();
                }
                else if (CropDetails.HasAnimation)
                {
                    m_Animator.SetTrigger
                        (PlayerModel.Instance.GetPosition.x < transform.position.x ? FallingRight : FallingLeft);

                    AudioSystem.EventSystem.CallPlaySoundEvent(SoundName.TreeFalling);
                    StartCoroutine(HarvestAfterAnimationCoroutine());
                }
            }
            else // m_ToolAlreadyUsedCount < toolRequireUsedCount
            {
                // 判断是否有动画
                if (m_Animator != null && CropDetails.HasAnimation)
                {
                    m_Animator.SetTrigger
                    (
                        PlayerModel.Instance.GetPosition.x < transform.position.x
                            ? RotateRight
                            : RotateLeft
                    );
                }

                // 播放粒子效果
                if (CropDetails.HasParticleEffect)
                {
                    EventSystem.CallParticleEffectEvent
                    (
                        CropDetails.ParticleEffectType
                      , transform.position + CropDetails.EffectGenerationPosition
                    );
                }

                // 播放声音
                if (CropDetails.SoundEffect != SoundName.None)
                {
                    AudioSystem.EventSystem.CallPlaySoundEvent(CropDetails.SoundEffect);
                }
            }
        }

        /// <summary>
        /// 生成农作物果实
        /// </summary>
        private void ProduceCropFruit()
        {
            #region 获取生成农作物果实的数量 -> 生成果实 -> 判断是否可重复生长

            for (int i = 0; i < CropDetails.HarvestFruitID.Length; ++i)
            {
                // 生成果实的数量
                int produceFruitCount = CropDetails.FruitMinAmount[i] == CropDetails.FruitMaxAmount[i]
                    ? CropDetails.FruitMaxAmount[i]
                    : Random.Range(CropDetails.FruitMinAmount[i], CropDetails.FruitMaxAmount[i] + 1);

                for (int j = 0; j < produceFruitCount; ++j)
                {
                    if (CropDetails.IsGeneratedOnPlayerPosition)
                    {
                        EventSystem.CallSpawnFruitAtPlayerPosition(CropDetails.HarvestFruitID[i]);
                    }
                    else // 在世界地图上生成物品
                    {
                        // 判断物品应该生成的方向
                        m_CropPosition = transform.position;
                        int directionX = m_CropPosition.x > PlayerModel.Instance.GetPosition.x ? 1 : -1;
                        // 物品生成的位置
                        Vector3 spawnPosition = new Vector3
                        (
                            m_CropPosition.x + Random.Range(directionX, CropDetails.SpawnRadius.x * directionX)
                          , m_CropPosition.y + Random.Range(-CropDetails.SpawnRadius.y, CropDetails.SpawnRadius.y)
                          , 0
                        );
                        InventorySystem.EventSystem.CallInstantiateItemInScene
                        (
                            CropDetails.HarvestFruitID[i]
                          , spawnPosition
                        );
                    }
                }
            }

            #endregion

            #region 判断是否可重复生长

            if (TileDetails == null) return;

            TileDetails.HasHarvestTimes++;
            // Debug.Log(TileDetails.HasHarvestTimes);
            // Debug.Log(CropDetails.RegrowTimes);

            // 可重复生长 
            if (CropDetails.DaysToRegrow > 0 && TileDetails.HasHarvestTimes < CropDetails.RegrowTimes - 1)
            {
                // 日期回退
                TileDetails.HaveGrownDays = CropDetails.TotalGrowthDays - CropDetails.DaysToRegrow;
                // 刷新种子
                MapSystem.EventSystem.CallRefreshCurrentSceneMap();
            }
            else // 不可重复生长
            {
                TileDetails.HasHarvestTimes = -1;
                TileDetails.SeedItemID = -1;
                // TileDetails.DaysSinceDug = -1;
            }

            #endregion

            Destroy(gameObject);
        }

        private IEnumerator HarvestAfterAnimationCoroutine()
        {
            while (!m_Animator.GetCurrentAnimatorStateInfo(0).IsName("End"))
            {
                yield return null;
            }

            ProduceCropFruit();
            // 转换新物体
            if (CropDetails.TransferNewItemID > 0)
            {
                TransferNewItemID();
            }
        }

        /// <summary>
        /// 转换成新物体
        /// </summary>
        private void TransferNewItemID()
        {
            TileDetails.SeedItemID = CropDetails.TransferNewItemID;
            TileDetails.HasHarvestTimes = -1;
            TileDetails.HaveGrownDays = 0;

            MapSystem.EventSystem.CallRefreshCurrentSceneMap();
        }
    }
}