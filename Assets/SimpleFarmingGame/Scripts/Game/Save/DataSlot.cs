using System.Collections.Generic;

namespace SimpleFarmingGame.Game
{
    public class DataSlot
    {
        /// <summary>
        /// 进度条<br/>
        /// key: GUID<br/>
        /// value: GameSaveData类对象
        /// </summary>
        public Dictionary<string, GameSaveData> GameDataDict = new();

        public string GameTime
        {
            get
            {
                string key = TimeManager.Instance.GUID;
                if (GameDataDict.ContainsKey(key))
                {
                    GameSaveData timeData = GameDataDict[key];
                    return timeData.TimeDict["m_GameYear"]        + "年/"
                      + (Season)timeData.TimeDict["m_GameSeason"] + "/"
                      + timeData.TimeDict["m_GameMonth"]          + "月/"
                      + timeData.TimeDict["m_GameDay"]            + "日"
                      + timeData.TimeDict["m_GameHour"]           + "时"
                      + timeData.TimeDict["m_GameMinute"]         + "分";
                }

                return string.Empty;
            }
        }

        public string GameScene
        {
            get
            {
                string key = TransitionManager.Instance.GUID;
                if (GameDataDict.ContainsKey(key))
                {
                    GameSaveData transitionData = GameDataDict[key];
                    return transitionData.DataSceneName switch
                    {
                        "00.Sea" => "海边", "01.Field" => "农场", "02.Home" => "房间", "03.Stall" => "市场", _ => string.Empty
                    };
                }

                return string.Empty;
            }
        }
    }
}