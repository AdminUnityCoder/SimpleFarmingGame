using UnityEngine;

namespace SFG.InventorySystem
{
    public class Box : MonoBehaviour
    {
        public InventoryBagSO BoxBagTemplate;
        public InventoryBagSO BoxBagData;

        public GameObject RightMouseButtonSign;
        public int Index;

        private bool m_CanOpen;
        private bool m_IsOpened;

        private void OnEnable()
        {
            if (BoxBagData == null)
            {
                BoxBagData = Instantiate(BoxBagTemplate);
            }
        }

        private void Update()
        {
            if (m_IsOpened == false && m_CanOpen && Input.GetMouseButtonDown(1))
            {
                Characters.NPC.EventSystem.CallBaseBagOpenEvent(SlotType.Box, BoxBagData);
                m_IsOpened = true;
            }

            if (m_CanOpen == false && m_IsOpened)
            {
                Characters.NPC.EventSystem.CallBaseBagCloseEvent(SlotType.Box, BoxBagData);
                m_IsOpened = false;
            }

            if (m_IsOpened && Input.GetKeyDown(KeyCode.Escape))
            {
                Characters.NPC.EventSystem.CallBaseBagCloseEvent(SlotType.Box, BoxBagData);
                m_IsOpened = false;
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

        public void InitializeBox(int boxIndex )
        {
            Index = boxIndex;
            string key = name + Index;
            if (InventoryManager.Instance.GetBoxDataList(key) != null) // 刷新地图读取数据
            {
                BoxBagData.ItemList = InventoryManager.Instance.GetBoxDataList(key);
            }
            else // 新建格子
            {
                InventoryManager.Instance.AddBoxDataDict(this);
            }
        }
    }
}