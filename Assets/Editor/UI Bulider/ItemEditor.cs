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


        // ����itemRowTemplate
        itemRowTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UI Bulider/UIRowTemplate.uxml");

        // ����ListView
        itemListView = root.Q<VisualElement>("ItemList").Q<ListView>("ListView");

        // ����ItemDetails
        itemDetailsSection = root.Q<ScrollView>("ItemDetails");

        // ��ȡ�Ҳ�����е�icon
        iconPreview = itemDetailsSection.Q<VisualElement>("Icon");

        // ��ȡ������ť
        root.Q<Button>("AddBtn").clicked += OnAddItemClick;
        root.Q<Button>("Delete").clicked += OnDeleteItemClick;

        // ��ȡĬ�ϵ�Icon
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
        // �ҵ�Asset�е�ItemDataList_SO��һ�����ݿ�
        var dataArray = AssetDatabase.FindAssets("ItemDataList_SO");

        if (dataArray.Length > 0)
        {
            var path = AssetDatabase.GUIDToAssetPath(dataArray[0]);
            database = AssetDatabase.LoadAssetAtPath(path, typeof(ItemDataList_SO)) as ItemDataList_SO;
        }

        itemList = database.itemDetailsList;
        // Ҫ���б�ǣ������޷���������
        EditorUtility.SetDirty(database);
        // Debug.Log(itemList[0].itemID);
    }

    private void GenerateListView()
    {
        Func<VisualElement> makeItem = () => itemRowTemplate.CloneTree();

        Action<VisualElement, int> bindItem = (e, i) =>
        {
            // ȷ����ǰ��i������itemIist�е�����
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

        // ��ʼ�Ҳ���忴����
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
        // �������ҵ�DataList�е�Iconû�б����õĻ��������ʱ�����ֿ�ָ��Ĵ�������Ҫ�趨һ��DefaultIcon
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

        // Item��ʹ�÷�Χ
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

        //Item Type --- ����Enum Flags���͵�UI����Ļ���Ҫ�Ȱ�һ��ö�٣�Ȼ���ٽ���value�ĸ�ֵ�ͻص�������ʹ��
        itemDetailsSection.Q<EnumFlagsField>("ItemType").Init(itemType.Seed);
        itemDetailsSection.Q<EnumFlagsField>("ItemType").value = activeDetails.itemtype;
        itemDetailsSection.Q<EnumFlagsField>("ItemType").RegisterValueChangedCallback(evt =>
        {
            activeDetails.itemtype = (itemType)evt.newValue;
        });
    }
}
