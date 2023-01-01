using System;
using System.Linq;
using UnityEngine;

namespace SimpleFarmingGame.Game
{
    [Serializable]
    public class CropDetails
    {
        [SerializeField] private string CropName;
        [Header("农作物种子ID")] public int CropSeedID;

        [Header("该农作物不同生长阶段需要的天数")] public int[] GrowthDays;
        public int TotalGrowthDays => GrowthDays.Sum();

        [Header("该农作物不同生长阶段的Prefab")] public GameObject[] GrowthPrefabs;

        [Header("该农作物不同生长阶段的图片")] public Sprite[] GrowthSprites;

        [Header("该农作物可种植的季节")] public Season[] Seasons;

        [Space] [Header("该农作物需要的收割工具的ID")] public int[] HarvestToolItemID;
        [Header("该农作物每种工具需要使用的次数")] public int[] ToolRequireUsedCount;

        [Header("该农作物转换成新物体的ID")] public int TransferNewItemID;

        [Space] [Header("收割果实的信息")] public int[] HarvestFruitID;
        [Header("生成果实最小数")] public int[] FruitMinAmount;
        [Header("生成果实最大数")] public int[] FruitMaxAmount;
        public Vector2 SpawnRadius; // 生成果实的范围

        [Header("该农作物再次生长的天数")] public int DaysToRegrow;
        [Header("该农作物再次生长的次数")] public int RegrowTimes; // 再次生长的次数

        [Header("其他选项")]
        [Tooltip("是否在玩家身上生成")]
        public bool IsGeneratedOnPlayerPosition;

        [Tooltip("是否有动画")] public bool HasAnimation;
        [Tooltip("是否有粒子特效")] public bool HasParticleEffect;

        public ParticleEffectType ParticleEffectType;
        public Vector3 EffectGenerationPosition;

        [Header("音效")] public SoundName SoundEffect;

        public bool CheckToolIsAvailable(int toolID)
        {
            for (int i = 0; i < HarvestToolItemID.Length; ++i)
            {
                if (HarvestToolItemID[i] == toolID)
                {
                    return true;
                }
            }

            return false;
        }

        public int GetToolRequireUsedCount(int toolID)
        {
            for (int i = 0; i < HarvestToolItemID.Length; ++i)
            {
                if (HarvestToolItemID[i] == toolID)
                {
                    return ToolRequireUsedCount[i];
                }
            }

            return -1;
        }
    }
}