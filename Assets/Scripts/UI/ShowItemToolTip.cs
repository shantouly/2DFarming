using Fram.Inventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(SlotUI))]
public class ShowItemToolTip : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
	private SlotUI slotUI;
	private InventoryUI InventoryUI => GetComponentInParent<InventoryUI>();

	private void Awake()
	{
		slotUI = GetComponent<SlotUI>();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if(slotUI.ItemAmount != 0)
		{
			InventoryUI.itemToolTip.gameObject.SetActive(true);
			InventoryUI.itemToolTip.SetUpToolTip(slotUI.itemDetails, slotUI.slotType);
			// 这里用transform.position这样可以先出现在slot_bag上，因为该代码是挂载在这上面的
			
			//InventoryUI.itemToolTip.GetComponent<RectTransform>().pivot = new Vector2(0.5f,0);
			InventoryUI.itemToolTip.transform.position = transform.position + Vector3.up * 20;
			
			if(slotUI.itemDetails.itemtype == itemType.Furniture)
			{
				InventoryUI.itemToolTip.bluePrint.gameObject.SetActive(true);
				InventoryUI.itemToolTip.SetUpResourcePanel(InventoryManager.Instance.bluePrintDataList_SO.GetBluePrintDetails(slotUI.itemDetails.itemID));
			}else
			{
				InventoryUI.itemToolTip.bluePrint.gameObject.SetActive(false);
			}
		}
		else
		{
			InventoryUI.itemToolTip.gameObject.SetActive(false);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		InventoryUI.itemToolTip.gameObject.SetActive(false);
	}
}
