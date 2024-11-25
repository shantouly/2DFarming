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
		[Header("����Ĳ���")]
		[SerializeField] private Image slotImage;
		[SerializeField] private TextMeshProUGUI slotAmount;
		[SerializeField] public Image slotHightLight;
		[SerializeField] private Button button;

		[Header("���ӵ�����")]
		public slotType slotType;
		public bool isSelected;

		[Header("�ø�������Ʒ������")]
		public ItemDetails itemDetails;             // ��ʹû�У�Ҳ�������г�ʼ������ʼ���е�ֵ����û�еġ�
		public int ItemAmount;

		[Header("���ӱ��")]
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
		/// ���ø���������Ʒ��ʱ��Ҫ���и��£����ҽ�button����Ϊ�ɽ�����
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
		/// ���ø�����û����Ʒ��ʱ��Ҫ��slotImage��slotAmount�����ÿ����������Ҹð�ť���ܽ��н���
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
		/// ��ʼ��ק
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
		/// ��ק����
		/// </summary>
		/// <param name="eventData"></param>

		public void OnDrag(PointerEventData eventData)
		{
			inventoryUI.dragItem.transform.position = Input.mousePosition;

			//Debug.Log(eventData.pointerCurrentRaycast.gameObject.name);
		}

		/// <summary>
		/// ������ק
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
				}else if(slotType == slotType.Shop && targetSlot.slotType == slotType.Bag)  // ��
				{
					EventHandler.CallShowTradeUI(itemDetails,false);
				}else if(slotType == slotType.Bag && targetSlot.slotType == slotType.Shop)  // ��
				{
					EventHandler.CallShowTradeUI(itemDetails,true);
				}else if(slotType != slotType.Shop && targetSlot.slotType != slotType.Shop && slotType != targetSlot.slotType)
				{
					InventoryManager.Instance.SwapItem(location,slotIndex,targetSlot.location,targetIndex);
				}
			}
		   /* else
			{
				// ��ȡ�����ĵط�������
				var pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));
				EventHandler.CallInstantiateItemInScene(itemDetails.itemID, pos);
			}*/

			isSelected = false;
			inventoryUI.UpdateSlotHighLight(-1);
		}
	}
}
