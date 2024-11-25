using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Fram.Inventory
{
	public class SlotUI : MonoBehaviour, IPointerClickHandler,IBeginDragHandler,IDragHandler,IEndDragHandler
	{
		[Header("所需的参数")]
		[SerializeField] private Image slotImage;
		[SerializeField] private TextMeshProUGUI slotAmount;
		[SerializeField] public Image slotHightLight;
		[SerializeField] private Button button;

		[Header("格子的类型")]
		public slotType slotType;
		public bool isSelected;

		[Header("该格子中物品的数据")]
		public ItemDetails itemDetails;             // 即使没有，也会对其进行初始化，初始化中的值都是没有的。
		public int ItemAmount;

		[Header("格子编号")]
		public int slotIndex;
		
		private InventoryLocation location
		{
			get
			{
				return slotType switch
				{
					slotType.Bag => InventoryLocation.Player,
					slotType.Box => InventoryLocation.Box,
					_ => InventoryLocation.Player
				};
			}
		}

		public InventoryUI inventoryUI => GetComponentInParent<InventoryUI>();

		private void Start()
		{
			isSelected = false;

			/*if (itemDetails.itemID == 0)
			{
				UpdateEmptySlots();
			}*/
		}

		/// <summary>
		/// 当该格子中有物品的时候，要进行更新，并且将button设置为可交互的
		/// </summary>
		/// <param name="item"></param>
		/// <param name="amount"></param>
		public void UpdateSlot(ItemDetails item, int amount)
		{
			itemDetails = item;
			slotImage.sprite = item.itemIcon;
			ItemAmount = amount;
			slotAmount.text = amount.ToString();
			button.interactable = true;
			slotImage.enabled = true;
		}

		/// <summary>
		/// 当该格子中没有物品的时候，要将slotImage和slotAmount都设置看不见，并且该按钮不能进行交互
		/// </summary>
		public void UpdateEmptySlots()
		{
			if (isSelected)
			{
				isSelected = false;

				inventoryUI.UpdateSlotHighLight(-1);
				EventHandler.CallItemSelectedEvent(itemDetails, isSelected);
			}
			itemDetails = null;
			slotImage.enabled = false;
			slotAmount.text = "";
			button.interactable = false;
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (itemDetails == null) return;
			//Debug.Log(eventData.pointerCurrentRaycast.gameObject.name);
			isSelected = !isSelected;


			inventoryUI.UpdateSlotHighLight(slotIndex);

			if(slotType == slotType.Bag)
			{
				EventHandler.CallItemSelectedEvent(itemDetails, isSelected);
			}
		}

		/// <summary>
		/// 开始拖拽
		/// </summary>
		/// <param name="eventData"></param>
		public void OnBeginDrag(PointerEventData eventData)
		{
			if (ItemAmount == 0) return;
			//Debug.Log(ItemAmount);
			inventoryUI.dragItem.enabled = true;
			inventoryUI.dragItem.sprite = slotImage.sprite;
			isSelected = true;
			inventoryUI.UpdateSlotHighLight(slotIndex);
		}

		/// <summary>
		/// 拖拽过程
		/// </summary>
		/// <param name="eventData"></param>

		public void OnDrag(PointerEventData eventData)
		{
			inventoryUI.dragItem.transform.position = Input.mousePosition;

			//Debug.Log(eventData.pointerCurrentRaycast.gameObject.name);
		}

		/// <summary>
		/// 结束拖拽
		/// </summary>
		/// <param name="eventData"></param>
		public void OnEndDrag(PointerEventData eventData)
		{
			inventoryUI.dragItem.enabled = false;

			if(eventData.pointerCurrentRaycast.gameObject!=null)
			{
				//Debug.Log(eventData.pointerCurrentRaycast.gameObject.name);
				if (eventData.pointerCurrentRaycast.gameObject.GetComponent<SlotUI>() == null)
					return;

				var targetSlot = eventData.pointerCurrentRaycast.gameObject.GetComponent<SlotUI>();
				int targetIndex = targetSlot.slotIndex;

				if(slotType == slotType.Bag && targetSlot.slotType == slotType.Bag)
				{
					InventoryManager.Instance.SwapItem(slotIndex, targetIndex);
				}else if(slotType == slotType.Shop && targetSlot.slotType == slotType.Bag)  // 买
				{
					EventHandler.CallShowTradeUI(itemDetails,false);
				}else if(slotType == slotType.Bag && targetSlot.slotType == slotType.Shop)  // 卖
				{
					EventHandler.CallShowTradeUI(itemDetails,true);
				}else if(slotType != slotType.Shop && targetSlot.slotType != slotType.Shop && slotType != targetSlot.slotType)
				{
					InventoryManager.Instance.SwapItem(location,slotIndex,targetSlot.location,targetIndex);
				}
			}
		   /* else
			{
				// 获取丢弃的地方的坐标
				var pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));
				EventHandler.CallInstantiateItemInScene(itemDetails.itemID, pos);
			}*/

			isSelected = false;
			inventoryUI.UpdateSlotHighLight(-1);
		}
	}
}
