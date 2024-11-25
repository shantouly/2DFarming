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
		int requireActionCount = cropDetails.GetTotalRequireCount(tool.itemID);// ����ʹ�ô���
		if(requireActionCount == -1) return;
		
		anim = GetComponentInChildren<Animator>();;
		
		// ���������
		if(harvestActionCount < requireActionCount)
		{
			harvestActionCount++;
			
			// �ж��Ƿ��ж��� ��ľ
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
			// ����������Ч
			if(cropDetails.hasParticleEffect)
			{
				EventHandler.CallParticleEffectEvent(cropDetails.particleEffectType,transform.position + cropDetails.effectPos);
			}
			// ��������
			if(cropDetails.soundEffect != SoundName.none)
			{
				EventHandler.CallInitSoundEffect(AudioManager.Instance.soundDetailsData.GetSoundDetails(cropDetails.soundEffect));
			}
		}
		
		if(harvestActionCount >= requireActionCount)
		{
			if(cropDetails.generateAtPlayerPosition || !cropDetails.hasAnimation)
			{
				// ����ũ����
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
		
		// ת��������
		if(cropDetails.transferItemID > 0)
		{
			CreateTransferCrop();
		}
	}
	
	/// <summary>
	/// ����ת������Ʒ
	/// </summary>
	private void CreateTransferCrop()
	{
		currentTile.seedItemID = cropDetails.transferItemID;
		currentTile.daysSinceLastHarvest = -1;
		currentTile.growthDays = 0;
		
		EventHandler.CallRefreshCurrentMap();
	}
	
	/// <summary>
	/// ����ũ����
	/// </summary>
	public void SpawnHarvestItem()
	{
		for(int i =0;i<cropDetails.producedItemID.Length;i++)
		{
			int amountToProduce;
			
			if(cropDetails.producedMinAmount[i] == cropDetails.producedMaxAmount[i])
			{
				// ֻ����ָ������
				amountToProduce = cropDetails.producedMinAmount[i];
			}else
			{
				// ������Ʒ�������
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
					// ����������������Ʒ
					var dirx = transform.position.x > playerTransfrom.position.x?1:-1;
					// һ����Χ�ڵ����
					var spawnPos = new Vector3(transform.position.x + Random.Range(dirx,cropDetails.spawnRadius.x * dirx),
					transform.position.y + Random.Range(-cropDetails.spawnRadius.y,cropDetails.spawnRadius.y),0);
					
					EventHandler.CallInstantiateItemInScene(cropDetails.producedItemID[i],spawnPos);
				}
			}
		}
		
		// �ո���֮��Ҫ������Ƭ��Ϣ�ϵ��ո���Ϣ���и���
		if(currentTile!=null)
		{
			currentTile.daysSinceLastHarvest++;
			
			// ��ʵ�����ظ�����
			if(cropDetails.daysToRegrow>0&&currentTile.daysSinceLastHarvest<=cropDetails.reGrowTime)
			{
				// �����ʵ�����ظ������Ļ�����ô�ҵ�ǰ��Ƭ������������Ҫ���е��ˣ����˵������ʵ���������Ľ׶Σ������Ǹ��׶���cropDetails.daysToRegrow������
				currentTile.growthDays = cropDetails.TotalGrowthDays - cropDetails.daysToRegrow;
				// ˢ������
				EventHandler.CallRefreshCurrentMap();
			}else
			{
				// �����ظ����� --- �����Ǵ�ָ�룬���������޸�Ҳ�ǿ��Ե�
				currentTile.daysSinceLastHarvest = -1;
				currentTile.seedItemID = -1;
			}
			
			Destroy(gameObject);
		}
	}
}
