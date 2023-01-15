using UnityEngine;

namespace SimpleFarmingGame.Game
{
    public class Box : MonoBehaviour
    {
        [SerializeField] private InventoryBagSO BoxBagTemplate;
        [SerializeField] private InventoryBagSO m_BoxBagData;

        [SerializeField] private GameObject RightMouseButtonSign;
        public int BoxIndex;

        private bool m_CanOpen;
        private bool m_IsOpened;
        public InventoryBagSO BoxBagData => m_BoxBagData;

        private void Update()
        {
            if (m_IsOpened == false && m_CanOpen && Input.GetMouseButtonDown(1))
            {
                EventSystem.CallBaseBagOpenEvent(SlotType.Box, m_BoxBagData);
                m_IsOpened = true;
            }

            if (m_CanOpen == false && m_IsOpened)
            {
                EventSystem.CallBaseBagCloseEvent(SlotType.Box, m_BoxBagData);
                m_IsOpened = false;
            }

            if (m_IsOpened && Input.GetKeyDown(KeyCode.Escape))
            {
                EventSystem.CallBaseBagCloseEvent(SlotType.Box, m_BoxBagData);
                m_IsOpened = false;
            }
        }

        private void OnEnable()
        {
            if (m_BoxBagData == null)
            {
                m_BoxBagData = Instantiate(BoxBagTemplate);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                m_CanOpen = true;
                RightMouseButtonSign.SetActive(true);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                m_CanOpen = false;
                RightMouseButtonSign.SetActive(false);
            }
        }

        public void InitializeBox(int boxIndex)
        {
            BoxIndex = boxIndex;
            string key = name + BoxIndex;
            if (InventoryManager.Instance.GetBoxDataList(key) != null) // 刷新地图读取数据
            {
                m_BoxBagData.ItemList = InventoryManager.Instance.GetBoxDataList(key);
            }
            else // 新建格子
            {
                InventoryManager.Instance.AddDataToStorageBoxDataDict(this);
            }
        }
    }
}