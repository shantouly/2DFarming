using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fram.AStar
{
	public class NPCManager : Singleton<NPCManager>
	{
		public SceneRouteDataList_SO sceneRouteData;
		public List<NPCPosition> NpcPositionList;
		public Dictionary<string,SceneRoute> sceneRouteDict = new Dictionary<string, SceneRoute>();

		protected override void Awake()
		{
			base.Awake();
			
			InitSceneRouteDict();
		}

		void OnEnable()
		{
			EventHandler.StartNewGameEvent += OnStartNewGameEvent;
		}
		
		void OnDisable()
		{
			EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
		}

		private void OnStartNewGameEvent(int obj)
		{
			foreach(var character in NpcPositionList)
			{
				character.npc.position = character.position;
				character.npc.GetComponent<NPCMovement>().currentScene = character.startScene;
			}
		}

		/// <summary>
		/// ��ʼ��·���ֵ�
		/// </summary>
		private void InitSceneRouteDict()
		{
			if(sceneRouteData.sceneRouteList.Count > 0)
			{
				foreach(SceneRoute route in sceneRouteData.sceneRouteList)
				{
					var key = route.fromSceneName + route.gotoSceneName;
					
					if(sceneRouteDict.ContainsKey(key))
					{
						continue;
					}else
					{
						sceneRouteDict.Add(key,route);
					}
				}
			}
		}
		
		/// <summary>
		/// ��ȡ��������֮���·��
		/// </summary>
		/// <param name="fromSceneName"></param>
		/// <param name="gotoSceneName"></param>
		/// <returns></returns>
		public SceneRoute GetSceneRoute(string fromSceneName,string gotoSceneName)
		{
			return sceneRouteDict[fromSceneName+gotoSceneName];
		}
	}
}

