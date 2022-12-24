using System;
using SFG.Game;
using SFG.InventorySystem;
using UnityEngine;

namespace SFG.Characters.NPC
{
    public static class EventSystem
    {
        public static event Action<SlotType, InventoryBagSO> BaseBagOpenEvent;

        public static void CallBaseBagOpenEvent(SlotType slotType, InventoryBagSO bagData)
        {
            BaseBagOpenEvent?.Invoke(slotType, bagData);
        }

        public static event Action<SlotType, InventoryBagSO> BaseBagCloseEvent;

        public static void CallBaseBagCloseEvent(SlotType slotType, InventoryBagSO bagData)
        {
            BaseBagCloseEvent?.Invoke(slotType, bagData);
        }
    }

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
            Game.EventSystem.CallUpdateGameStateEvent(GameState.Pause);
        }

        public void CloseShop()
        {
            m_IsOpen = false;
            EventSystem.CallBaseBagCloseEvent(SlotType.Shop, ShopData);
            Game.EventSystem.CallUpdateGameStateEvent(GameState.Gameplay);
        }
    }
}