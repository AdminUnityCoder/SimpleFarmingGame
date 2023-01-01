using System.Collections.Generic;
using UnityEngine;

namespace SimpleFarmingGame.Game
{
    [CreateAssetMenu(fileName = "ScheduleDataListSO", menuName = "ScriptableObject/NPC Schedule/ScheduleDataList")]
    public class ScheduleDataListSO : ScriptableObject
    {
        public List<ScheduleDetails> ScheduleDetailsList;
    }
}