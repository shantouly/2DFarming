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
					return timeData.timeDict["gameYear"]+"年/"+(Season)timeData.timeDict["gameSeason"]+"/"+timeData.timeDict["gameMonth"]+"月/"+timeData.timeDict["gameDay"]+"日/";
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
						"01 Field"=>"农场",
						"02 Home" => "小木屋",
						"03 Stall"=>"市场",
						"05 Start"=>"海边",
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

