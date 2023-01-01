using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleFarmingGame.Game
{
    public static partial class EventSystem
    {
        /// <summary>
        /// day season
        /// </summary>
        public static event Action<int, Season> GameDayChangeEvent;

        public static void CallGameDayChangeEvent(int day, Season season)
        {
            GameDayChangeEvent?.Invoke(day, season);
        }
    }

    public enum Season { 春天, 夏天, 秋天, 冬天 }

    public class TimeManager : Singleton<TimeManager>, ISavable
    {
        private int m_GameSecond;
        private int m_GameMinute;
        private int m_GameHour;
        private int m_GameDay;
        private int m_GameMonth;
        private int m_GameYear;
        private int m_SeasonRemainingMonthNum = 3;
        private Season m_GameSeason = Season.春天;
        private bool m_GameClockPause;
        private float m_Timer;
        [SerializeField] private bool IsTimeAcceleration;

        private float m_TimeDifference; // 时间差
        public TimeSpan GameTime => new TimeSpan(m_GameHour, m_GameMinute, m_GameSecond);

        private void OnEnable()
        {
            EventSystem.BeforeSceneUnloadedEvent += OnBeforeSceneUnloadedEvent;
            EventSystem.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
            EventSystem.UpdateGameStateEvent += OnUpdateGameStateEvent;
            EventSystem.StartNewGameEvent += OnStartNewGameEvent;
            EventSystem.EndGameEvent += OnEndGameEvent;
        }

        private void OnDisable()
        {
            EventSystem.BeforeSceneUnloadedEvent -= OnBeforeSceneUnloadedEvent;
            EventSystem.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
            EventSystem.UpdateGameStateEvent -= OnUpdateGameStateEvent;
            EventSystem.StartNewGameEvent -= OnStartNewGameEvent;
            EventSystem.EndGameEvent -= OnEndGameEvent;
        }

        private void Start()
        {
            ISavable savable = this;
            savable.RegisterSavable();
            m_GameClockPause = true;
            // 因为在一开始 UI Scene 就已经 Unloaded 掉了，所以下面的 EventSystem 呼叫会导致空引用报错
            // EventSystem.CallGameMinuteChangeEvent(m_GameMinute, m_GameHour, m_GameDay, m_GameSeason);
            // EventSystem.CallGameDateChangeEvent(m_GameHour, m_GameDay, m_GameMonth, m_GameYear, m_GameSeason);
            // LightSystem.EventSystem.CallLightShiftChangeEvent(m_GameSeason, GetCurrentLightShift(), m_TimeDifference);
        }

        private void Update()
        {
            if (m_GameClockPause == false)
            {
                m_Timer += Time.deltaTime;
                if (m_Timer >= TimeModel.SecondThreshold)
                {
                    m_Timer -= TimeModel.SecondThreshold;
                    UpdateGameTime();
                }
            }

            if (IsTimeAcceleration && Input.GetKey(KeyCode.T))
            {
                for (int i = 0; i < 60; ++i)
                {
                    UpdateGameTime();
                }
            }

            if (IsTimeAcceleration && Input.GetKeyDown(KeyCode.G))
            {
                m_GameDay++;
                EventSystem.CallGameDayChangeEvent(m_GameDay, m_GameSeason);
                EventSystem.CallGameDateChangeEvent(m_GameHour, m_GameDay, m_GameMonth, m_GameYear, m_GameSeason);
            }
        }

        private void OnBeforeSceneUnloadedEvent()
        {
            m_GameClockPause = true;
        }

        private void OnAfterSceneLoadedEvent()
        {
            m_GameClockPause = false;
            EventSystem.CallGameMinuteChangeEvent(m_GameMinute, m_GameHour, m_GameDay, m_GameSeason);
            EventSystem.CallGameDateChangeEvent(m_GameHour, m_GameDay, m_GameMonth, m_GameYear, m_GameSeason);
            EventSystem.CallLightShiftChangeEvent(m_GameSeason, GetCurrentLightShift(), m_TimeDifference);
        }

        private void OnUpdateGameStateEvent(GameState gameState)
        {
            m_GameClockPause = gameState == GameState.Pause;
        }

        private void OnStartNewGameEvent(int obj)
        {
            InitGameTime();
            m_GameClockPause = false; // 此处是否多余，因为 OnAfterSceneLoadedEvent() 中已经设置过一次
            Debug.Log(m_GameClockPause);
        }

        private void OnEndGameEvent()
        {
            m_GameClockPause = true;
        }

        private void InitGameTime()
        {
            m_GameSecond = 0;
            m_GameMinute = 0;
            m_GameHour = 7;
            m_GameDay = 1;
            m_GameMonth = 1;
            m_GameYear = 2022;
            m_GameSeason = Season.春天;
        }

        private void UpdateGameTime()
        {
            m_GameSecond++;
            if (m_GameSecond > TimeModel.SecondLimit)
            {
                m_GameSecond = 0;
                m_GameMinute++;
                if (m_GameMinute > TimeModel.MinuteLimit)
                {
                    m_GameMinute = 0;
                    m_GameHour++;
                    if (m_GameHour > TimeModel.HourLimit)
                    {
                        m_GameHour = 0;
                        m_GameDay++;
                        if (m_GameDay > TimeModel.DayLimit)
                        {
                            m_GameDay = 1;
                            m_GameMonth++;
                            if (m_GameMonth > TimeModel.MonthLimit)
                            {
                                m_GameMonth = 1;
                            }

                            m_SeasonRemainingMonthNum--;
                            if (m_SeasonRemainingMonthNum == 0) // 2 1 0
                            {
                                m_SeasonRemainingMonthNum = 3; // Reset

                                int seasonNumber = (int)m_GameSeason;
                                seasonNumber++;
                                if (seasonNumber > TimeModel.SeasonLimit)
                                {
                                    seasonNumber = 0;
                                    m_GameYear++;
                                }

                                m_GameSeason = (Season)seasonNumber;
                                if (m_GameYear > 9999)
                                {
                                    m_GameYear = 2022;
                                }
                            }
                        }

                        // Update map info and update crop grow
                        EventSystem.CallGameDayChangeEvent(m_GameDay, m_GameSeason);
                    }

                    EventSystem.CallGameDateChangeEvent(m_GameHour, m_GameDay, m_GameMonth, m_GameYear, m_GameSeason);
                }

                EventSystem.CallGameMinuteChangeEvent(m_GameMinute, m_GameHour, m_GameDay, m_GameSeason);
                // 切换灯光
                EventSystem.CallLightShiftChangeEvent
                    (m_GameSeason, GetCurrentLightShift(), m_TimeDifference);
            }

            // Debug.Log("Minute: " + m_GameMinute + "Second: " + m_GameSecond);
        }

        private LightShift GetCurrentLightShift()
        {
            if (GameTime >= LightModel.MorningTime && GameTime < LightModel.NightTime)
            {
                m_TimeDifference = (float)(GameTime - LightModel.MorningTime).TotalMinutes;
                return LightShift.Morning;
            }

            if (GameTime < LightModel.MorningTime || GameTime >= LightModel.NightTime)
            {
                m_TimeDifference = Mathf.Abs((float)(GameTime - LightModel.NightTime).TotalMinutes);
                return LightShift.Night;
            }

            return LightShift.Morning;
        }

        public string GUID => GetComponent<DataGUID>().GUID;

        /// <summary>
        /// 保存游戏时间
        /// </summary>
        /// <returns></returns>
        public GameSaveData GenerateSaveData()
        {
            GameSaveData saveData = new GameSaveData();
            saveData.TimeDict = new Dictionary<string, int>();
            saveData.TimeDict.Add("m_GameYear", m_GameYear);
            saveData.TimeDict.Add("m_GameSeason", (int)m_GameSeason);
            saveData.TimeDict.Add("m_GameMonth", m_GameMonth);
            saveData.TimeDict.Add("m_GameDay", m_GameDay);
            saveData.TimeDict.Add("m_GameHour", m_GameHour);
            saveData.TimeDict.Add("m_GameMinute", m_GameMinute);
            saveData.TimeDict.Add("m_GameSecond", m_GameSecond);

            return saveData;
        }

        /// <summary>
        /// 恢复游戏时间
        /// </summary>
        /// <param name="saveData"></param>
        public void RestoreData(GameSaveData saveData)
        {
            m_GameYear = saveData.TimeDict["m_GameYear"];
            m_GameSeason = (Season)saveData.TimeDict["m_GameSeason"];
            m_GameMonth = saveData.TimeDict["m_GameMonth"];
            m_GameDay = saveData.TimeDict["m_GameDay"];
            m_GameHour = saveData.TimeDict["m_GameHour"];
            m_GameMinute = saveData.TimeDict["m_GameMinute"];
            m_GameSecond = saveData.TimeDict["m_GameSecond"];
        }
    }
}