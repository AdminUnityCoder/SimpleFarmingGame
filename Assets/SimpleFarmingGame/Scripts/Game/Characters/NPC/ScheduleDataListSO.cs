using System.Collections.Generic;
using UnityEngine;

namespace SFG.Characters.NPC
{
    [CreateAssetMenu(fileName = "ScheduleDataListSO", menuName = "ScriptableObject/NPC Schedule/ScheduleDataList")]
    public class ScheduleDataListSO : ScriptableObject
    {
        public List<ScheduleDetails> ScheduleDetailsList;
    }
}