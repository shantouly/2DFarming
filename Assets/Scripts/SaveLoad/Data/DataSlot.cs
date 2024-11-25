using System.Collections;
using System.Collections.Generic;
using Fram.Transition;
using UnityEngine;

namespace Fram.Save
{
	public class DataSlot
	{
		public Dictionary<string,GameSaveData> dataDict = new Dictionary<string, GameSaveData>();
		
		public string DataTime
		{
			get
			{
				var key = TimeManager.Instance.GUID;
				
				if(dataDict.ContainsKey(key))
				{
					var timeData = dataDict[key];
					return timeData.timeDict["gameYear"]+"��/"+(Season)timeData.timeDict["gameSeason"]+"/"+timeData.timeDict["gameMonth"]+"��/"+timeData.timeDict["gameDay"]+"��/";
				}else
				{
					return string.Empty;
				}
			}
		}
		
		public string DataScene
		{
			get
			{
				var key = TransitionManager.Instance.GUID;
				
				if(dataDict.ContainsKey(key))
				{
					var transitionData = dataDict[key];
					return transitionData.dataSceneName switch
					{
						"01 Field"=>"ũ��",
						"02 Home" => "Сľ��",
						"03 Stall"=>"�г�",
						"05 Start"=>"����",
						_=>string.Empty
					};
				}else
				{
					return string.Empty;
				}
			}
		}
	}
}

