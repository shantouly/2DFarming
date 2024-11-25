using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Fram.Save
{
	public class SaveManager : Singleton<SaveManager>
	{
		private List<ISaveable> saveableList = new List<ISaveable>();
		public List<DataSlot> dataSlots = new List<DataSlot>(new DataSlot[3]);
		private string jsonFolder;
		private int currentDataIndex;

		protected override void Awake()
		{
			base.Awake();
			
			jsonFolder = Application.persistentDataPath + "/SAVE DATA/";
			
			ReadSaveData();
		}
		
		void OnEnable()
		{
			EventHandler.StartNewGameEvent += OnStartNewGameEvent;
			EventHandler.EndGameEvent += OnEndGameEvent;
		}
		
		void OnDisable()
		{
			EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
			EventHandler.EndGameEvent -= OnEndGameEvent;
		}


		private void OnStartNewGameEvent(int index)
		{
			currentDataIndex = index;
		}
		private void OnEndGameEvent()
		{
			Save(currentDataIndex);
		}

		void Update()
		{
			if(Input.GetKeyDown(KeyCode.I))
			{
				Save(currentDataIndex);
			}
			if(Input.GetKeyDown(KeyCode.O))
			{
				Load(currentDataIndex);
			}
		}
		
		private void ReadSaveData()
		{
			if(Directory.Exists(jsonFolder))
			{
				for(int i =0;i<dataSlots.Count;i++)
				{
					var resultPath = jsonFolder+"data"+i+".json";
					Debug.Log(resultPath);
					if(File.Exists(resultPath))
					{
						var stringData = File.ReadAllText(resultPath);
						var jsonData = JsonConvert.DeserializeObject<DataSlot>(stringData);
						dataSlots[i] = jsonData;
					}
				}
			}
		}

		/// <summary>
		/// 注册saveable，并添加至字典中
		/// </summary>
		/// <param name="saveable"></param>
		public void RegisterSaveable(ISaveable saveable)
		{
			if(!saveableList.Contains(saveable))
			{
				saveableList.Add(saveable);
			}
		}
		
		
		private void Save(int index)
		{
			DataSlot data = new DataSlot();
			
			foreach(var saveable in saveableList)
			{
				data.dataDict.Add(saveable.GUID,saveable.gameSaveData());
			}
			
			dataSlots[index] = data;
			
			var resultPath = jsonFolder+"data"+index+".json";
			
			var jsonData = JsonConvert.SerializeObject(dataSlots[index],Formatting.Indented); 
			
			if(!File.Exists(resultPath))
			{
				Directory.CreateDirectory(jsonFolder);
			}
			
			File.WriteAllText(resultPath,jsonData);
			
			//Debug.Log("保存！");
		}
		
		/// <summary>
		/// 加载我的数据
		/// </summary>
		/// <param name="index"></param>
		public void Load(int index)
		{
			currentDataIndex = index;
			
			var resultPath = jsonFolder+"data"+index+".json";
			Debug.Log(resultPath);
			
			var stringData = File.ReadAllText(resultPath);
			
			var jsonData = JsonConvert.DeserializeObject<DataSlot>(stringData);
			
			foreach(var saveable in saveableList)
			{
				saveable.RestoreData(jsonData.dataDict[saveable.GUID]);
			}
		}
	}	
}
