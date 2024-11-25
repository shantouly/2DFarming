using System.Collections;
using System.Collections.Generic;
using Fram.Inventory;
using UnityEngine;

public class AnimatorOverride : MonoBehaviour
{
	private Animator[] anims;

	public SpriteRenderer holdItem;

	[Header("�����ֶ����б�")]
	public List<AnimatorType> animatorTypes;
	private Dictionary<string, Animator> animateNameDict = new Dictionary<string, Animator>();

	private void Awake()
	{
		anims = GetComponentsInChildren<Animator>();

		foreach(var anim in anims)
		{
			animateNameDict.Add(anim.name, anim);
		}
	}

	private void OnEnable()
	{
		EventHandler.ItemSelectedEvent += OnItemSelectedEvent;
		EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
		EventHandler.HarvestAtPlayerPosition += OnHarvestAtPlayerPosition;
	}

	/// <summary>
	/// ũ������������ҵ�λ����
	/// </summary>
	/// <param name="itemID"></param>
	private void OnHarvestAtPlayerPosition(int itemID)
	{
		Sprite itemSprite = InventoryManager.Instance.GetItemDetails(itemID).itemOnWorldSprite;
		if(holdItem.enabled == false)
		{
			StartCoroutine(ShowItem(itemSprite));
		}else
		{
			return;
		}
	}
	
	/// <summary>
	/// ��ʾͼƬ
	/// </summary>
	/// <param name="itemSprite"></param>
	/// <returns></returns>
	private IEnumerator ShowItem(Sprite itemSprite)
	{
		holdItem.sprite = itemSprite;
		holdItem.enabled = true;
		yield return new WaitForSeconds(0.7f);
		holdItem.enabled = false;
	}

	private void OnBeforeSceneUnloadEvent()
	{
		holdItem.enabled = false;
		SwitchAnimator(PartType.None);
	}

	private void OnDisable()
	{
		EventHandler.ItemSelectedEvent -= OnItemSelectedEvent;
		EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
		EventHandler.HarvestAtPlayerPosition -= OnHarvestAtPlayerPosition;
	}

	private void OnItemSelectedEvent(ItemDetails itemDetails, bool isSelected)
	{
/*        PartType currentType = itemDetails.itemtype switch
		{
			itemType.Seed => PartType.Carry,
			itemType.Commodity => PartType.Carry,
			_=>PartType.None
		};*/

		PartType currentType = itemDetails.itemtype switch
		{
			itemType.Seed => PartType.Carry,
			itemType.Commodity => PartType.Carry,
			itemType.HoeTool => PartType.Hoe,
			itemType.WaterTool => PartType.water,
			itemType.CollectTool => PartType.Collect,
			itemType.ChopTool => PartType.Chop,
			itemType.BreakTool => PartType.Break,
			itemType.ReapTool => PartType.Reap,
			itemType.Furniture => PartType.Carry,
			_=>PartType.None
		};
		if(isSelected == false)
		{
			currentType = PartType.None;
			holdItem.enabled = false;
		}
		else
		{
			if(currentType == PartType.Carry)
			{
				//��������Ա�����
				holdItem.sprite = itemDetails.itemOnWorldSprite;
				holdItem.enabled = true;
			}
			else
			{
				holdItem.enabled = false;
			}
		}
		SwitchAnimator(currentType);
	}

	private void SwitchAnimator(PartType partType)
	{
		foreach(var item in animatorTypes)
		{
			if(partType == item.partType)
			{
				// ת��animator
				animateNameDict[item.partName.ToString()].runtimeAnimatorController = item.overrideController;
			}
		}
	}
}
