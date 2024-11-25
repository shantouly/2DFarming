using System.Collections;
using System.Collections.Generic;
using Fram.Save;
using UnityEngine;

namespace Fram.Inventory
{
	public class InventoryManager : Singleton<InventoryManager>,ISaveable
	{
		[Header("��Ʒ����")]
		public ItemDataList_SO itemDataList_SO;
		[Header("����ͼֽ")]
		public BluePrintDataList_SO bluePrintDataList_SO;
		[Header("��������")]
		public IventoryBag_SO playerBagTemp;
		public IventoryBag_SO playerBag;
		public IventoryBag_SO currentBag;
		[Header("����")]
		public int playermoney;
		
		public Dictionary<string,List<InventoryItem>> boxDataList = new Dictionary<string, List<InventoryItem>>();
		public Dictionary<string,List<InventoryItem>> boxStartDataDict = new Dictionary<string, List<InventoryItem>>();

		public string GUID => GetComponent<DataGUID>().guid;

		private void OnEnable()
		{
			EventHandler.DropItemEvent += OnDropItemEvent;
			EventHandler.HarvestAtPlayerPosition+=OnHarvestAtPlayerPosition;
			EventHandler.BuildFurnitureEvent += OnBuildFurnitureEvent;
			EventHandler.BaseBagOpenEvent += OnBaseBagOpenEvent;
			EventHandler.StartNewGameEvent += OnStartNewGameEvent;
		}

		private void OnDisable()
		{
			EventHandler.DropItemEvent -= OnDropItemEvent;
			EventHandler.HarvestAtPlayerPosition -=OnHarvestAtPlayerPosition;
			EventHandler.BuildFurnitureEvent -= OnBuildFurnitureEvent;
			EventHandler.BaseBagOpenEvent -= OnBaseBagOpenEvent;
			EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
		}

		private void Start()
		{
			ISaveable saveable = this;
			saveable.RegisterSaveable();
			//EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.inventoryItemList);
		}
		
		// void Update()
		// {
		// 	Debug.Log(boxStartDataDict.Count);
		// }
		
		/// <summary>
		/// ��ʼһ������Ϸ��Ҫ��������
		/// </summary>
		/// <param name="index"></param>
		private void OnStartNewGameEvent(int index)
		{
			playerBag = Instantiate(playerBagTemp);
			playermoney = Settings.playerMoney;
			boxDataList.Clear();
			boxStartDataDict.Clear();
			EventHandler.CallUpdateInventoryUI(InventoryLocation.Player,playerBag.inventoryItemList);
		}
		private void OnBaseBagOpenEvent(slotType slotType, IventoryBag_SO currentBag_SO)
		{
			currentBag = currentBag_SO;
		}

		private void OnBuildFurnitureEvent(int itemID, Vector3 mousePosOnWorld)
		{
			RemoveItem(itemID,1);
			
			BluePrintDetails bluePrintDetails = bluePrintDataList_SO.GetBluePrintDetails(itemID);
			
			foreach(var item in bluePrintDetails.resourceItem)
			{
				RemoveItem(item.itemID,item.itemAmount);
			}
		}
		private void OnDropItemEvent(int ID, Vector3 position,itemType itemType)
		{
			RemoveItem(ID, 1);
		}
		
		private void OnHarvestAtPlayerPosition(int itemID)
		{
			var index = GetItemIndexAtBag(itemID);

			AddItemAtIndex(itemID, index, 1);
			
			EventHandler.CallUpdateInventoryUI(InventoryLocation.Player,playerBag.inventoryItemList);
		}

		public ItemDetails GetItemDetails(int ID)
		{
			return itemDataList_SO.itemDetailsList.Find(i => i.itemID == ID);
		}

		/// <summary>
		/// �����Ʒ��������
		/// </summary>
		/// <param name="item">�����ϵ���Ʒ���صĽű�</param>
		/// <param name="CanDestory">ʰȡ֮���Ƿ��������</param>
		public void AddItem(Item item,bool CanDestory)
		{
			var index = GetItemIndexAtBag(item.itemID);

			AddItemAtIndex(item.itemID, index, 1);
			if (CanDestory)
			{
				Destroy(item.gameObject);
			}

			// ����UI
			EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.inventoryItemList);
		}

