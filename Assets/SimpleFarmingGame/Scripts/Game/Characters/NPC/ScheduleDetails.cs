using System;
using SFG.TimeSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace SFG.Characters.NPC
{
    [Serializable]
    public class ScheduleDetails : IComparable<ScheduleDetails>
    {
        public int Day, Hour, Minute;
        [Tooltip("优先级越小越先执行")] public int Priority;
        public Season Season;
        public string TargetSceneName;
        public Vector2Int TargetGridPosition;
        public AnimationClip StopAnimationClip;

        [FormerlySerializedAs("InteractWithPlayer")]
        public bool CanInteractable;

        public ScheduleDetails
        (
            int day
          , int hour
          , int minute
          , int priority
          , Season season
          , string targetSceneName
          , Vector2Int targetGridPosition
          , AnimationClip stopAnimationClip
          , bool canInteractable
        )
        {
            Day = day;
            Hour = hour;
            Minute = minute;
            Priority = priority;
            Season = season;
            TargetSceneName = targetSceneName;
            TargetGridPosition = targetGridPosition;
            StopAnimationClip = stopAnimationClip;
            CanInteractable = canInteractable;
        }

        public int Time => Hour * 100 + Minute;

        public int CompareTo(ScheduleDetails other)
        {
            if (Time == other.Time)
            {
                if (Priority > other.Priority)
                {
                    return 1;
                }

                return -1; // Priority小的排前面
            }

            if (Time > other.Time)
            {
                return 1;
            }

            if (Time < other.Time)
            {
                return -1;
            }

            return 0;
        }
    }
}