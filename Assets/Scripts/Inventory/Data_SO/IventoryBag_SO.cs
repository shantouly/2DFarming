using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="InventoryBag_SO",menuName ="Inventory/InventoryBag")]
public class IventoryBag_SO : ScriptableObject
{
	public List<InventoryItem> inventoryItemList;
	
	public InventoryItem GetInventoryItem(int ID)
	{
		return inventoryItemList.Find(i => i.itemID == ID);
	}
}
