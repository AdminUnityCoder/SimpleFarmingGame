using UnityEngine;
using UnityEngine.EventSystems;

namespace SFG.InventorySystem
{
    [RequireComponent(typeof(SlotUI))]
    public class ShowItemTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private SlotUI m_SlotUI;
        private InventoryUI InventoryUI => GetComponentInParent<InventoryUI>();

        private void Awake()
        {
            m_SlotUI = GetComponent<SlotUI>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (m_SlotUI.ItemDetails != null)
            {
                InventoryUI.ShowItemTooltip(m_SlotUI);
            }
            else
            {
                InventoryUI.HideItemTooltip();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            InventoryUI.HideItemTooltip();
        }
    }
}