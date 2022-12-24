using System.Collections.Generic;

namespace SFG.Characters.NPC
{
    public class NPCManager : Singleton<NPCManager>
    {
        public RouteMapDataListSO RouteMapData;
        public List<NPCPosition> NPCPositionList;

        /// <summary>
        /// key: route.FromSceneName + route.GotoSceneName
        /// Value: RouteMap
        /// </summary>
        private Dictionary<string, RouteMap> m_RouteMapDict = new();

        protected override void Awake()
        {
            base.Awake();
            InitializeRouteMapDict();
        }

        private void OnEnable()
        {
            UI.EventSystem.StartNewGameEvent += OnStartNewGameEvent;
        }

        private void OnDisable()
        {
            UI.EventSystem.StartNewGameEvent -= OnStartNewGameEvent;
        }

        private void OnStartNewGameEvent(int obj)
        {
            foreach (NPCPosition npcPosition in NPCPositionList)
            {
                npcPosition.NPCTransform.position = npcPosition.InitialPosition;
                npcPosition.NPCTransform.GetComponent<NPC>().StartScene = npcPosition.StartScene;
            }
        }

        private void InitializeRouteMapDict()
        {
            if (RouteMapData.RouteMapList.Count > 0)
            {
                foreach (RouteMap route in RouteMapData.RouteMapList)
                {
                    string key = route.FromSceneName + route.GotoSceneName;

                    if (m_RouteMapDict.ContainsKey(key)) continue;

                    m_RouteMapDict.Add(key, route);
                }
            }
        }

        /// <summary>
        /// 通过两个场景名获取两个场景间的路线图
        /// </summary>
        /// <param name="fromSceneName">起始场景</param>
        /// <param name="gotoSceneName">目标场景</param>
        /// <returns>从 Route Map 字典中返回对应的值</returns>
        public RouteMap GetRouteMap(string fromSceneName, string gotoSceneName) =>
            m_RouteMapDict[fromSceneName + gotoSceneName];
    }
}