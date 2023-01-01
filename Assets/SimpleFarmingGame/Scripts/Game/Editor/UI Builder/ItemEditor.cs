using System.Collections.Generic;
using System.Linq;
using SimpleFarmingGame.Game;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ItemEditor : EditorWindow
{
    private ItemDataListSO m_DataBase;
    private List<ItemDetails> m_ItemList = new();
    private VisualTreeAsset m_ItemRowTemplate;
    private ScrollView m_ItemDetailsSection;
    private ItemDetails m_CurrentActiveItem;
    private Sprite m_DefaultIcon;
    private VisualElement m_IconPreview;
    private ListView m_ItemListView;

    [MenuItem("M STUDIO/ItemEditor")]
    public static void ShowExample()
    {
        ItemEditor wnd = GetWindow<ItemEditor>();
        wnd.titleContent = new GUIContent("ItemEditor");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            "Assets/SimpleFarmingGame/Scripts/Editor/UI Builder/ItemEditor.uxml");
        VisualElement labelFromUxml = visualTree.Instantiate();
        root.Add(labelFromUxml);

        //拿到模版数据
        m_ItemRowTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            "Assets/SimpleFarmingGame/Scripts/Editor/UI Builder/ItemRowTemplate.uxml");

        //拿默认Icon图片
        m_DefaultIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/M Studio/Art/Items/Icons/icon_M.png");

        //变量赋值
        m_ItemListView = root.Q<VisualElement>("ItemList").Q<ListView>("ListView");
        m_ItemDetailsSection = root.Q<ScrollView>("ItemDetails");
        m_IconPreview = m_ItemDetailsSection.Q<VisualElement>("Icon");

        //获得按键
        root.Q<Button>("AddButton").clicked += OnAddItemClicked;
        root.Q<Button>("DeleteButton").clicked += OnDeleteClicked;
        //加载数据
        LoadDataBase();

        //生成ListView
        GenerateListView();
    }

    #region 按键事件

    private void OnDeleteClicked()
    {
        m_ItemList.Remove(m_CurrentActiveItem);
        m_ItemListView.Rebuild();
        m_ItemDetailsSection.visible = false;
    }

    private void OnAddItemClicked()
    {
        ItemDetails newItem = new ItemDetails
        {
            ItemName = "NEW ITEM"
          , ItemID = 1001 + m_ItemList.Count
        };
        m_ItemList.Add(newItem);
        m_ItemListView.Rebuild();
    }

    #endregion

    private void LoadDataBase()
    {
        var dataArray = AssetDatabase.FindAssets("ItemDataListSO");

        if (dataArray.Length > 1)
        {
            var path = AssetDatabase.GUIDToAssetPath(dataArray[0]);
            m_DataBase = AssetDatabase.LoadAssetAtPath(path, typeof(ItemDataListSO)) as ItemDataListSO;
        }

        m_ItemList = m_DataBase.ItemDetailsList;
        //如果不标记则无法保存数据
        EditorUtility.SetDirty(m_DataBase);
        // Debug.Log(itemList[0].itemID);
    }

    private void GenerateListView()
    {
        VisualElement MakeItem() => m_ItemRowTemplate.CloneTree();

        void BindItem(VisualElement element, int index)
        {
            if (index < m_ItemList.Count)
            {
                if (m_ItemList[index].ItemIcon != null)
                {
                    element.Q<VisualElement>("Icon").style.backgroundImage = m_ItemList[index].ItemIcon.texture;
                }

                element.Q<Label>("Name").text = m_ItemList[index] == null
                    ? "NO ITEM"
                    : m_ItemList[index].ItemName;
            }
        }

        m_ItemListView.fixedItemHeight = 50; //根据需要高度调整数值
        m_ItemListView.itemsSource = m_ItemList;
        m_ItemListView.makeItem = MakeItem;
        m_ItemListView.bindItem = BindItem;

        m_ItemListView.onSelectionChange += OnListSelectionChange;

        //右侧信息面板不可见
        m_ItemDetailsSection.visible = false;
    }

    private void OnListSelectionChange(IEnumerable<object> selectedItem)
    {
        m_CurrentActiveItem = (ItemDetails)selectedItem.First();
        GetItemDetails();
        m_ItemDetailsSection.visible = true;
    }

    private void GetItemDetails()
    {
        m_ItemDetailsSection.MarkDirtyRepaint();

        m_ItemDetailsSection.Q<IntegerField>("ItemID").value = m_CurrentActiveItem.ItemID;
        m_ItemDetailsSection.Q<IntegerField>("ItemID").RegisterValueChangedCallback(evt =>
        {
            m_CurrentActiveItem.ItemID = evt.newValue;
        });

        m_ItemDetailsSection.Q<TextField>("ItemName").value = m_CurrentActiveItem.ItemName;
        m_ItemDetailsSection.Q<TextField>("ItemName").RegisterValueChangedCallback(evt =>
        {
            m_CurrentActiveItem.ItemName = evt.newValue;
            m_ItemListView.Rebuild();
        });

        m_IconPreview.style.backgroundImage = m_CurrentActiveItem.ItemIcon == null
            ? m_DefaultIcon.texture
            : m_CurrentActiveItem.ItemIcon.texture;

        m_ItemDetailsSection.Q<ObjectField>("ItemIcon").value = m_CurrentActiveItem.ItemIcon;
        m_ItemDetailsSection.Q<ObjectField>("ItemIcon").RegisterValueChangedCallback(evt =>
        {
            Sprite newIcon = evt.newValue as Sprite;
            m_CurrentActiveItem.ItemIcon = newIcon;

            m_IconPreview.style.backgroundImage = newIcon == null
                ? m_DefaultIcon.texture
                : newIcon.texture;
            m_ItemListView.Rebuild();
        });

        m_ItemDetailsSection.Q<ObjectField>("ItemSprite").value = m_CurrentActiveItem.ItemIconOnWorld;
        m_ItemDetailsSection.Q<ObjectField>("ItemSprite").RegisterValueChangedCallback(evt =>
        {
            m_CurrentActiveItem.ItemIconOnWorld = (Sprite)evt.newValue;
        });

        m_ItemDetailsSection.Q<EnumField>("ItemType").Init(m_CurrentActiveItem.ItemType);
        m_ItemDetailsSection.Q<EnumField>("ItemType").value = m_CurrentActiveItem.ItemType;
        m_ItemDetailsSection.Q<EnumField>("ItemType").RegisterValueChangedCallback(evt =>
        {
            m_CurrentActiveItem.ItemType = (ItemType)evt.newValue;
        });

        m_ItemDetailsSection.Q<TextField>("Description").value = m_CurrentActiveItem.ItemDescription;
        m_ItemDetailsSection.Q<TextField>("Description").RegisterValueChangedCallback(evt =>
        {
            m_CurrentActiveItem.ItemDescription = evt.newValue;
        });

        m_ItemDetailsSection.Q<IntegerField>("ItemUseRadius").value = m_CurrentActiveItem.ItemUseRadius;
        m_ItemDetailsSection.Q<IntegerField>("ItemUseRadius").RegisterValueChangedCallback(evt =>
        {
            m_CurrentActiveItem.ItemUseRadius = evt.newValue;
        });

        m_ItemDetailsSection.Q<Toggle>("CanPickedup").value = m_CurrentActiveItem.CanPickedUp;
        m_ItemDetailsSection.Q<Toggle>("CanPickedup").RegisterValueChangedCallback(evt =>
        {
            m_CurrentActiveItem.CanPickedUp = evt.newValue;
        });

        m_ItemDetailsSection.Q<Toggle>("CanDropped").value = m_CurrentActiveItem.CanDropped;
        m_ItemDetailsSection.Q<Toggle>("CanDropped").RegisterValueChangedCallback(evt =>
        {
            m_CurrentActiveItem.CanDropped = evt.newValue;
        });

        m_ItemDetailsSection.Q<Toggle>("CanCarried").value = m_CurrentActiveItem.CanCarried;
        m_ItemDetailsSection.Q<Toggle>("CanCarried").RegisterValueChangedCallback(evt =>
        {
            m_CurrentActiveItem.CanCarried = evt.newValue;
        });

        m_ItemDetailsSection.Q<IntegerField>("Price").value = m_CurrentActiveItem.ItemPrice;
        m_ItemDetailsSection.Q<IntegerField>("Price").RegisterValueChangedCallback(evt =>
        {
            m_CurrentActiveItem.ItemPrice = evt.newValue;
        });

        m_ItemDetailsSection.Q<Slider>("SellPercentage").value = m_CurrentActiveItem.SellPercentage;
        m_ItemDetailsSection.Q<Slider>("SellPercentage").RegisterValueChangedCallback(evt =>
        {
            m_CurrentActiveItem.SellPercentage = evt.newValue;
        });
    }
}