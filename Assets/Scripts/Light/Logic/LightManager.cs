using System.Collections;
using System.Collections.Generic;
using Fram.Save;
using UnityEditor.Rendering;
using UnityEngine;

public class LightManager : MonoBehaviour
{
	private LightController[] sceneLight;
	private LightShift currentLightShift;
	private Season currentSeason;
	private float timeDifference = Settings.lightChangeDruation;

	//public string GUID => GetComponent<DataGUID>().guid;

	void OnEnable()
	{
		EventHandler.AfterSceneUnloadEvent += OnAfterSceneUnloadEvent;
		EventHandler.LightShiftChangeEvent += OnLightShiftChangeEvent;
		EventHandler.StartNewGameEvent += OnStartNewGameEvent;
	}
	
	// void Start()
	// {
	// 	ISaveable saveable = this;
	// 	saveable.RegisterSaveable();
	// }
	
	void OnDisable()
	{
		EventHandler.AfterSceneUnloadEvent -= OnAfterSceneUnloadEvent;
		EventHandler.LightShiftChangeEvent -= OnLightShiftChangeEvent;
		EventHandler.StartNewGameEvent += OnStartNewGameEvent;
	}

	/// <summary>
	/// 初始的时候将这个设置为早上
	/// </summary>
	/// <param name="obj"></param>
	private void OnStartNewGameEvent(int obj)
	{
		currentLightShift = LightShift.Morning;
	}

	private void OnAfterSceneUnloadEvent()
	{
		sceneLight = FindObjectsOfType<LightController>();
		foreach(LightController light in sceneLight)
		{
			// 执行lightController中改变灯光的方法
			light.ChangeLightShtft(currentSeason,currentLightShift,timeDifference);
		}
	}
	
	private void OnLightShiftChangeEvent(Season season, LightShift shift, float timeDifference)
	{
		currentSeason = season;
		this.timeDifference = timeDifference;
		
		if(currentLightShift != shift)
		{
			currentLightShift = shift;
			foreach(LightController light in sceneLight)
			{
				light.ChangeLightShtft(currentSeason,currentLightShift,timeDifference);
			}
		}
	}

	// public GameSaveData gameSaveData()
	// {
	// 	GameSaveData saveData = new GameSaveData();
	// 	saveData.currentShift = this.currentLightShift;
	// 	return saveData;
	// }

	// public void RestoreData(GameSaveData saveData)
	// {
	// 	this.currentLightShift = saveData.currentShift;
	// }
}
