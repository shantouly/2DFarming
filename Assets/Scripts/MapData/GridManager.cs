using System.Collections;
using System.Collections.Generic;
using Fram.CropPlate;
using Fram.Save;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Fram.Map
{
	public class GridManager : Singleton<GridManager>,ISaveable
	{
		[Header("种地瓦片切换信息")]
		public RuleTile digTile;
		public RuleTile waterTile;
		private Tilemap digTileMap;
		private Tilemap waterTileMap;
		private Season currentSeason;

		[Header("地图信息")]
		public List<MapData_SO> mapDataList;

		// 该字典是存储每一个瓦片的信息(TileDetails)
		private Dictionary<string, TileDetails> tileDetailsDict = new Dictionary<string, TileDetails>();
		private Dictionary<string,bool> firstLoadDict = new Dictionary<string, bool>();
		private Grid currentGrid;
		private List<ReapItem> itemInRadius;

		public string GUID => GetComponent<DataGUID>().guid;

		private void OnEnable()
		{
			EventHandler.ExecuteActionAfterAnimation += OnExecuteActionAfterAnimation;
			EventHandler.AfterSceneUnloadEvent += OnAfterSceneUnloadEvent;
			EventHandler.GameDayEvent += OnGameDayEvent;
			EventHandler.RefreshCurrentMap +=  RefreshMap;
		}

		private void Start()
		{			
			ISaveable saveable = this;
			saveable.RegisterSaveable();
			
			foreach (MapData_SO mapData in mapDataList)
			{
				firstLoadDict.Add(mapData.sceneName,true);
				InitTileDetailsDict(mapData);
			}
		}

		private void OnDisable()
		{
			EventHandler.ExecuteActionAfterAnimation -= OnExecuteActionAfterAnimation;
			EventHandler.AfterSceneUnloadEvent -= OnAfterSceneUnloadEvent;
			EventHandler.GameDayEvent -= OnGameDayEvent;
			EventHandler.RefreshCurrentMap -= RefreshMap;
		}

		/// <summary>
		/// 初始化字典中的瓦片信息
		/// </summary>
		/// <param name="mapData">地图的信息</param>
		private void InitTileDetailsDict(MapData_SO mapData)
		{
			foreach (TileProperty tileProperty in mapData.tileProperties)
			{
				TileDetails tileDetails = new TileDetails
				{
					gridX = tileProperty.tileCordinate.x,
					gridY = tileProperty.tileCordinate.y
				};

				string key = tileDetails.gridX + "x" + tileDetails.gridY + "y" + mapData.sceneName;

				if (GetTileDetails(key) != null)
				{
					tileDetails = tileDetailsDict[key];
				}
				switch (tileProperty.gridType)
				{
					case GridType.Diggable:
						tileDetails.canDig = tileProperty.boolTypeValue;
						break;
					case GridType.DropItem:
						tileDetails.canDropItem = tileProperty.boolTypeValue;
						break;
					case GridType.PlaceFurniture:
						tileDetails.canPlaceFurniture = tileProperty.boolTypeValue;
						break;
					case GridType.NPCObstacle:
						tileDetails.isNPCObstacle = tileProperty.boolTypeValue;
						break;
				}

				if (GetTileDetails(key) != null)
				{
					tileDetailsDict[key] = tileDetails;
				}
				else
				{
					tileDetailsDict.Add(key, tileDetails);
				}
			}
		}

		/// <summary>
		/// 获取字典中的瓦片信息
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public TileDetails GetTileDetails(string key)
		{
			if (tileDetailsDict.ContainsKey(key))
			{
				return tileDetailsDict[key];
			}
			else
			{
				return null;
			}
		}

		private void OnAfterSceneUnloadEvent()
		{
			currentGrid = FindObjectOfType<Grid>();
			digTileMap = GameObject.FindWithTag("Dig").GetComponent<Tilemap>();
			waterTileMap = GameObject.FindWithTag("Water").GetComponent<Tilemap>();
			
			if(firstLoadDict[SceneManager.GetActiveScene().name] == true)
			{
				EventHandler.CallGenerateCropEvent();
				firstLoadDict[SceneManager.GetActiveScene().name] = false;
			}
				
			RefreshMap();
		}
		
		/// <summary>
		/// 每天执行一次
		/// </summary>
		/// <param name="gameDay">当前的天数</param>
		/// <param name="gameSeason">当前的季节</param>
		private void OnGameDayEvent(int gameDay, Season gameSeason)
		{
			currentSeason = gameSeason;
			
			foreach(var tile in tileDetailsDict)
			{
				// 浇水只存在一天，如果昨天浇过了，那么第二天就恢复
				if(tile.Value.daysSinceWatered > -1)
				{
					tile.Value.daysSinceWatered = -1;
				}
				if(tile.Value.daysSinceDug > -1)
				{
					tile.Value.daysSinceDug++;
				}
				
				// 如果此时该瓦片已经挖了超过5天并且该瓦片上没有种子的ID的话，那么恢复
				if(tile.Value.daysSinceDug>5&&tile.Value.seedItemID==-1)
				{
					tile.Value.daysSinceDug = -1;
					tile.Value.canDig = true;
					tile.Value.growthDays = -1;
				}
				
				if(tile.Value.seedItemID!=-1)
				{
					tile.Value.growthDays++;
				}
			}
			
			// 重新绘制
			RefreshMap();
		}

		/// <summary>
		/// 获取我当前鼠标下的网格中的瓦片信息
		/// </summary>
		/// <param name="mousePos">鼠标的坐标</param>
		/// <returns></returns>
		public TileDetails GetTileDetailsOnMousePosition(Vector3Int mouseGridPos)
		{
			string key = mouseGridPos.x + "x" + mouseGridPos.y + "y" + SceneManager.GetActiveScene().name;
			return GetTileDetails(key);
		}

		/// <summary>
		/// 动画执行之后的方法（例如扔掉物品）世界地图上执行的功能
		/// </summary>
		/// <param name="position">鼠标点击的位置</param>
		/// <param name="details">该物品的xinxi</param>
		/// <exception cref="System.NotImplementedException"></exception>
		private void OnExecuteActionAfterAnimation(Vector3 mousePositionWorld, ItemDetails itemDetails)
		{
			var mouseGridPos = currentGrid.WorldToCell(mousePositionWorld);
			var currentTile = GetTileDetailsOnMousePosition(mouseGridPos);

			if(currentTile != null)
			{
				Crop currentCrop = GetCropObject(mousePositionWorld);
				//TODO 物品使用实际的功能
				switch (itemDetails.itemtype)
				{
					case itemType.Seed:
						EventHandler.CallPlantSeedEvent(itemDetails.itemID,currentTile);
						EventHandler.CallDropItemEvent(itemDetails.itemID,mousePositionWorld,itemDetails.itemtype);
						EventHandler.CallPlaySoundEvent(SoundName.Plant);
						break;
					case itemType.Commodity:
						EventHandler.CallDropItemEvent(itemDetails.itemID, mousePositionWorld,itemDetails.itemtype);
						break;
					case itemType.HoeTool:
						SetDigGround(currentTile);
						currentTile.daysSinceDug = 0;
						currentTile.canDig = false;
						currentTile.canDropItem = false;
						EventHandler.CallPlaySoundEvent(SoundName.Hoe);
						break;
					case itemType.WaterTool:
						SetWaterGround(currentTile);
						currentTile.daysSinceWatered = 0;
						EventHandler.CallPlaySoundEvent(SoundName.Water);
						break;
					case itemType.BreakTool:
					case itemType.ChopTool:
						//Crop currentCrop = GetCropObject(mousePositionWorld);
						if(currentCrop!=null)
						{
							// Debug.Log(currentTile.gridX + "..." + currentTile.gridY + "..." + currentCrop.currentTile.gridX+"..." + currentCrop.currentTile.gridY);
							// 执行收割方法,这些关于种子收获的方法希望放在Crop这个脚本中编写
							currentCrop.ProcessToolAction(itemDetails,currentCrop.currentTile);
						}
						break;
					case itemType.CollectTool:
						if(currentCrop!=null)
						{
							//Debug.Log(currentTile.gridX + "..." + currentTile.gridY + "..." + currentCrop.currentTile.gridX+"..." + currentCrop.currentTile.gridY);
							// 执行收割方法,这些关于种子收获的方法希望放在Crop这个脚本中编写
							currentCrop.ProcessToolAction(itemDetails,currentTile);
						}
						break;
					case itemType.ReapTool:
						var reapCount = 0;
						for(int i =0;i<itemInRadius.Count;i++)
						{
							EventHandler.CallParticleEffectEvent(ParticleEffectType.ReapableScenery,itemInRadius[i].transform.position + Vector3.up);
							itemInRadius[i].SpawnHarvestItem();
							Destroy(itemInRadius[i].gameObject);
							reapCount++;
							
							if(reapCount == Settings.reapCount)
								break;
						}
						
						EventHandler.CallPlaySoundEvent(SoundName.Reap);
						break;
					case itemType.Furniture:
						// 在地图上生成物品			-- itemManager
						// 移除当前物品(图纸)		-- InventoryManager
						// 移除资源物品			   	-- InventoryManager
						EventHandler.CallBuildFurnitureEvent(itemDetails.itemID,mousePositionWorld);
						break;
				}
				UpdateTileDetails(currentTile);
			}
		}
		
		/// <summary>
		/// 获取含Crop脚本的对象
	/// </summary>
		/// <param name="mouseWorldPos"></param>
		/// <returns></returns>
		public Crop GetCropObject(Vector3 mouseWorldPos)
		{
			Collider2D[] colliders = Physics2D.OverlapPointAll(mouseWorldPos);
			
			Crop currentCrop = null;
			foreach(var collider in colliders)
			{
				// 点击的地方是种子成熟的地方
				if(collider.GetComponent<Crop>())
				{
					currentCrop = collider.GetComponent<Crop>();
				}
			}
			
			return currentCrop;
		}

		/// <summary>
		/// 判断我十字镐的范围之内有没有ReapableItem
		/// </summary>
		/// <returns></returns>
		public bool HaveReapableItemsInRadius(Vector3 mouseWorldPosition,ItemDetails tool)
		{
			itemInRadius = new List<ReapItem>();
			
			Collider2D[] colliders = new Collider2D[20];
			
			// 圆形范围内是否有collider,这里第一个参数为鼠标的世界坐标，不能是Input.mousePosition（这个是屏幕坐标了）
			Physics2D.OverlapCircleNonAlloc(mouseWorldPosition, tool.itemUseRadius, colliders);
			
			if(colliders.Length > 0)
			{
				for(int i = 0;i < colliders.Length;i++)
				{
					if(colliders[i] != null)
					{
						if (colliders[i].GetComponent<ReapItem>())
						{
							var item = colliders[i].GetComponent<ReapItem>();
							Debug.Log(item.name);
							itemInRadius.Add(item);
						}
					}
				}
			}
			
			Debug.Log(itemInRadius.Count);
			return itemInRadius.Count > 0;
		}

		/// <summary>
		/// 绘制种地的瓦片信息
		/// </summary>
		/// <param name="tile"></param>
		private void SetDigGround(TileDetails tile)
		{
			Vector3Int pos = new Vector3Int(tile.gridX, tile.gridY, 0);
			if (digTileMap != null)
			{
				digTileMap.SetTile(pos, digTile);
			}
		}

		/// <summary>
		/// 绘制浇水的瓦片信息
		/// </summary>
		/// <param name="tile"></param>
		private void SetWaterGround(TileDetails tile)
		{
			Vector3Int pos = new Vector3Int(tile.gridX, tile.gridY, 0);
			if (waterTileMap != null)
			{
				waterTileMap.SetTile(pos, waterTile);
			}
		}
		
		/// <summary>
		/// 更新字典中的信息
		/// </summary>
		/// <param name="tileDetails">要更新的瓦片的信息</param>
		public void UpdateTileDetails(TileDetails tileDetails)
		{
			string key = tileDetails.gridX +"x" + tileDetails.gridY+"y" + SceneManager.GetActiveScene().name;
			if(tileDetailsDict.ContainsKey(key))
			{
				tileDetailsDict[key] = tileDetails;
			}else
			{
				tileDetailsDict.Add(key,tileDetails);
			}
		}
		
		/// <summary>
		/// 重新绘制瓦片地图信息
		/// </summary>
		private void RefreshMap()
		{
			if(digTileMap!=null)
			{
				digTileMap.ClearAllTiles();
			}
			if(waterTileMap!=null)
			{
				waterTileMap.ClearAllTiles();
			}
			
			foreach(var crop in FindObjectsOfType<Crop>())
			{
				Destroy(crop.gameObject);
			}
			
			DisPlayMap(SceneManager.GetActiveScene().name);
		}
		
		/// <summary>
		/// 用于场景转换之后更新我种地或者浇水的瓦片信息
		/// </summary>
		/// <param name="sceneName"></param>
		private void DisPlayMap(string sceneName)
		{
			foreach(var tile in tileDetailsDict)
			{
				var key = tile.Key;
				var tileDetails = tile.Value;
				
				// 表明是该场景中的瓦片
				if(key.Contains(sceneName))
				{
					if(tileDetails.daysSinceDug > -1)
					{
						SetDigGround(tileDetails);
					}
					if(tileDetails.daysSinceWatered > -1)
					{
						SetWaterGround(tileDetails);
					}
					
					// 种子
					if(tileDetails.seedItemID > -1)
					{
						EventHandler.CallPlantSeedEvent(tileDetails.seedItemID,tileDetails);
					}
				}
			}
		}
		
		/// <summary>
		/// 根据场景名字构建网格范围，输出范围和原点
		/// </summary>
		/// <param name="sceneName">场景名字</param>
		/// <param name="gridDimensions">场景中网格的大小</param>
		/// <param name="gridOrigin">场景中原点的大小</param>
		/// <returns>是否有当前场景的信息</returns>
		public bool GetGridDimensions(string sceneName,out Vector2Int gridDimensions,out Vector2Int gridOrigin)
		{
			gridDimensions = Vector2Int.zero;
			gridOrigin = Vector2Int.zero;
			
			foreach(var mapData in mapDataList)
			{
				if(mapData.sceneName == sceneName)
				{
					gridDimensions.x = mapData.gridWidth;
					gridDimensions.y = mapData.gridWidth;
					
					gridOrigin.x = mapData.originX;
					gridOrigin.y = mapData.originY;
					
					return true;
				}
			}
			
			return false;
		}

		public GameSaveData gameSaveData()
		{
			GameSaveData saveData = new GameSaveData();
			saveData.tileDetailsDict = this.tileDetailsDict;
			saveData.firstLoadDict = this.firstLoadDict;
			
			return saveData;
		}

		public void RestoreData(GameSaveData saveData)
		{
			this.tileDetailsDict = saveData.tileDetailsDict;
			this.firstLoadDict = saveData.firstLoadDict;
		}
	}
}

