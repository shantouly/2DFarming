using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fram.Save
{
	public class GameSaveData
	{
		public string dataSceneName;
		/// <summary>
		/// 存储人物坐标，string--人物名字
		/// </summary>
		public Dictionary<string,SerializableVector3> characterPosDict;		// 角色的数据
		public Dictionary<string,List<SceneItem>> sceneItemDict;			// 场景中物体的数据
		public Dictionary<string,List<SceneFurniture>> sceneFurnitureDic;	// 场景中家具的数据	
		//public Dictionary<string,List<int>> sceneBoxes;
		public Dictionary<string,TileDetails> tileDetailsDict;				// 场景中瓦片的数据
		public Dictionary<string,bool> firstLoadDict;						// 是否是第一次加载的数据
		public Dictionary<string,List<InventoryItem>> inventoryDict;		// 背包的数据
		public Dictionary<string,List<InventoryItem>> startBoxDict;
		public Dictionary<string,int> timeDict;
		public LightShift currentShift;
		public float timeDifference;
		public int playerMoney;
		
		//NPC
		public string targetScene;
		public bool interactable;
		public int animationInstanceID;
	}
}

