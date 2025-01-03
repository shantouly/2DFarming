using System;
using System.Collections;
using System.Collections.Generic;
using Fram.Dialogue;
using UnityEngine;

public static class EventHandler
{
	// 注册一个事件
	public static event Action<InventoryLocation, List<InventoryItem>> UpdateInventoryUI;

	/// <summary>
	/// UpdateInventoryUI的回调函数
	/// </summary>
	/// <param name="location"></param>
	/// <param name="list"></param>
	public static void CallUpdateInventoryUI(InventoryLocation location, List<InventoryItem> list)
	{
		UpdateInventoryUI?.Invoke(location, list);
	}

	public static event Action<int, Vector3> InstantiateItemInScene;
	public static void CallInstantiateItemInScene(int ID, Vector3 pos)
	{
		InstantiateItemInScene?.Invoke(ID, pos);
	}

	public static event Action<int, Vector3,itemType> DropItemEvent;
	public static void CallDropItemEvent(int ID,Vector3 pos,itemType itemType)
	{
		DropItemEvent?.Invoke(ID, pos,itemType);
	}

	public static event Action<ItemDetails, bool> ItemSelectedEvent;
	public static void CallItemSelectedEvent(ItemDetails itemDetails,bool isSelected)
	{
		ItemSelectedEvent?.Invoke(itemDetails, isSelected);
	}

	// 用于Time UI里面的UI组件的事件
	public static event Action<int, int,int, Season> GameMinuteEvent;
	public static void CallGameMinuteEvent(int minute,int hour,int day,Season season)
	{
		GameMinuteEvent?.Invoke(minute, hour, day,season);
	}

	public static event Action<int, int, int, int, Season> GameDateEvent;
	public static void CallGameDateEvent(int hour,int day,int month,int year,Season season)
	{
		GameDateEvent.Invoke(hour, day, month, year, season);
	}
	
	public static event Action<int,Season> GameDayEvent;
	public static void CallGameDayEvent(int day,Season season)
	{
		GameDayEvent?.Invoke(day, season);
	}

	public static event Action<string, Vector3> TransitionEvent;
	public static void CallTransitionEvent(string sceneName,Vector3 pos)
	{
		TransitionEvent?.Invoke(sceneName, pos);
	}

	public static event Action BeforeSceneUnloadEvent;
	public static void CallBeforeSceneUnloadEvent()
	{
		BeforeSceneUnloadEvent?.Invoke();
	}

	public static event Action AfterSceneUnloadEvent;
	public static void CallAfterSceneUnloadEvent()
	{
		AfterSceneUnloadEvent?.Invoke();
	}

	public static event Action<Vector3> MoveToPosition;
	public static void CallMoveToPosition(Vector3 pos)
	{
		MoveToPosition?.Invoke(pos);
	}

	public static event Action<Vector3, ItemDetails> MouseClickEvent;
	public static void CallMouseClickEvent(Vector3 position,ItemDetails itemDetails)
	{
		MouseClickEvent?.Invoke(position, itemDetails);
	}

	/// <summary>
	/// 当动画执行完毕之后才执行的方法---人物扔掉物品.........
	/// </summary>
	public static event Action<Vector3, ItemDetails> ExecuteActionAfterAnimation;
	public static void CallExecuteActionAfterAnimation(Vector3 position,ItemDetails itemDetails)
	{
		ExecuteActionAfterAnimation?.Invoke(position, itemDetails);
	}
	
	public static event Action<int,TileDetails> PlantSeedEvent;
	public static void CallPlantSeedEvent(int itemID,TileDetails tileDetails)
	{
		PlantSeedEvent?.Invoke(itemID,tileDetails);
	}
	
	public static event Action<int> HarvestAtPlayerPosition;
	public static void CallHarvestAtPlayerPosition(int itemID)
	{
		HarvestAtPlayerPosition?.Invoke(itemID);
	}
	
	public static event Action RefreshCurrentMap;
	public static void CallRefreshCurrentMap()
	{
		RefreshCurrentMap?.Invoke();
	}
	
	public static event Action<ParticleEffectType,Vector3> ParticleEffectEvent;
	public static void CallParticleEffectEvent(ParticleEffectType type,Vector3 position)
	{
		ParticleEffectEvent?.Invoke(type,position);
	}
	
	public static event Action GenerateCropEvent;
	public static void CallGenerateCropEvent()
	{
		GenerateCropEvent?.Invoke();
	}
	
	public static event Action<DialoguePiece> ShowDialogueEvent;
	public static void CallShowDialogueEvent(DialoguePiece piece)
	{
		ShowDialogueEvent?.Invoke(piece);
	}
	
	public static event Action<slotType,IventoryBag_SO> BaseBagOpenEvent;
	public static void CallBaseBagOpenEvent(slotType slotType,IventoryBag_SO bag_SO)
	{
		BaseBagOpenEvent?.Invoke(slotType,bag_SO);
	}
	
	public static event Action<slotType,IventoryBag_SO> BaseBagCloseEvent;
	public static void CallBaseBagCloseEvent(slotType slotType,IventoryBag_SO bag_SO)
	{
		BaseBagCloseEvent?.Invoke(slotType,bag_SO);
	}
	
	public static event Action<GameState> UpdateGameStateEvent;
	public static void CallUpdateGameStateEvent(GameState state)
	{
		UpdateGameStateEvent?.Invoke(state);
	}
	
	public static event Action<ItemDetails,bool> ShowTradeUI;
	public static void CallShowTradeUI(ItemDetails itemDetails,bool isSell)
	{
		ShowTradeUI?.Invoke(itemDetails,isSell);
	}
	
	public static event Action<int,Vector3> BuildFurnitureEvent;
	public static void CallBuildFurnitureEvent(int itemID,Vector3 mousePos)
	{
		BuildFurnitureEvent?.Invoke(itemID,mousePos);
	}
	
	public static event Action<Season,LightShift,float> LightShiftChangeEvent;
	public static void CallLightShiftChangeEvent(Season season,LightShift lightShift,float timeDifference)
	{
		LightShiftChangeEvent?.Invoke(season,lightShift,timeDifference);
	}
	
	// 音效
	public static event Action<SoundDetails> InitSoundEffect;
	public static void CallInitSoundEffect(SoundDetails soundDetails)
	{
		InitSoundEffect?.Invoke(soundDetails);
	}
	
	public static event Action<SoundName> PlaySoundEvent;
	public static void CallPlaySoundEvent(SoundName soundName)
	{
		PlaySoundEvent?.Invoke(soundName);
	}
	
	public static event Action<int> StartNewGameEvent;
	public static void CallStartNewGameEvent(int index)
	{
		StartNewGameEvent?.Invoke(index);
	}
	
	public static event Action EndGameEvent;
	public static void CallEndGameEvent()
	{
		EndGameEvent?.Invoke();
	}
}
