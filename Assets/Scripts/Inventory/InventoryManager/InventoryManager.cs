using System.Collections;
using System.Collections.Generic;
using Fram.Save;
using UnityEngine;

namespace Fram.Inventory
{
	public class InventoryManager : Singleton<InventoryManager>,ISaveable
	{
		[Header("物品数据")]
		public ItemDataList_SO itemDataList_SO;
		[Header("建造图纸")]
		public BluePrintDataList_SO bluePrintDataList_SO;
		[Header("背包数据")]
		public IventoryBag_SO playerBagTemp;
		public IventoryBag_SO playerBag;
		public IventoryBag_SO currentBag;
		[Header("交易")]
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
		/// 开始一个新游戏需要做的事情
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
		/// 添加物品到背包中
		/// </summary>
		/// <param name="item">地面上的物品挂载的脚本</param>
		/// <param name="CanDestory">拾取之后是否可以销毁</param>
		public void AddItem(Item item,bool CanDestory)
		{
			var index = GetItemIndexAtBag(item.itemID);

			AddItemAtIndex(item.itemID, index, 1);
			if (CanDestory)
			{
				Destroy(item.gameObject);
			}

			// 更新UI
			EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.inventoryItemList);
		}

		/// <summary>
		/// 检查背包的容量是否还有空的
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
		/// 如果背包中已有该物品，返回该物品在背包里面的index
		/// </summary>
		/// <param name="ID">物品ID</param>
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
		/// 将物品添加到背包中
		/// </summary>
		/// <param name="ID">物品的ID</param>
		/// <param name="index">物品在背包里面的index,如果背包中有的话</param>
		/// <param name="amount">添加物品的数量</param>
		private void AddItemAtIndex(int ID,int index,int amount)
		{
			if(index == -1 && ChenkBagCapacity())         // 背包中没有该物品并且背包中有空余容量的时候
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
			}                       // 背包中有该物品的时候
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
				
				if(targetItem.itemID !=0 && currentItem.itemID != targetItem.itemID)	// 两个不相同的物品
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
		/// 根据背包的类型返回相应的List
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
		/// 移除指定数量的背包物品
		/// </summary>
		/// <param name="ID">物品的ID</param>
		/// <param name="removeAmount">要扔掉的数量</param>
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
		/// 进行物品交易的方法
		/// </summary>
		/// <param name="itemDetails">物品的信息</param>
		/// <param name="amount">数量</param>
		/// <param name="isSellTrade">是否是卖</param>
		public void TradeItem(ItemDetails itemDetails,int amount,bool isSellTrade)
		{
			// 交易的金钱
			var cost = itemDetails.itemPrice * amount;
			int index = GetItemIndexAtBag(itemDetails.itemID);
			
			if(isSellTrade)		// 卖出物品
			{
				if(playerBag.inventoryItemList[index].itemAmount >= amount)
				{
					RemoveItem(itemDetails.itemID,amount);
					 cost = (int)(cost * itemDetails.sellPercentage);
					 playermoney += cost;
				}
			}else if(playermoney - cost >= 0)				// 卖物品
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
		/// 判断我建造图纸中所需要的材料的数量在玩家背包中是否足够
		/// </summary>
		/// <param name="ID">建造图纸的ID</param>
		/// <returns></returns>
		public bool CheckStock(int ID)
		{
			var bluePrintDetails = bluePrintDataList_SO.GetBluePrintDetails(ID);
			
			foreach(var resourceItem in bluePrintDetails.resourceItem)
			{
				var itemStock = playerBag.GetInventoryItem(resourceItem.itemID);
				
				// 判断我建造图纸中当前所需要的材料的数量在玩家背包中的数量是否足够		
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
		/// 根据关键字判断字典中是否有该数据并返回
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
		/// 获取初始的box中的数据
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
		/// 向字典中添加这个数据
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
		/// 向字典中添加初始的箱子数据
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
		/// 存储我的背包数据和箱子的数据，还有玩家的金钱数量
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
		/// 重新获取我的背包，箱子和玩家金钱的数据
		/// </summary>
		/// <param name="saveData"></param>
		public void RestoreData(GameSaveData saveData)
		{
			this.playermoney = saveData.playerMoney;
			
			
			// 这里要赋值是因为重新获取的时候playerBag是为空
			
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
