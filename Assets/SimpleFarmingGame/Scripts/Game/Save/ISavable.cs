namespace SimpleFarmingGame.Game
{
    public interface ISavable
    {
        string GUID { get; }

        void RegisterSavable()
        {
            SaveLoadManager.Instance.RegisterSavable(this);
        }

        /// <summary>
        /// 生成保存数据（保存）
        /// </summary>
        GameSaveData GenerateSaveData();

        /// <summary>
        /// 恢复数据（加载）
        /// </summary>
        void RestoreData(GameSaveData saveData);
    }
}