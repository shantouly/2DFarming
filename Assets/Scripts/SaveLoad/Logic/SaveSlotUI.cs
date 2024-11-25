using System;
using System.Collections;
using System.Collections.Generic;
using Fram.Save;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlotUI : MonoBehaviour
{
	public Text dateTime,dataScene;
	private Button currentButton;
	private DataSlot currentData;
	private int index => transform.GetSiblingIndex();
	
	void OnEnable()
	{
		SetupSlotUI();
		//Debug.Log(index);
	}
	
	void Awake()
	{
		currentButton = GetComponent<Button>();
		currentButton.onClick.AddListener(LoadGameData);
	}
	
	private void SetupSlotUI()
	{
		currentData = SaveManager.Instance.dataSlots[index];
		if(currentData != null)
		{
			dateTime.text = currentData.DataTime;
			dataScene.text = currentData.DataScene;
		}else
		{
			dateTime.text = "这个世界还没开始";
			dataScene.text = "梦还没开始";
		}
	}
	
	private void LoadGameData()
	{
		if(currentData!=null)
		{
			TimeLineManager.Instance.isFirstLoad = false;
			SaveManager.Instance.Load(index);
		}else
		{
			//Debug.Log("New Game");
			TimeLineManager.Instance.isFirstLoad = true;
			EventHandler.CallStartNewGameEvent(index);
		}
	}
}
