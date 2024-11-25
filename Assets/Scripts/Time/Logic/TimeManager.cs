using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fram.Save;
using UnityEngine;

public class TimeManager : Singleton<TimeManager>,ISaveable
{
	private int gameSecond;
	private int gameMinute;
	private int gameHour;
	private int gameDay;
	private int gameMonth;
	private int gameYear;

	private Season gameSeason;
	private int monthInSeason = 3;
	public bool gameClockPause;             // 用来是否暂停时间
	private float tikTime;
	public TimeSpan GameTime => new TimeSpan(gameHour,gameMinute,gameSecond);       // 游戏的时间戳

	public string GUID => GetComponent<DataGUID>().guid;

	private float timeDifference;

	protected override void Awake()
	{
		base.Awake();
		NewGameTime();
	}
	
	void OnEnable()
	{
		EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
		EventHandler.AfterSceneUnloadEvent += OnAfterSceneUnloadEvent;
		EventHandler.UpdateGameStateEvent += OnUpdateGameStateEvent;
		EventHandler.StartNewGameEvent += OnStartNewGameEvent;
		EventHandler.EndGameEvent += OnEndGameEvent;
	}
	
	void OnDisable()
	{
		EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
		EventHandler.AfterSceneUnloadEvent -= OnAfterSceneUnloadEvent;
		EventHandler.UpdateGameStateEvent -= OnUpdateGameStateEvent;
		EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
		EventHandler.EndGameEvent -= OnEndGameEvent;
	}


	private void OnStartNewGameEvent(int obj)
	{
		NewGameTime();
		gameClockPause = false;
	}
	private void OnEndGameEvent()
	{
		gameClockPause = true;
	}
	
	private void NewGameTime()
	{
		gameSecond = 0;
		gameMinute = 0;
		gameHour = 7;
		gameDay = 1;
		gameMonth = 1;
		gameYear = 2024;
		gameSeason = Season.春天;
	}

	private void OnUpdateGameStateEvent(GameState state)
	{
		gameClockPause = state == GameState.GamePause;
	}

	private void OnAfterSceneUnloadEvent()
	{
		gameClockPause = false;
		
		EventHandler.CallGameDateEvent(gameHour, gameDay, gameMonth, gameYear, gameSeason);
		EventHandler.CallGameMinuteEvent(gameMinute, gameHour,gameDay,gameSeason);
		// 灯光
		EventHandler.CallLightShiftChangeEvent(gameSeason,GetLightShift(),timeDifference);
	}

	private void OnBeforeSceneUnloadEvent()
	{
		gameClockPause = true;
	}

	private void Start()
	{
		ISaveable saveable = this;
		saveable.RegisterSaveable();
		gameClockPause = true;
		// EventHandler.CallGameDateEvent(gameHour, gameDay, gameMonth, gameYear, gameSeason);
		// EventHandler.CallGameMinuteEvent(gameMinute, gameHour,gameDay,gameSeason);
		// // 灯光
		// EventHandler.CallLightShiftChangeEvent(gameSeason,GetLightShift(),timeDifference);
	}

	private void Update()
	{
		if (!gameClockPause)
		{
			tikTime += Time.deltaTime;

			if(tikTime >= Settings.secondThresHold)
			{
				tikTime -= Settings.secondThresHold;
				UpdateGameTime();
			}
		}

		if(Input.GetKey(KeyCode.G))
		{
			gameDay++;
			EventHandler.CallGameDayEvent(gameDay,gameSeason);
			EventHandler.CallGameDateEvent(gameHour,gameDay,gameMonth,gameSecond,gameSeason);
		}
		
		if(Input.GetKey(KeyCode.T))
		{
			for(int i = 0 ; i<60;i++)
			{
				UpdateGameTime();
			}
		}
	}


	private void UpdateGameTime()
	{
		gameSecond++;
		if(gameSecond > Settings.secondHold)
		{
			gameMinute++;
			gameSecond = 0;

			if(gameMinute > Settings.minuteHold)
			{
				gameHour++;
				gameMinute = 0;

				if(gameHour > Settings.hourHold)
				{
					gameDay++;
					gameHour = 0;

					if(gameDay > Settings.dayHold)
					{
						gameDay = 1;
						gameMonth++;

						if(gameMonth > 12)
						{
							gameMonth = 1;
						}

						monthInSeason--;
						if(monthInSeason == 0)
						{
							monthInSeason = 3;

							int seasonNumber = (int)gameSeason;
							seasonNumber++;

							if(seasonNumber > Settings.seasonHold)
							{
								seasonNumber = 0;
								gameYear++;
							}

							gameSeason = (Season)seasonNumber;

							if(gameYear > 9999)
							{
								gameYear = 2024;
							}
						}
					}
					EventHandler.CallGameDayEvent(gameDay,gameSeason);
				}
				EventHandler.CallGameDateEvent(gameHour, gameDay, gameMonth, gameYear, gameSeason);
			}
			EventHandler.CallGameMinuteEvent(gameMinute,gameHour,gameDay,gameSeason);
			
			// 切换灯光
			EventHandler.CallLightShiftChangeEvent(gameSeason,GetLightShift(),timeDifference);
		}
	}
	
	private LightShift GetLightShift()
	{
		if(GameTime >= Settings.morningTime && GameTime < Settings.nightTime)
		{
			// 此时是白天
			timeDifference = MathF.Abs((float)(GameTime - Settings.morningTime).TotalMinutes);
			Debug.Log("morning");
			return LightShift.Morning;
		}
		
		if(GameTime < Settings.morningTime || GameTime >= Settings.nightTime)
		{
			// 此时是晚上
			timeDifference = MathF.Abs((float)(GameTime - Settings.nightTime).TotalMinutes);
			return LightShift.Night;
		}
		
		return LightShift.Morning;
	}

	/// <summary>
	/// 存储我当前游戏的时间数据
	/// </summary>
	/// <returns></returns>
	public GameSaveData gameSaveData()
	{
		GameSaveData saveData = new GameSaveData();
		saveData.timeDict = new Dictionary<string, int>();
		saveData.timeDict.Add("gameYear", gameYear);
		saveData.timeDict.Add("gameMonth", gameMonth);
		saveData.timeDict.Add("gameDay", gameDay);
		saveData.timeDict.Add("gameHour", gameHour);
		saveData.timeDict.Add("gameMinute", gameMinute);
		saveData.timeDict.Add("gameSecond", gameSecond);
		saveData.timeDict.Add("gameSeason", (int)gameSeason);
		saveData.timeDifference = this.timeDifference;

		return saveData;
	}

	/// <summary>
	/// 获取我存储的游戏时间数据
	/// </summary>
	/// <param name="saveData"></param>
	public void RestoreData(GameSaveData saveData)
	{
		gameYear = saveData.timeDict["gameYear"];
		gameMonth = saveData.timeDict["gameMonth"];
		gameDay = saveData.timeDict["gameDay"];
		gameHour = saveData.timeDict["gameHour"];
		gameMinute = saveData.timeDict["gameMinute"];
		gameSecond = saveData.timeDict["gameSecond"];
		gameSeason = (Season)saveData.timeDict["gameSeason"];
		this.timeDifference = saveData.timeDifference;
	}
}