		/// <summary>
		/// ��鱳���������Ƿ��пյ�
		/// </summary>
		/// <returns></returns>
		private bool ChenkBagCapacity()
		{
			for(int i = 0; i < playerBag.inventoryItemList.Count; i++)
			{
				if (playerBag.inventoryItemList[i].itemID == 0)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// ������������и���Ʒ�����ظ���Ʒ�ڱ��������index
		/// </summary>
		/// <param name="ID">��ƷID</param>
		/// <returns></returns>
		private int GetItemIndexAtBag(int ID)
		{
			for(int i = 0; i < playerBag.inventoryItemList.Count; i++)
			{
				if (playerBag.inventoryItemList[i].itemID == ID)
				{
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// ����Ʒ��ӵ�������
		/// </summary>
		/// <param name="ID">��Ʒ��ID</param>
		/// <param name="index">��Ʒ�ڱ��������index,����������еĻ�</param>
		/// <param name="amount">�����Ʒ������</param>
		private void AddItemAtIndex(int ID,int index,int amount)
		{
			if(index == -1 && ChenkBagCapacity())         // ������û�и���Ʒ���ұ������п���������ʱ��
			{
				var item = new InventoryItem { itemID = ID, itemAmount = amount };
				for(int i = 0; i < playerBag.inventoryItemList.Count; i++)
				{
					if (playerBag.inventoryItemList[i].itemID == 0)
					{
						playerBag.inventoryItemList[i] = item;
						return;
					}
				}
			}                       // �������и���Ʒ��ʱ��
			else
			{
				//Debug.Log("ID" + "....." + index);
				int currentAmount = playerBag.inventoryItemList[index].itemAmount + amount;
				var item = new InventoryItem { itemID = ID, itemAmount = currentAmount };

				playerBag.inventoryItemList[index] = item;
			}
		}

		public void SwapItem(int formIndex,int targetIndex)
		{
			var fromItem = playerBag.inventoryItemList[formIndex];
			var targetItem = playerBag.inventoryItemList[targetIndex];

			if(targetItem.itemID != 0)
			{
				playerBag.inventoryItemList[formIndex] = targetItem;
				playerBag.inventoryItemList[targetIndex] = fromItem;
			}
			else
			{
				playerBag.inventoryItemList[targetIndex] = fromItem;
				playerBag.inventoryItemList[formIndex] = new InventoryItem();
			}

			EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.inventoryItemList);
		}
		
		public void SwapItem(InventoryLocation locationFrom,int formIndex,InventoryLocation locationTarget,int targetIndex)
		{
			var currentList = GetItemList(locationFrom);
			var targetList = GetItemList(locationTarget);
			
			InventoryItem currentItem = currentList[formIndex];
			if(targetIndex < targetList.Count)
			{
				InventoryItem targetItem = targetList[targetIndex];
				
				if(targetItem.itemID !=0 && currentItem.itemID != targetItem.itemID)	// ��������ͬ����Ʒ
				{
					currentList[formIndex] = targetItem;
					targetList[targetIndex] = currentItem;
				}else if(targetItem.itemID == currentItem.itemID)
				{
					targetItem.itemAmount += currentItem.itemAmount;
					targetList[targetIndex] = targetItem;
					currentList[formIndex] = new InventoryItem();
				}else
				{
					targetList[targetIndex] = currentItem;
					currentList[formIndex] = new InventoryItem();
				}
				
				EventHandler.CallUpdateInventoryUI(locationFrom,currentList);
				EventHandler.CallUpdateInventoryUI(locationTarget,targetList);
			}
		}
		
		/// <summary>
		/// ���ݱ��������ͷ�����Ӧ��List
		/// </summary>
		/// <param name="location"></param>
		/// <returns></returns>
		private List<InventoryItem> GetItemList(InventoryLocation location)
		{
			return location switch
			{
				InventoryLocation.Player => playerBag.inventoryItemList,
				InventoryLocation.Box => currentBag.inventoryItemList,
				_ => null
			};
		}

		/// <summary>
		/// �Ƴ�ָ�������ı�����Ʒ
		/// </summary>
		/// <param name="ID">��Ʒ��ID</param>
		/// <param name="removeAmount">Ҫ�ӵ�������</param>
		private void RemoveItem(int ID,int removeAmount)
		{
			var index = GetItemIndexAtBag(ID);

			if (playerBag.inventoryItemList[index].itemAmount > removeAmount)
			{
				var amount = playerBag.inventoryItemList[index].itemAmount - removeAmount;
				var newItem = new InventoryItem
				{
					itemID = ID,
					itemAmount = amount
				};
				playerBag.inventoryItemList[index] = newItem;

			}else if (playerBag.inventoryItemList[index].itemAmount == removeAmount)
			{
				var newItem = new InventoryItem();
				playerBag.inventoryItemList[index] = newItem;
			}

			EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.inventoryItemList);
		}
		
		/// <summary>
		/// ������Ʒ���׵ķ���
		/// </summary>
		/// <param name="itemDetails">��Ʒ����Ϣ</param>
		/// <param name="amount">����</param>
		/// <param name="isSellTrade">�Ƿ�����</param>
		public void TradeItem(ItemDetails itemDetails,int amount,bool isSellTrade)
		{
			// ���׵Ľ�Ǯ
			var cost = itemDetails.itemPrice * amount;
			int index = GetItemIndexAtBag(itemDetails.itemID);
			
			if(isSellTrade)		// ������Ʒ
			{
				if(playerBag.inventoryItemList[index].itemAmount >= amount)
				{
					RemoveItem(itemDetails.itemID,amount);
					 cost = (int)(cost * itemDetails.sellPercentage);
					 playermoney += cost;
				}
			}else if(playermoney - cost >= 0)				// ����Ʒ
			{
				if(ChenkBagCapacity())
				{
					AddItemAtIndex(itemDetails.itemID,index,amount);
				}
				playermoney -= cost;
			}
			
			EventHandler.CallUpdateInventoryUI(InventoryLocation.Player,playerBag.inventoryItemList);
		}
		
		/// <summary>
		/// �ж��ҽ���ͼֽ������Ҫ�Ĳ��ϵ���������ұ������Ƿ��㹻
		/// </summary>
		/// <param name="ID">����ͼֽ��ID</param>
		/// <returns></returns>
		public bool CheckStock(int ID)
		{
			var bluePrintDetails = bluePrintDataList_SO.GetBluePrintDetails(ID);
			
			foreach(var resourceItem in bluePrintDetails.resourceItem)
			{
				var itemStock = playerBag.GetInventoryItem(resourceItem.itemID);
				
				// �ж��ҽ���ͼֽ�е�ǰ����Ҫ�Ĳ��ϵ���������ұ����е������Ƿ��㹻		
				if(itemStock.itemAmount >= resourceItem.itemAmount)
				{
					continue;
				}else
				{
					return false;
				}
			}
			return true;
		}
		
		/// <summary>
		/// ���ݹؼ����ж��ֵ����Ƿ��и����ݲ�����
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public List<InventoryItem> GetBoxDataList(string key)
		{
			if(boxDataList.ContainsKey(key))
			{
				return boxDataList[key];
			}else
			{
				 return null;
			}
		}
		
		/// <summary>
		/// ��ȡ��ʼ��box�е�����
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public List<InventoryItem> GetStartBoxDataList(string key)
		{
			if(boxStartDataDict.ContainsKey(key))
			{
				return boxStartDataDict[key];
			}else
			{
				return null;
			}
		}
		
		/// <summary>
		/// ���ֵ�������������
		/// </summary>
		/// <param name="box"></param>
		public void AddBoxDataDict(Box box)
		{
			var key = box.name + box.index;
			if(!boxDataList.ContainsKey(key))
			{
				boxDataList.Add(key,box.boxBagData.inventoryItemList);
			}
		}
		
		/// <summary>
		/// ���ֵ�����ӳ�ʼ����������
		/// </summary>
		/// <param name="box"></param>
		public void AddBoxStartDataDict(Box box)
		{
			var key = box.name + box.index;
			if(!boxStartDataDict.ContainsKey(key))
			{
				boxStartDataDict.Add(key,box.boxBagData.inventoryItemList);
			}
		}

		/// <summary>
		/// �洢�ҵı������ݺ����ӵ����ݣ�������ҵĽ�Ǯ����
		/// </summary>
		/// <returns></returns>
		public GameSaveData gameSaveData()
		{
			GameSaveData saveData = new GameSaveData();
			saveData.playerMoney = this.playermoney;
			
			saveData.inventoryDict = new Dictionary<string, List<InventoryItem>>();
			saveData.startBoxDict = new Dictionary<string, List<InventoryItem>>();
			saveData.inventoryDict.Add(playerBag.name,playerBag.inventoryItemList);
			
			foreach(var item in boxDataList)
			{
				saveData.inventoryDict.Add(item.Key,item.Value);
			}
			
			foreach(var item in boxStartDataDict)
			{
				saveData.startBoxDict.Add(item.Key,item.Value);
			}
			
			return saveData;
		}

		/// <summary>
		/// ���»�ȡ�ҵı��������Ӻ���ҽ�Ǯ������
		/// </summary>
		/// <param name="saveData"></param>
		public void RestoreData(GameSaveData saveData)
		{
			this.playermoney = saveData.playerMoney;
			
			
			// ����Ҫ��ֵ����Ϊ���»�ȡ��ʱ��playerBag��Ϊ��
			
			playerBag = Instantiate(playerBagTemp);
			//Debug.Log(playerBag.name);
			playerBag.inventoryItemList = saveData.inventoryDict[playerBag.name];
			
			foreach(var item in saveData.inventoryDict)
			{
				if(boxDataList.ContainsKey(item.Key))
				{
					boxDataList[item.Key] = item.Value;
				}
			}
			
			foreach(var item in saveData.startBoxDict)
			{
				if(boxStartDataDict.ContainsKey(item.Key))
				{
					boxStartDataDict[item.Key] = item.Value;
				}else
				{
					boxStartDataDict.Add(item.Key,item.Value);
				}
			}
			//Debug.Log(boxStartDataDict.ContainsKey("Box0"));
			
			EventHandler.CallUpdateInventoryUI(InventoryLocation.Player,playerBag.inventoryItemList);
		}
	}
}
