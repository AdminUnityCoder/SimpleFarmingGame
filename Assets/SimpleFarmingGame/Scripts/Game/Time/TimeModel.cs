using UnityEngine;

namespace SimpleFarmingGame.Game
{
    public static class TimeModel
    {
        [Tooltip("The smaller the number, the faster the time")]
        public const float SecondThreshold = 0.12f;

        public const int SecondLimit = 59;
        public const int MinuteLimit = 59;
        public const int HourLimit = 23;
        public const int DayLimit = 30;
        public const int MonthLimit = 12;
        public const int SeasonLimit = 3;
    }
}