using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fram.CropPlate
{
	public class ReapItem : MonoBehaviour
	{
		private CropDetails cropDetails;
		private Transform playerTransfrom => FindObjectOfType<Player>().transform;
		
		public void InitCropData(int ID)
		{
			cropDetails = CropManager.Instance.GetCropDetails(ID);
		}
		
		/// <summary>
		/// 生成产品
		/// </summary>
		public void SpawnHarvestItem()
		{
			for(int i =0;i<cropDetails.producedItemID.Length;i++)
			{
				int amountToProduce;
				
				if(cropDetails.producedMinAmount[i] == cropDetails.producedMaxAmount[i])
				{
					// 只生成指定数量
					amountToProduce = cropDetails.producedMinAmount[i];
				}else
				{
					// 生成物品随机数量
					amountToProduce = Random.Range(cropDetails.producedMinAmount[i],cropDetails.producedMaxAmount[i]+1);
				}
				
				for(int j = 0;j<amountToProduce;j++)
				{
					if(cropDetails.generateAtPlayerPosition)
					{
						EventHandler.CallHarvestAtPlayerPosition(cropDetails.producedItemID[i]);
					}else
					{
						// 在世界坐标生成物品
						var dirx = transform.position.x > playerTransfrom.position.x?1:-1;
						// 一定范围内的随机
						var spawnPos = new Vector3(transform.position.x + Random.Range(dirx,cropDetails.spawnRadius.x * dirx),
						transform.position.y + Random.Range(-cropDetails.spawnRadius.y,cropDetails.spawnRadius.y),0);
						
						EventHandler.CallInstantiateItemInScene(cropDetails.producedItemID[i],spawnPos);
					}
				}
			}
		}
	}
}

