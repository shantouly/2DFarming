using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ItemEditor : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;
    private ItemDataList_SO database;
    private List<ItemDetails> itemList;
    private VisualTreeAsset itemRowTemplate;
    private ListView itemListView;
    private ScrollView itemDetailsSection;
    private ItemDetails activeDetails;
    private VisualElement iconPreview;
    private Sprite defaultIcon;
    private ObjectField itemIcon;
    private itemType type;

    [MenuItem("UI_Bulider/ItemEditor")]
    public static void ShowExample() 
    {
        ItemEditor wnd = GetWindow<ItemEditor>();
        wnd.titleContent = new GUIContent("ItemEditor");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
     /*   VisualElement label = new Label("Hello World! From C#");
        root.Add(label);*/

        // Instantiate UXML
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);


        // 加载itemRowTemplate
        itemRowTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UI Bulider/UIRowTemplate.uxml");

        // 加载ListView
        itemListView = root.Q<VisualElement>("ItemList").Q<ListView>("ListView");

        // 加载ItemDetails
        itemDetailsSection = root.Q<ScrollView>("ItemDetails");

        // 获取右侧面板中的icon
        iconPreview = itemDetailsSection.Q<VisualElement>("Icon");

        // 获取两个按钮
        root.Q<Button>("AddBtn").clicked += OnAddItemClick;
        root.Q<Button>("Delete").clicked += OnDeleteItemClick;

        // 获取默认的Icon
        defaultIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/M Studio/Art/Items/Icons/icon_M.png");
        LoadDataBase();
        GenerateListView();
    }

    private void OnAddItemClick()
    {
        ItemDetails newItem = new ItemDetails();
        newItem.ItemName = "NEW ITEM";
        newItem.itemID = 1001 + itemList.Count;
        itemList.Add(newItem);
        itemListView.Rebuild();
    }

    private void OnDeleteItemClick()
    {
        itemList.Remove(activeDetails);
        itemListView.Rebuild();
        itemDetailsSection.visible = false;
    }

    private void LoadDataBase()
    {
        // 找到Asset中的ItemDataList_SO这一个数据库
        var dataArray = AssetDatabase.FindAssets("ItemDataList_SO");

        if (dataArray.Length > 0)
        {
            var path = AssetDatabase.GUIDToAssetPath(dataArray[0]);
            database = AssetDatabase.LoadAssetAtPath(path, typeof(ItemDataList_SO)) as ItemDataList_SO;
        }

        itemList = database.itemDetailsList;
        // 要进行标记，否则无法保存数据
        EditorUtility.SetDirty(database);
        // Debug.Log(itemList[0].itemID);
    }

    private void GenerateListView()
    {
        Func<VisualElement> makeItem = () => itemRowTemplate.CloneTree();

        Action<VisualElement, int> bindItem = (e, i) =>
        {
            // 确保当前的i不超过itemIist中的数量
            if (i < itemList.Count)
            {
                if (itemList[i].itemIcon != null)
                    e.Q<VisualElement>("Icon").style.backgroundImage = itemList[i].itemIcon.texture;
                e.Q<Label>("Name").text = itemList[i] == null ? "No ITEM" : itemList[i].ItemName;
            }
        };
        itemListView.fixedItemHeight = 60;
        itemListView.itemsSource = itemList;
        itemListView.makeItem = makeItem;
        itemListView.bindItem = bindItem;

        itemListView.selectionChanged += OnListSelectionChange;

        // 初始右侧面板看不见
        itemDetailsSection.visible = false;
    }

    private void OnListSelectionChange(IEnumerable<object> enumerable)
    {
        activeDetails =(ItemDetails)enumerable.First();
        GetItemDetails();
        itemDetailsSection.visible = true;
    }

    private void GetItemDetails()
    {
        itemDetailsSection.MarkDirtyRepaint();

        // ItemID
        itemDetailsSection.Q<IntegerField>("ItemID").value = activeDetails.itemID;
        itemDetailsSection.Q<IntegerField>("ItemID").RegisterValueChangedCallback(evt =>
        {
            activeDetails.itemID = evt.newValue;
        });

        // ItemName
        itemDetailsSection.Q<TextField>("ItemName").value = activeDetails.ItemName;
        itemDetailsSection.Q<TextField>("ItemName").RegisterValueChangedCallback(evt =>
        {
            activeDetails.ItemName = evt.newValue;
            itemListView.Rebuild();
        });

        // ItemIcon and Icon
        // 这里，如果我的DataList中的Icon没有被设置的话，点击的时候会出现空指针的错误，所以要设定一个DefaultIcon
        iconPreview.style.backgroundImage = activeDetails.itemIcon == null ? defaultIcon.texture : activeDetails.itemIcon.texture;
        itemDetailsSection.Q<ObjectField>("ItemIcon").value = activeDetails.itemIcon;
        itemDetailsSection.Q<ObjectField>("ItemIcon").RegisterValueChangedCallback(evt =>
        {
            Sprite newIcon = evt.newValue as Sprite;
            activeDetails.itemIcon = newIcon;

            iconPreview.style.backgroundImage = newIcon == null ? defaultIcon.texture : newIcon.texture;
            //iconPreview.style.backgroundImage = newIcon.texture
            itemListView.Rebuild();
        });

        // Item Sprite
        itemDetailsSection.Q<ObjectField>("ItemSprite").value = activeDetails.itemOnWorldSprite == null ? null : activeDetails.itemOnWorldSprite;
        itemDetailsSection.Q<ObjectField>("ItemSprite").RegisterValueChangedCallback(evt =>
        {
            activeDetails.itemOnWorldSprite = evt.newValue as Sprite;
        });

        // Description
        itemDetailsSection.Q<TextField>("Description").value = activeDetails.itemDescription == "" ? "" : activeDetails.itemDescription;
        itemDetailsSection.Q<TextField>("Description").RegisterValueChangedCallback(evt =>
        {
            activeDetails.itemDescription = evt.newValue;
        });

        // Item的使用范围
        itemDetailsSection.Q<IntegerField>("ItemUseRadius").value = activeDetails.itemUseRadius;
        itemDetailsSection.Q<IntegerField>("ItemUseRadius").RegisterValueChangedCallback(evt =>
        {
            activeDetails.itemUseRadius = evt.newValue;
        });

        // CanPickedUp
        itemDetailsSection.Q<Toggle>("CanPickedUp").value = activeDetails.canPickUp;
        itemDetailsSection.Q<Toggle>("CanPickedUp").RegisterValueChangedCallback(evt =>
        {
            activeDetails.canPickUp = evt.newValue;
        });

        // CanDropped
        itemDetailsSection.Q<Toggle>("CanDropped").value = activeDetails.canDropped;
        itemDetailsSection.Q<Toggle>("CanDropped").RegisterValueChangedCallback(evt =>
        {
            activeDetails.canDropped = evt.newValue;
        });

        // CanCarried
        itemDetailsSection.Q<Toggle>("CanCarried").value = activeDetails.canCarried;
        itemDetailsSection.Q<Toggle>("CanCarried").RegisterValueChangedCallback(evt =>
        {
            activeDetails.canCarried = evt.newValue;
        });

        // Price
        itemDetailsSection.Q<IntegerField>("Price").value = activeDetails.itemPrice;
        itemDetailsSection.Q<IntegerField>("Price").RegisterValueChangedCallback(evt =>
        {
            activeDetails.itemPrice = evt.newValue;
        });

        // Sell Percentage
        itemDetailsSection.Q<Slider>("SellPercentage").value = activeDetails.sellPercentage;
        itemDetailsSection.Q<Slider>("SellPercentage").RegisterValueChangedCallback(evt =>
        {
            activeDetails.sellPercentage = evt.newValue;
        });

        //Item Type --- 对于Enum Flags类型的UI组件的话，要先绑定一个枚举，然后再进行value的赋值和回调函数的使用
        itemDetailsSection.Q<EnumFlagsField>("ItemType").Init(itemType.Seed);
        itemDetailsSection.Q<EnumFlagsField>("ItemType").value = activeDetails.itemtype;
        itemDetailsSection.Q<EnumFlagsField>("ItemType").RegisterValueChangedCallback(evt =>
        {
            activeDetails.itemtype = (itemType)evt.newValue;
        });
    }
}
