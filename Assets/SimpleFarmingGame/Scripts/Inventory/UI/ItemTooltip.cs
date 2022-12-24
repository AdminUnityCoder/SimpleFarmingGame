using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace SFG.InventorySystem
{
    public class ItemTooltip : MonoBehaviour
    {
        [SerializeField] private RectTransform RectTransform;

        [Header("ChildrenComponent")]
        [SerializeField]
        private TextMeshProUGUI NameText;

        [SerializeField] private TextMeshProUGUI TypeText;
        [SerializeField] private TextMeshProUGUI DescriptionText;
        [SerializeField] private Text PriceText;
        [SerializeField] private GameObject BottomGameObject;
        private readonly Vector2 m_BottomAnchorPoint = new(0.5f, 0f);
        [Header("建造家具")] public GameObject RequireResourcePanel;

        [SerializeField, Tooltip("建造家具所需要的资源")]
        private Image[] RequireResourceItems;

        private const string SEED = "种子";
        private const string COMMODITY = "商品";
        private const string FURNITURE = "家具";
        private const string TOOL = "工具";
        private const string NULL = "无";

        public void SetupTooltip(ItemDetails itemDetails, SlotType slotType)
        {
            // Set the anchor point at the bottom
            RectTransform.pivot = m_BottomAnchorPoint;

            #region Setup the tooltip content

            NameText.text = itemDetails.ItemName;
            TypeText.text = GetItemTypeChineseName(itemDetails.ItemType);
            DescriptionText.text = itemDetails.ItemDescription;

            if (itemDetails.ItemType is ItemType.Seed or ItemType.Furniture or ItemType.Commodity)
            {
                BottomGameObject.SetActive(true);

                int price = itemDetails.ItemPrice;

                if (slotType == SlotType.Bag) // Display sale price if the item in the player bag
                {
                    price = (int)(price * itemDetails.SellPercentage);
                }

                PriceText.text = price.ToString();

                #endregion
            }
            else
            {
                BottomGameObject.SetActive(false);
            }

            /*
             * public static void ForceRebuildLayoutImmediate(RectTransform layoutRoot);
             * 强制立即重新生成受计算影响的布局元素和子布局元素。
             */
            LayoutRebuilder.ForceRebuildLayoutImmediate(RectTransform);
        }

        public void SetupRequireResourcePanel(int buildingPaperID)
        {
            BluePrintDetails bluePrintDetails
                = InventoryManager.Instance.BluePrintData.GetBluePrintDetails(buildingPaperID);
            for (int i = 0; i < RequireResourceItems.Length; ++i) // 3个
            {
                if (i < bluePrintDetails.RequireResourceItems.Length) // 蓝图中的需求的个数
                {
                    RequireResourceItems[i].gameObject.SetActive(true);
                    RequireResourceItems[i].sprite = InventoryManager.Instance.GetItemDetails
                        (bluePrintDetails.RequireResourceItems[i].ItemID).ItemIcon;
                    RequireResourceItems[i].transform.GetChild(0).GetComponent<Text>().text
                        = bluePrintDetails.RequireResourceItems[i].ItemAmount.ToString();
                }
                else
                {
                    RequireResourceItems[i].gameObject.SetActive(false);
                }
            }
        }

        private static string GetItemTypeChineseName(ItemType itemType)
        {
            return itemType switch
            {
                ItemType.AxeTool => TOOL
              , ItemType.CollectTool => TOOL
              , ItemType.HoeTool => TOOL
              , ItemType.SickleTool => TOOL
              , ItemType.WaterTool => TOOL
              , ItemType.PickAxeTool => TOOL
              , ItemType.Seed => SEED
              , ItemType.Furniture => FURNITURE
              , ItemType.Commodity => COMMODITY
              , _ => NULL
            };
        }
    }
}