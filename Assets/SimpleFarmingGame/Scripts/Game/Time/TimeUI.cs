using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleFarmingGame.Game
{
    public static partial class EventSystem
    {
        /// <summary>
        /// minute hour day season
        /// </summary>
        public static event Action<int, int, int, Season> GameHourMinuteChangeEvent;

        public static void CallGameMinuteChangeEvent(int minute, int hour, int day, Season season)
        {
            GameHourMinuteChangeEvent?.Invoke(minute, hour, day, season);
        }

        /// <summary>
        /// hour day month year season
        /// </summary>
        public static event Action<int, int, int, int, Season> GameDateChangeEvent;

        public static void CallGameDateChangeEvent(int hour, int day, int month, int year, Season season)
        {
            GameDateChangeEvent?.Invoke(hour, day, month, year, season);
        }
    }

    public class TimeUI : MonoBehaviour
    {
        public RectTransform DayAndNightImage;
        public RectTransform ClockParent;
        public TextMeshProUGUI GameDateText;
        public TextMeshProUGUI GameTimeText;
        public Image SeasonImage;
        public Sprite[] SeasonSprites;
        private List<GameObject> m_ClockBlocks = new();

        private void Awake()
        {
            InitClock();
        }

        private void OnEnable()
        {
            EventSystem.GameHourMinuteChangeEvent += OnGameHourMinuteChangeEvent;
            EventSystem.GameDateChangeEvent += OnGameDateChangeEvent;
        }

        private void OnDisable()
        {
            EventSystem.GameHourMinuteChangeEvent -= OnGameHourMinuteChangeEvent;
            EventSystem.GameDateChangeEvent -= OnGameDateChangeEvent;
        }

        private void OnGameHourMinuteChangeEvent(int minute, int hour, int day, Season season)
        {
            GameTimeText.text = hour.ToString("00") + ":" + minute.ToString("00");
        }

        // Update UI
        private void OnGameDateChangeEvent(int hour, int day, int month, int year, Season season)
        {
            GameDateText.text = year + "年" + month.ToString("00") + "月" + day.ToString("00") + "日";
            SeasonImage.sprite = SeasonSprites[(int)season];
            SwitchTimeBlockImage(hour);
            RotateDayAndNightImage(hour);
        }

        private void SwitchTimeBlockImage(int hour)
        {
            int index = hour / 4; // The hour goes from 0 to 23, index goes from 0 to 5.
            if (index == 0)
            {
                m_ClockBlocks[0].SetActive(true);
                for (int i = 1; i < m_ClockBlocks.Count; ++i)
                {
                    m_ClockBlocks[i].SetActive(false);
                }
            }
            else
            {
                for (int i = 0; i < m_ClockBlocks.Count; ++i)
                {
                    // index + 1 = 6, i goes from 0 to 5
                    m_ClockBlocks[i].SetActive(i < index + 1);
                }
            }
        }

        private void RotateDayAndNightImage(int hour)
        {
            // The image should start with the dark image, so we have to subtract 90 degrees.
            Vector3 endValue = new Vector3(0, 0, hour * 15 - 90);
            DayAndNightImage.DORotate(endValue: endValue, duration: 1f, mode: RotateMode.Fast);
        }

        private void InitClock()
        {
            for (int i = 0; i < ClockParent.childCount; ++i)
            {
                m_ClockBlocks.Add(ClockParent.GetChild(i).gameObject);
                ClockParent.GetChild(i).gameObject.SetActive(false);
            }
        }
    }
}