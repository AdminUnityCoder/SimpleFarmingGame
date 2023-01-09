using System;
using System.Collections.Generic;

namespace SimpleFarmingGame.Game
{
    [Serializable]
    public class GameSaveData
    {
        public string DataSceneName;

        /// <summary>
        /// key: 人物名字
        /// value: 坐标
        /// </summary>
        public Dictionary<string, SerializableVector3> CharactersPosDict;

        public int PlayerMoney;

        #region Item

        public Dictionary<string, List<SceneItem>> SceneItemDict;
        public Dictionary<string, List<SceneFurniture>> SceneFurnitureDict;

        #endregion

        #region Map

        /// <summary>
        /// key: coordinate + sceneName <br/>
        /// value: TileDetails
        /// </summary>
        public Dictionary<string, TileDetails> TileDetailsDict;

        /// <summary>
        /// 场景是否已经被加载过一次
        /// </summary>
        public Dictionary<string, bool> HasBeenLoadedSceneDict;

        #endregion

        #region Inventory

        /// <summary>
        /// 玩家背包数据 + 储物箱数据
        /// </summary>
        public Dictionary<string, List<InventoryItem>> InventoryDict;

        #endregion

        #region Time

        public Dictionary<string, int> TimeDict;

        #endregion

        #region NPC

        public string TargetScene;
        public bool CanInteractable;
        public int AnimationInstanceID;

        #endregion

        #region TimeLine

        public bool IsNewCutSceneFirstLoaded;

        #endregion
    }
}