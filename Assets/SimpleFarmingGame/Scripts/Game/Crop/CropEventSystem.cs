using System;
using UnityEngine;

namespace SimpleFarmingGame.Game
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
        
        public static event Action PreGeneratedCropsEvent;

        public static void CallPreGeneratedCropsEvent()
        {
            PreGeneratedCropsEvent?.Invoke();
        }
        
        public static event Action<int, TileDetails> UpdateSceneCropEvent;

        public static void CallUpdateSceneCropEvent(int cropSeedID, TileDetails tileDetails)
        {
            UpdateSceneCropEvent?.Invoke(cropSeedID, tileDetails);
        }
    }
}