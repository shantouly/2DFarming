using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Fram.Inventory
{
	public class InventoryUI : MonoBehaviour
	{
		[SerializeField]
		private SlotUI[] playerSlots;
		[Header("玩家背包UI")]
		[SerializeField] private GameObject bagUI;
		[Header("拖拽图片")]
		public Image dragItem;

		[Header("ToolTip")]
		public ItemToolTip itemToolTip;
		private bool openBag;

		[Header("通用背包")]
		[SerializeField] private GameObject baseBag;
		public GameObject shopSlotPrefab;
		public GameObject boxSlotPrefab;
		[Header("交易UI")]
		[SerializeField] private TradeUI tradeUI;
		[SerializeField] private List<SlotUI> baseBagSlot;
		public TextMeshProUGUI coinAmounts;

		private void OnEnable()
		{
			EventHandler.UpdateInventoryUI += OnUpdateInventoryUI;
			EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
			EventHandler.BaseBagOpenEvent += OnBaseBagOpenEvent;
			EventHandler.BaseBagCloseEvent += OnBaseBagCloseEvent;
			EventHandler.ShowTradeUI += OnShowTradeUI;
		}

		private void OnDisable()
		{
			EventHandler.UpdateInventoryUI -= OnUpdateInventoryUI;
			EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
			EventHandler.BaseBagOpenEvent -= OnBaseBagOpenEvent;
			EventHandler.BaseBagCloseEvent -= OnBaseBagCloseEvent;
			EventHandler.ShowTradeUI -= OnShowTradeUI;
		}

		private void OnShowTradeUI(ItemDetails itemDetails, bool isSell)
		{
			tradeUI.gameObject.SetActive(true);
			tradeUI.SetupTredeUI(itemDetails,isSell);
		}

		/// <summary>
		/// 关闭交易的事件
		/// </summary>
		/// <param name="slotType">背包类型</param>
		/// <param name="bag_SO">背包数据</param>
		private void OnBaseBagCloseEvent(slotType slotType, IventoryBag_SO bag_SO)
		{
			baseBag.SetActive(false);
			itemToolTip.gameObject.SetActive(false);
			
			foreach(var item in baseBagSlot)
			{
				Destroy(item.gameObject);
			}
			baseBagSlot.Clear();
			
			// 如果是进行商品的交易的话，同时关闭player身上的bag，并设置一下锚点坐标位置
			if(slotType == slotType.Shop)
			{
				bagUI.GetComponent<RectTransform>().pivot = new Vector2(0.5f,0.5f);
				bagUI.gameObject.SetActive(false);
				openBag = false;
			}
		}

		/// <summary>
		/// 打开交易的事件
		/// </summary>
		/// <param name="slotType">该背包的类型</param>
		/// <param name="bag_SO">背包数据</param>
		private void OnBaseBagOpenEvent(slotType slotType, IventoryBag_SO bag_SO)
		{
			GameObject prefab = slotType switch
			{
				slotType.Shop => shopSlotPrefab,
				slotType.Box => boxSlotPrefab,
				_=>null
			};
			
			// 显示背包
			baseBag.SetActive(true);
			baseBagSlot = new List<SlotUI>();
			
			for(int i =0;i < bag_SO.inventoryItemList.Count;i++)
			{
				var slot = Instantiate(prefab,baseBag.transform.GetChild(0)).GetComponent<SlotUI>();
				slot.slotIndex = i;
				baseBagSlot.Add(slot);
			}
			
			// 如果是进行商品的交易的话，同时打开player身上的bag，并设置一下锚点坐标位置
			if(slotType == slotType.Shop)
			{
				bagUI.GetComponent<RectTransform>().pivot = new Vector2(-1,0.5f);
				bagUI.gameObject.SetActive(true);
				openBag = true;
			}
			
			// 重新绘制
			LayoutRebuilder.ForceRebuildLayoutImmediate(baseBag.GetComponent<RectTransform>());
			OnUpdateInventoryUI(InventoryLocation.Box,bag_SO.inventoryItemList);
		}

		private void OnBeforeSceneUnloadEvent()
		{
			UpdateSlotHighLight(-1);
		}

		private void Start()
		{
			for(int i = 0; i < playerSlots.Length; i++)
			{
				playerSlots[i].slotIndex = i;
			}
			
			coinAmounts.text = InventoryManager.Instance.playermoney.ToString();
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.B))
			{
				OpenBagUI();
			}
		}

		private void OnUpdateInventoryUI(InventoryLocation location, List<InventoryItem> list)
		{
			switch (location)
			{
				case InventoryLocation.Player:
					for(int i = 0; i < playerSlots.Length; i++)
					{
						if (list[i].itemAmount > 0)
						{
							var item = InventoryManager.Instance.GetItemDetails(list[i].itemID);
							playerSlots[i].UpdateSlot(item, list[i].itemAmount);
						}
						else
						{
							playerSlots[i].UpdateEmptySlots();
						}
					}
					break;
				case InventoryLocation.Box:
					for(int i = 0; i < baseBagSlot.Count; i++)
					{
						if (list[i].itemAmount > 0)
						{
							var item = InventoryManager.Instance.GetItemDetails(list[i].itemID);
							baseBagSlot[i].UpdateSlot(item, list[i].itemAmount);
						}
						else
						{
							baseBagSlot[i].UpdateEmptySlots();
						}
					}
					break;
			}
			
			coinAmounts.text = InventoryManager.Instance.playermoney.ToString();
		}

		/// <summary>
		/// 这里想的话可以添加背包的Type，如果想每个背包中都可以实现高亮的话
		/// </summary>
		/// <param name="index"></param>
		public void UpdateSlotHighLight(int index)
		{
			for(int i = 0;i < playerSlots.Length; i++)
			{
				if (playerSlots[i].slotIndex == index)
				{
					playerSlots[i].slotHightLight.gameObject.SetActive(playerSlots[i].isSelected);
				}
				else
				{
					playerSlots[i].isSelected = false;
					playerSlots[i].slotHightLight.gameObject.SetActive(false);
				}
			}
		}

		public void OpenBagUI()
		{
			openBag = !openBag;
			bagUI.gameObject.SetActive(openBag);
		}
	}
}

