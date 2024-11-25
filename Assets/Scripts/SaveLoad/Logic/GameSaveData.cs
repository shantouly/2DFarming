using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fram.Save
{
	public class GameSaveData
	{
		public string dataSceneName;
		/// <summary>
		/// �洢�������꣬string--��������
		/// </summary>
		public Dictionary<string,SerializableVector3> characterPosDict;		// ��ɫ������
		public Dictionary<string,List<SceneItem>> sceneItemDict;			// ���������������
		public Dictionary<string,List<SceneFurniture>> sceneFurnitureDic;	// �����мҾߵ�����	
		//public Dictionary<string,List<int>> sceneBoxes;
		public Dictionary<string,TileDetails> tileDetailsDict;				// ��������Ƭ������
		public Dictionary<string,bool> firstLoadDict;						// �Ƿ��ǵ�һ�μ��ص�����
		public Dictionary<string,List<InventoryItem>> inventoryDict;		// ����������
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

