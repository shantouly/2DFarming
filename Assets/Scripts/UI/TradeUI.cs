using System;
using System.Collections;
using System.Collections.Generic;
using Fram.Inventory;
using UnityEngine;
using UnityEngine.UI;

public class TradeUI : MonoBehaviour
{
	public Image iconImage;
	public Text itemName;
	public InputField tradeAmount;
	public Button submitBtn;
	public Button cancelBtn;
	private ItemDetails itemDetails;
	private bool isSell;
	
	void Awake()
	{
		submitBtn.onClick.AddListener(SubmitTradeUI);
		cancelBtn.onClick.AddListener(CancelTradeUI);
	}
	
	public void SetupTredeUI(ItemDetails itemDetails,bool isSell)
	{
		this.itemDetails = itemDetails;
		this.isSell = isSell;
		iconImage.sprite = itemDetails.itemIcon;
		itemName.text = itemDetails.ItemName;
		tradeAmount.text = string.Empty;
	}
	
	private void CancelTradeUI()
	{
		gameObject.SetActive(false);
	}
	
	private void SubmitTradeUI()
	{
		var amount = Convert.ToInt32(tradeAmount.text);
		InventoryManager.Instance.TradeItem(itemDetails,amount,isSell);
		
		CancelTradeUI();
	}
}
