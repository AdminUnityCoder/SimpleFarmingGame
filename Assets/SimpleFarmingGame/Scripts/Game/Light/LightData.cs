using System;
using System.Collections.Generic;
using SFG.TimeSystem;
using UnityEngine;

namespace SFG.LightSystem
{
    [CreateAssetMenu(fileName = "LightDataList_SO", menuName = "ScriptableObject/LightData")]
    public class LightData : ScriptableObject
    {
        public List<LightDetails> LightDetailsList;

        public LightDetails GetLightDetails(Season season, LightShift lightShift) =>
            LightDetailsList.Find(light => light.Season == season && light.LightShift == lightShift);
    }

    [Serializable]
    public class LightDetails
    {
        public Season Season;
        public LightShift LightShift;
        public Color LightColor;
        public float LightIntensity;
    }

    public enum LightShift
    {
        Morning, Night
    }
}