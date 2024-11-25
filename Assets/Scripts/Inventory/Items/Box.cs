using System.Collections;
using System.Collections.Generic;
using Fram.Inventory;
using UnityEngine;

public class Box : MonoBehaviour
{
	public IventoryBag_SO boxBagTemplate;
	public IventoryBag_SO boxBagData;
	public GameObject mouseIcon;
	public bool isStart;
	private bool canOpen;
	private bool isOpen;
	public int index;
	
	void OnEnable()
	{
		if(boxBagData == null)
		{
			boxBagData = Instantiate(boxBagTemplate);
			InitStartBox();
			//Debug.Log("recreateinitboxes");
		}
		
		//EventHandler.AfterSceneUnloadEvent += OnAfterSceneUnloadEvent;
		//EventHandler.StartNewGameEvent += OnStartNewGameEvent;
	}
	
	void OnDisable()
	{
		//EventHandler.AfterSceneUnloadEvent -= OnAfterSceneUnloadEvent;
		//EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
	}

	// private void OnStartNewGameEvent(int obj)
	// {
	// 	if(isStart)
	// 	{
	// 		InitStartBox();		
	// 	}
	// }

	// private void OnAfterSceneUnloadEvent()
	// {
	// 	if(isStart)
	// 	{
	// 		//InitStartBox();
	// 		Debug.Log("recreateinitboxes");
	// 	}
	// }

	void OnTriggerEnter2D(Collider2D other)
	{
		if(other.gameObject.CompareTag("Player"))
		{
			canOpen = true;
			mouseIcon.SetActive(true);
		}
	}
	
	void OnTriggerExit2D(Collider2D other)
	{
		if(other.gameObject.CompareTag("Player"))
		{
			canOpen = false;
			mouseIcon.SetActive(false);
		}
	}
	
	void Update()
	{
		if(!isOpen && canOpen && Input.GetMouseButtonDown(1))
		{
			// 打开箱子
			EventHandler.CallBaseBagOpenEvent(slotType.Box,boxBagData);
			isOpen = true;
		}
		
		if(isOpen && !canOpen)
		{
			// 关闭箱子
			EventHandler.CallBaseBagCloseEvent(slotType.Box,boxBagData);
			isOpen = false;
		}
		
		if(isOpen && Input.GetKeyDown(KeyCode.Escape))
		{
			// 关闭箱子
			EventHandler.CallBaseBagCloseEvent(slotType.Box,boxBagData);
			isOpen = false;
		}
	}
	
	public void Init(int boxIndex)
	{
		index = boxIndex;
		
		var key = this.name + index;
		if(InventoryManager.Instance.boxDataList.ContainsKey(key))
		{
			boxBagData.inventoryItemList = InventoryManager.Instance.GetBoxDataList(key);
		}else
		{
			InventoryManager.Instance.AddBoxDataDict(this);
		}
	}
	
	public void InitStartBox()
	{
		var key = this.name + index;
		//Debug.Log(key);
		if(InventoryManager.Instance.boxStartDataDict.ContainsKey(key))
		{
			//Debug.Log("contains");
			boxBagData.inventoryItemList = InventoryManager.Instance.GetStartBoxDataList(key);
		}else
		{
			//Debug.Log("nocontains");
			InventoryManager.Instance.AddBoxStartDataDict(this);
		}
	}
}
