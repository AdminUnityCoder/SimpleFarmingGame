using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using SFG.UI;
using UnityEngine;

namespace SFG.Save
{
    public class SaveLoadManager : Singleton<SaveLoadManager>
    {
        private List<ISavable> m_SavableList = new();
        public List<DataSlot> DataSlotList = new(new DataSlot[3]);

        private string m_JsonFolder;
        private int m_CurrentDataIndex;

        protected override void Awake()
        {
            base.Awake();
            // https://docs.unity3d.com/2021.3/Documentation/ScriptReference/Application-persistentDataPath.html
            /*
             * Windows Editor and Standalone Player: Application.persistentDataPath usually points to 
             * %userprofile%\AppData\LocalLow\<companyname>\<productname>.
            */
            m_JsonFolder = Application.persistentDataPath + "/SAVE DATA/";

            ReadGameProgress();
        }

        private void OnEnable()
        {
            EventSystem.StartNewGameEvent += OnStartNewGameEvent;
            EventSystem.EndGameEvent += OnEndGameEvent;
        }

        private void OnDisable()
        {
            EventSystem.StartNewGameEvent -= OnStartNewGameEvent;
            EventSystem.EndGameEvent -= OnEndGameEvent;
        }

        private void OnStartNewGameEvent(int index)
        {
            m_CurrentDataIndex = index;
        }

        private void OnEndGameEvent()
        {
            Save(m_CurrentDataIndex);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                Save(m_CurrentDataIndex);
            }

            if (Input.GetKeyDown(KeyCode.O))
            {
                Load(m_CurrentDataIndex);
            }
        }

        public void RegisterSavable(ISavable savable)
        {
            if (!m_SavableList.Contains(savable))
            {
                m_SavableList.Add(savable);
            }
        }

        private void Save(int index)
        {
            DataSlot data = new DataSlot();

            foreach (ISavable savable in m_SavableList)
            {
                data.GameDataDict.Add(savable.GUID, savable.GenerateSaveData());
            }

            DataSlotList[index] = data;

            string path = m_JsonFolder + "data_" + index + ".json";
            string jsonData = JsonConvert.SerializeObject(DataSlotList[index], Formatting.Indented);
            if (!File.Exists(path))
            {
                Directory.CreateDirectory(m_JsonFolder);
            }

            File.WriteAllText(path, jsonData);
            Debug.Log("DATA" + index + "SAVED!");
        }

        public void Load(int index)
        {
            m_CurrentDataIndex = index;

            string path = m_JsonFolder + "data_" + index + ".json";

            string data = File.ReadAllText(path);

            DataSlot json = JsonConvert.DeserializeObject<DataSlot>(data);

            foreach (ISavable savable in m_SavableList)
            {
                savable.RestoreData(json.GameDataDict[savable.GUID]);
            }

            Debug.Log("DATA" + index + "LOADED!");
        }

        private void ReadGameProgress()
        {
            if (Directory.Exists(m_JsonFolder))
            {
                for (int i = 0; i < DataSlotList.Count; ++i)
                {
                    string path = m_JsonFolder + "data_" + i + ".json";
                    if (File.Exists(path))
                    {
                        string data = File.ReadAllText(path);                          // 读取
                        DataSlot json = JsonConvert.DeserializeObject<DataSlot>(data); // 反序列化
                        DataSlotList[i] = json;                                        // 给字典复制
                    }
                }
            }
        }
    }
}