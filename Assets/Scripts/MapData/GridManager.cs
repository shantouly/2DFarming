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
		[Header("�ֵ���Ƭ�л���Ϣ")]
		public RuleTile digTile;
		public RuleTile waterTile;
		private Tilemap digTileMap;
		private Tilemap waterTileMap;
		private Season currentSeason;

		[Header("��ͼ��Ϣ")]
		public List<MapData_SO> mapDataList;

		// ���ֵ��Ǵ洢ÿһ����Ƭ����Ϣ(TileDetails)
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
		/// ��ʼ���ֵ��е���Ƭ��Ϣ
		/// </summary>
		/// <param name="mapData">��ͼ����Ϣ</param>
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
		/// ��ȡ�ֵ��е���Ƭ��Ϣ
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
		/// ÿ��ִ��һ��
		/// </summary>
		/// <param name="gameDay">��ǰ������</param>
		/// <param name="gameSeason">��ǰ�ļ���</param>
		private void OnGameDayEvent(int gameDay, Season gameSeason)
		{
			currentSeason = gameSeason;
			
			foreach(var tile in tileDetailsDict)
			{
				// ��ˮֻ����һ�죬������콽���ˣ���ô�ڶ���ͻָ�
				if(tile.Value.daysSinceWatered > -1)
				{
					tile.Value.daysSinceWatered = -1;
				}
				if(tile.Value.daysSinceDug > -1)
				{
					tile.Value.daysSinceDug++;
				}
				
				// �����ʱ����Ƭ�Ѿ����˳���5�첢�Ҹ���Ƭ��û�����ӵ�ID�Ļ�����ô�ָ�
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
			
			// ���»���
			RefreshMap();
		}

		/// <summary>
		/// ��ȡ�ҵ�ǰ����µ������е���Ƭ��Ϣ
		/// </summary>
		/// <param name="mousePos">��������</param>
		/// <returns></returns>
		public TileDetails GetTileDetailsOnMousePosition(Vector3Int mouseGridPos)
		{
			string key = mouseGridPos.x + "x" + mouseGridPos.y + "y" + SceneManager.GetActiveScene().name;
			return GetTileDetails(key);
		}

		/// <summary>
		/// ����ִ��֮��ķ����������ӵ���Ʒ�������ͼ��ִ�еĹ���
		/// </summary>
		/// <param name="position">�������λ��</param>
		/// <param name="details">����Ʒ��xinxi</param>
		/// <exception cref="System.NotImplementedException"></exception>
		private void OnExecuteActionAfterAnimation(Vector3 mousePositionWorld, ItemDetails itemDetails)
		{
			var mouseGridPos = currentGrid.WorldToCell(mousePositionWorld);
			var currentTile = GetTileDetailsOnMousePosition(mouseGridPos);

			if(currentTile != null)
			{
				Crop currentCrop = GetCropObject(mousePositionWorld);
				//TODO ��Ʒʹ��ʵ�ʵĹ���
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
							// ִ���ո��,��Щ���������ջ�ķ���ϣ������Crop����ű��б�д
							currentCrop.ProcessToolAction(itemDetails,currentCrop.currentTile);
						}
						break;
					case itemType.CollectTool:
						if(currentCrop!=null)
						{
							//Debug.Log(currentTile.gridX + "..." + currentTile.gridY + "..." + currentCrop.currentTile.gridX+"..." + currentCrop.currentTile.gridY);
							// ִ���ո��,��Щ���������ջ�ķ���ϣ������Crop����ű��б�д
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
						// �ڵ�ͼ��������Ʒ			-- itemManager
						// �Ƴ���ǰ��Ʒ(ͼֽ)		-- InventoryManager
						// �Ƴ���Դ��Ʒ			   	-- InventoryManager
						EventHandler.CallBuildFurnitureEvent(itemDetails.itemID,mousePositionWorld);
						break;
				}
				UpdateTileDetails(currentTile);
			}
		}
		
		/// <summary>
		/// ��ȡ��Crop�ű��Ķ���
	/// </summary>
		/// <param name="mouseWorldPos"></param>
		/// <returns></returns>
		public Crop GetCropObject(Vector3 mouseWorldPos)
		{
			Collider2D[] colliders = Physics2D.OverlapPointAll(mouseWorldPos);
			
			Crop currentCrop = null;
			foreach(var collider in colliders)
			{
				// ����ĵط������ӳ���ĵط�
				if(collider.GetComponent<Crop>())
				{
					currentCrop = collider.GetComponent<Crop>();
				}
			}
			
			return currentCrop;
		}

		/// <summary>
		/// �ж���ʮ�ָ�ķ�Χ֮����û��ReapableItem
		/// </summary>
		/// <returns></returns>
		public bool HaveReapableItemsInRadius(Vector3 mouseWorldPosition,ItemDetails tool)
		{
			itemInRadius = new List<ReapItem>();
			
			Collider2D[] colliders = new Collider2D[20];
			
			// Բ�η�Χ���Ƿ���collider,�����һ������Ϊ�����������꣬������Input.mousePosition���������Ļ�����ˣ�
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
		/// �����ֵص���Ƭ��Ϣ
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
		/// ���ƽ�ˮ����Ƭ��Ϣ
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
		/// �����ֵ��е���Ϣ
		/// </summary>
		/// <param name="tileDetails">Ҫ���µ���Ƭ����Ϣ</param>
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
		/// ���»�����Ƭ��ͼ��Ϣ
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
		/// ���ڳ���ת��֮��������ֵػ��߽�ˮ����Ƭ��Ϣ
		/// </summary>
		/// <param name="sceneName"></param>
		private void DisPlayMap(string sceneName)
		{
			foreach(var tile in tileDetailsDict)
			{
				var key = tile.Key;
				var tileDetails = tile.Value;
				
				// �����Ǹó����е���Ƭ
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
					
					// ����
					if(tileDetails.seedItemID > -1)
					{
						EventHandler.CallPlantSeedEvent(tileDetails.seedItemID,tileDetails);
					}
				}
			}
		}
		
		/// <summary>
		/// ���ݳ������ֹ�������Χ�������Χ��ԭ��
		/// </summary>
		/// <param name="sceneName">��������</param>
		/// <param name="gridDimensions">����������Ĵ�С</param>
		/// <param name="gridOrigin">������ԭ��Ĵ�С</param>
		/// <returns>�Ƿ��е�ǰ��������Ϣ</returns>
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

