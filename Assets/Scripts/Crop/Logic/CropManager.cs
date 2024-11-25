using System.Collections;
using System.Collections.Generic;
using Fram.Inventory;
using Unity.Mathematics;
using UnityEngine;

namespace Fram.CropPlate
{
	public class CropManager : Singleton<CropManager>
	{
		public CropDataList_SO cropData;
		//public IventoryBag_SO playerBag;
		private Transform cropParent;
		private Grid currentGrid;
		private Season currentSeason;
		void OnEnable()
		{
			EventHandler.PlantSeedEvent += OnPlantSeedEvent;
			EventHandler.AfterSceneUnloadEvent += OnAfterSceneUnloadEvent;
			EventHandler.GameDayEvent += OnGameDayEvent;
		}


		void OnDisable()
		{
			EventHandler.PlantSeedEvent -= OnPlantSeedEvent;
			EventHandler.AfterSceneUnloadEvent -= OnAfterSceneUnloadEvent;
			EventHandler.GameDayEvent -= OnGameDayEvent;
		}
		
		private void OnAfterSceneUnloadEvent()
		{
			cropParent = GameObject.FindWithTag("CropParent").transform;
			currentGrid = FindObjectOfType<Grid>();
		}
		
		private void OnGameDayEvent(int gameDay, Season gameSeason)
		{
			currentSeason = gameSeason;
		}
		
		private void OnPlantSeedEvent(int itemID, TileDetails tileDetails)
		{
			CropDetails currentCrop = GetCropDetails(itemID);
			
			// 如果我的种子非空并且当前季节是可以耕种的话并且当前的瓦片中没有种子的话
			if(currentCrop!=null && SeasonAvailable(currentCrop) && tileDetails.seedItemID==-1)// 首次播种
			{
				tileDetails.growthDays = 0;
				tileDetails.seedItemID = itemID;
				// 显示农作物
				DisPlayCropPlant(tileDetails,currentCrop);
			}else if(tileDetails.seedItemID != -1)
			{
				DisPlayCropPlant(tileDetails,currentCrop);
			}
		}
		
		private void DisPlayCropPlant(TileDetails tileDetails,CropDetails cropDetails)
		{
			// 成长阶段
			int growthStages = cropDetails.growthDays.Length;
			int currentStage = 0;
			int dayCounter = cropDetails.TotalGrowthDays;
			
			for(int i = growthStages - 1; i >= 0; i--)
			{
				if(tileDetails.growthDays >= dayCounter)
				{
					currentStage = i;
					break;
				}
				dayCounter -= cropDetails.growthDays[i];
			}
			
			GameObject cropPrefab = cropDetails.growthPreafabs[currentStage];
			Sprite cropSprite = cropDetails.growthSprites[currentStage];
			
			Vector3 pos = new Vector3(tileDetails.gridX + 0.5f,tileDetails.gridY + 0.5f,0);
			
			GameObject cropInstance = Instantiate(cropPrefab,pos,quaternion.identity,cropParent);
			cropInstance.GetComponentInChildren<SpriteRenderer>().sprite = cropSprite;
			cropInstance.GetComponent<Crop>().cropDetails = cropDetails;
			cropInstance.GetComponent<Crop>().currentTile = tileDetails;
		}
		
		/// <summary>
		/// 返回我种子的信息
		/// </summary>
		/// <param name="itemID"></param>
		public CropDetails GetCropDetails(int itemID)
		{
			return cropData.cropDetailsList.Find(c=>c.seedItemID == itemID);
		}
		
		/// <summary>
		/// 返回当前季节是否可以耕种种子
		/// </summary>
		/// <param name="crop"></param>
		/// <returns></returns>
		private bool SeasonAvailable(CropDetails crop)
		{
			for(int i =0;i<crop.seasons.Length;i++)
			{
				if(crop.seasons[i] == currentSeason)
					return true;
			}
			return false;
		}
	}
}

