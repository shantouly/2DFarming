using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;

[System.Serializable]
public class CropDetails
{
	public int seedItemID;								// 种子的ID
	[Header("不同阶段需要的天数")]
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
	[Header("不同阶段的图片")]
	public Sprite[] growthSprites;
	[Header("可种子的季节")]
	public Season[] seasons;
	
	[Space]
	[Header("收割工具")]
	public int[] harvestToolItemID;						// 有哪些工具可以收割该作物
	[Header("收割工具所需的次数")]
	public int[] requireActionCount;
	public int transferItemID;
	
	[Space]
	[Header("收割果实信息")]
	public int[] producedItemID;					//生成出来的ID
	public int[] producedMinAmount;					// 生成的最小个数
	public int[] producedMaxAmount;					// 生成的最多个数
	public Vector2 spawnRadius;						// 生成的范围
	
	[Header("再次生长的时间")]
	public int daysToRegrow;
	public int reGrowTime;
	
	[Header("Options")]
	public bool generateAtPlayerPosition;
	public bool hasAnimation;
	public bool hasParticleEffect;
	
	public ParticleEffectType particleEffectType;
	public Vector3 effectPos;					// 生成粒子效果的坐标
	public SoundName soundEffect;
	
	/// <summary>
	/// 检测判断该工具是否可以收割果实
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
	/// 获取到该果实需要执行多少下才能收割
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
