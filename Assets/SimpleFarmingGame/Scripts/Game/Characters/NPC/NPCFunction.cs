using UnityEngine;

namespace SimpleFarmingGame.Game
{
    public class NPCFunction : MonoBehaviour
    {
        public InventoryBagSO ShopData;
        private bool m_IsOpen;

        private void Update()
        {
            if (m_IsOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                // 关闭背包
                CloseShop();
            }
        }

        public void OpenShop()
        {
            m_IsOpen = true;
            EventSystem.CallBaseBagOpenEvent(SlotType.Shop, ShopData);
            EventSystem.CallUpdateGameStateEvent(GameState.Pause);
        }

        public void CloseShop()
        {
            m_IsOpen = false;
            EventSystem.CallBaseBagCloseEvent(SlotType.Shop, ShopData);
            EventSystem.CallUpdateGameStateEvent(GameState.Gameplay);
        }
    }
}