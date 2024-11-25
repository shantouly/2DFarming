using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;

[System.Serializable]
public class CropDetails
{
	public int seedItemID;								// ���ӵ�ID
	[Header("��ͬ�׶���Ҫ������")]
	public int[] growthDays;
	
	public int TotalGrowthDays
	{
		get
		{
			int amount = 0;
			foreach(var days in growthDays)
			{
				amount+=days;
			}
			
			return amount;
		}
	}
	
	public GameObject[] growthPreafabs;
	[Header("��ͬ�׶ε�ͼƬ")]
	public Sprite[] growthSprites;
	[Header("�����ӵļ���")]
	public Season[] seasons;
	
	[Space]
	[Header("�ո��")]
	public int[] harvestToolItemID;						// ����Щ���߿����ո������
	[Header("�ո������Ĵ���")]
	public int[] requireActionCount;
	public int transferItemID;
	
	[Space]
	[Header("�ո��ʵ��Ϣ")]
	public int[] producedItemID;					//���ɳ�����ID
	public int[] producedMinAmount;					// ���ɵ���С����
	public int[] producedMaxAmount;					// ���ɵ�������
	public Vector2 spawnRadius;						// ���ɵķ�Χ
	
	[Header("�ٴ�������ʱ��")]
	public int daysToRegrow;
	public int reGrowTime;
	
	[Header("Options")]
	public bool generateAtPlayerPosition;
	public bool hasAnimation;
	public bool hasParticleEffect;
	
	public ParticleEffectType particleEffectType;
	public Vector3 effectPos;					// ��������Ч��������
	public SoundName soundEffect;
	
	/// <summary>
	/// ����жϸù����Ƿ�����ո��ʵ
	/// </summary>
	/// <param name="toolID"></param>
	/// <returns></returns>
	public bool CheckToolAvailable(int toolID)
	{
		foreach(var tool in harvestToolItemID)
		{
			if(tool == toolID)
			{
				return true;
			}
		}
		
		return false;
	}
	
	/// <summary>
	/// ��ȡ���ù�ʵ��Ҫִ�ж����²����ո�
	/// </summary>
	/// <param name="toolID"></param>
	/// <returns></returns>
	public int GetTotalRequireCount(int toolID)
	{
		for(int i =0;i<harvestToolItemID.Length;i++)
		{
			if(harvestToolItemID[i] == toolID)
			{
				return requireActionCount[i];
			}
		}
		return -1;
	}
}
