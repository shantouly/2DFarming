using System.Collections;
using System.Collections.Generic;
using Fram.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemToolTip : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI nameText;
	[SerializeField] private TextMeshProUGUI typeText;
	[SerializeField] private TextMeshProUGUI descriptionText;
	[SerializeField] private TextMeshProUGUI valueText;
	[SerializeField] private GameObject buttonPart;
	public GameObject bluePrint;
	[SerializeField] private Image[] resourceItem;

	public void SetUpToolTip(ItemDetails itemDetails,slotType slotType)
	{
		//nameText.text = itemDetails.ItemName;
		//typeText.text = itemDetails.itemtype.ToString();
		//descriptionText.text = itemDetails.itemDescription;
		// typeText.text = GetItemType(itemDetails.itemtype);		// ��ȡ����type
		nameText.text = "name";
		typeText.text = "type";
		descriptionText.text = "description";

		if(itemDetails.itemtype == itemType.Seed || itemDetails.itemtype == itemType.Commodity || itemDetails.itemtype == itemType.Furniture)
		{
			// ������value�ǳ�����sellPercentage��
			buttonPart.SetActive(true);

			var price = itemDetails.itemPrice;
			if(slotType == slotType.Bag)
			{
				price = (int)(price * itemDetails.sellPercentage);
			}

			valueText.text = price.ToString();
		}
		else
		{
			buttonPart.SetActive(false);
		}

		LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
	}
	
	/// <summary>
	/// ������Ʒ�����ͻ�ȡ���ĵ�����
	/// </summary>
	/// <param name="itemType">��Ʒ������</param>
	/// <returns></returns>
	private string GetItemType(itemType itemType)
	{
		// TODO �������������������ʱ���ٽ��в���
		return itemType switch
		{
			itemType.Seed => "����",
			_=>""
		};
	}
	
	/// <summary>
	/// ���ý������
	/// </summary>
	/// <param name="bluePrintDetails"></param>
	public void SetUpResourcePanel(BluePrintDetails bluePrintDetails)
	{
		for(int i =0;i<resourceItem.Length;i++)
		{
			if(i < bluePrintDetails.resourceItem.Length)
			{
				var item = bluePrintDetails.resourceItem[i];
				resourceItem[i].gameObject.SetActive(true);
				resourceItem[i].sprite = InventoryManager.Instance.GetItemDetails(item.itemID).itemIcon;
				resourceItem[i].transform.GetChild(0).GetComponent<Text>().text = item.itemAmount.ToString();
			}else
			{
				resourceItem[i].gameObject.SetActive(false);
			}
		}
	}
}
