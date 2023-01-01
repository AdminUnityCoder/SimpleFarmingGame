using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleFarmingGame.Game
{
    [CreateAssetMenu(fileName = "SoundDetailsListSO", menuName = "ScriptableObject/Audio/SoundDetailsList")]
    public class SoundDetailsListSO : ScriptableObject
    {
        public List<SoundDetails> SoundDetailsList;

        public SoundDetails GetSoundDetails(SoundName soundName) =>
            SoundDetailsList.Find(soundDetails => soundDetails.SoundName == soundName);
    }

    [Serializable]
    public class SoundDetails
    {
        public SoundName SoundName;
        public AudioClip SoundClip;
        [Range(0.1f, 1.5f)] public float MinSoundPitch = 0.8f;
        [Range(0.1f, 1.5f)] public float MaxSoundPitch = 1.2f;
        [Range(0.1f, 1.0f)] public float SoundVolume = 0.2f;
    }

    public enum SoundName
    {
        None
      , Countryside1
      , Countryside2
      , Indoors1
      , PlantSeed
      , Pluck
      , StoneShatter
      , TreeFalling
      , WoodSplinters
      , FootStepSoft
      , FootStepHard
      , Rustle
      , Calm1
      , Calm2
      , Calm3
      , Calm4
      , Calm5
      , Calm6
      , PickupPop
      , Axe
      , Basket
      , Hoe
      , Pickaxe
      , Scythe
      , WateringCan
    }
}