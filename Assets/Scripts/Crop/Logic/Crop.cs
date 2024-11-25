using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crop : MonoBehaviour
{
	// Start is called before the first frame update
	public CropDetails cropDetails;
	private int harvestActionCount;
	public TileDetails currentTile;
	private Animator anim;
	public bool canHarvest => currentTile.growthDays >= cropDetails.TotalGrowthDays;
	private Transform playerTransfrom => FindObjectOfType<Player>().transform;
	public void ProcessToolAction(ItemDetails tool,TileDetails tileDetails)
	{
		currentTile = tileDetails;	
		int requireActionCount = cropDetails.GetTotalRequireCount(tool.itemID);// 工具使用次数
		if(requireActionCount == -1) return;
		
		anim = GetComponentInChildren<Animator>();;
		
		// 点击计数器
		if(harvestActionCount < requireActionCount)
		{
			harvestActionCount++;
			
			// 判断是否有动画 树木
			if(anim!=null&&cropDetails.hasAnimation)
			{
				if(playerTransfrom.position.x < transform.position.x)
				{
					anim.SetTrigger("RotateRight");
				}else
				{
					anim.SetTrigger("RotateLeft");
				}
			}
			// 播放粒子特效
			if(cropDetails.hasParticleEffect)
			{
				EventHandler.CallParticleEffectEvent(cropDetails.particleEffectType,transform.position + cropDetails.effectPos);
			}
			// 播放声音
			if(cropDetails.soundEffect != SoundName.none)
			{
				EventHandler.CallInitSoundEffect(AudioManager.Instance.soundDetailsData.GetSoundDetails(cropDetails.soundEffect));
			}
		}
		
		if(harvestActionCount >= requireActionCount)
		{
			if(cropDetails.generateAtPlayerPosition || !cropDetails.hasAnimation)
			{
				// 生成农作物
				SpawnHarvestItem();
			}else if(cropDetails.hasAnimation)
			{
				if(playerTransfrom.position.x < transform.position.x)
				{
					anim.SetTrigger("FallingRight");
				}else
				{
					anim.SetTrigger("FallingLeft");
				}
				
				EventHandler.CallPlaySoundEvent(SoundName.TreeFalling);
				StartCoroutine(HarvestAfterAnimation());
			}
		}
		
	}
	
	private IEnumerator HarvestAfterAnimation()
	{
		while(!anim.GetCurrentAnimatorStateInfo(0).IsName("End"))
		{
			yield return null;
		}
		
		SpawnHarvestItem();
		
		// 转换新物体
		if(cropDetails.transferItemID > 0)
		{
			CreateTransferCrop();
		}
	}
	
	/// <summary>
	/// 生成转换的物品
	/// </summary>
	private void CreateTransferCrop()
	{
		currentTile.seedItemID = cropDetails.transferItemID;
		currentTile.daysSinceLastHarvest = -1;
		currentTile.growthDays = 0;
		
		EventHandler.CallRefreshCurrentMap();
	}
	
	/// <summary>
	/// 生成农作物
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
					// TODO
					// 在世界坐标生成物品
					var dirx = transform.position.x > playerTransfrom.position.x?1:-1;
					// 一定范围内的随机
					var spawnPos = new Vector3(transform.position.x + Random.Range(dirx,cropDetails.spawnRadius.x * dirx),
					transform.position.y + Random.Range(-cropDetails.spawnRadius.y,cropDetails.spawnRadius.y),0);
					
					EventHandler.CallInstantiateItemInScene(cropDetails.producedItemID[i],spawnPos);
				}
			}
		}
		
		// 收割完之后，要将该瓦片信息上的收割信息进行更新
		if(currentTile!=null)
		{
			currentTile.daysSinceLastHarvest++;
			
			// 果实可以重复生长
			if(cropDetails.daysToRegrow>0&&currentTile.daysSinceLastHarvest<=cropDetails.reGrowTime)
			{
				// 如果果实可以重复生长的话，那么我当前瓦片的生长的日期要进行倒退，倒退到成熟果实还在生长的阶段，但是那个阶段由cropDetails.daysToRegrow决定了
				currentTile.growthDays = cropDetails.TotalGrowthDays - cropDetails.daysToRegrow;
				// 刷新种子
				EventHandler.CallRefreshCurrentMap();
			}else
			{
				// 不可重复生长 --- 这里是传指针，所以这里修改也是可以的
				currentTile.daysSinceLastHarvest = -1;
				currentTile.seedItemID = -1;
			}
			
			Destroy(gameObject);
		}
	}
}
