using System;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleFarmingGame.Game
{
    public class TransactionUI : MonoBehaviour
    {
        public Image TransactionItemSprite; // 交易物品的图片
        public Text TransactionItemName;
        public InputField TransactionAmount;
        public Button SubmitButton;
        public Button CancelButton;

        private ItemDetails m_ItemDetails;
        private bool m_IsSell;

        private void Awake()
        {
            CancelButton.onClick.AddListener(CancelTransaction);
            SubmitButton.onClick.AddListener(TransactionItem);
        }

        public void SetupTransactionUI(ItemDetails itemDetails, bool isSell)
        {
            m_ItemDetails = itemDetails;
            m_IsSell = isSell;
            TransactionItemSprite.sprite = itemDetails.ItemIcon;
            TransactionItemName.text = itemDetails.ItemName;
            TransactionAmount.text = string.Empty;
        }

        private void CancelTransaction()
        {
            gameObject.SetActive(false);
        }

        private void TransactionItem()
        {
            int amount = Convert.ToInt32(TransactionAmount.text);
            InventoryManager.Instance.TransactionItem(m_ItemDetails, amount, m_IsSell);
            CancelTransaction();
        }
    }
}