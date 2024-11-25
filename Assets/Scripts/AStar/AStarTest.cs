using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Fram.AStar
{
	public class AStarTest : MonoBehaviour
	{
		private AStar aStar;
		[Header("用于测试")]
		public Vector2Int startPos;
		public Vector2Int finishPos;
		public Tilemap disPlayMap;
		public Tile displayTile;
		public bool displayStartAndFinish;
		public bool displayPath;
		private Stack<MovementStep> npcMovementStep;
		
		[Header("测试移动NPC")]
		public NPCMovement mayorMovement;
		public NPCMovement girl02Movement;
		public bool moveMayor;
		public bool moveGirl;
		[SceneName] public string targetScene_Mayor;
		[SceneName] public string targetScene_Girl02;
		public Vector2Int targetPos_Mayor;
		public Vector2Int targetPos_Girl02;
		public AnimationClip stopClip_Mayor;
		public AnimationClip stopClip_Girl02;
		
		void Awake()
		{
			aStar = GetComponent<AStar>();
			npcMovementStep = new Stack<MovementStep>();
		}
		
		void Update()
		{
			ShowPathOnGridMap();
			
			if(moveMayor)
			{
				moveMayor = false;
				var schedule = new ScheduleDetails(0,0,0,0,Season.春天,targetScene_Mayor,targetPos_Mayor,stopClip_Mayor,true);
				mayorMovement.BuildPath(schedule);
			}
			
			if(moveGirl)
			{
				moveGirl = false;
				var schedule = new ScheduleDetails(0,0,0,0,Season.春天,targetScene_Girl02,targetPos_Girl02,stopClip_Girl02,true);
				girl02Movement.BuildPath(schedule);
			}
		}
		
		private void ShowPathOnGridMap()
		{
			if(disPlayMap!=null && displayTile!=null)
			{
				if(displayStartAndFinish)
				{
				disPlayMap.SetTile((Vector3Int)startPos,displayTile);
				disPlayMap.SetTile((Vector3Int)finishPos,displayTile);
				}
			}else
			{
				disPlayMap.SetTile((Vector3Int)startPos,null);
				disPlayMap.SetTile((Vector3Int)finishPos,null);
			}
			
			if(displayPath)
			{
				var sceneName = SceneManager.GetActiveScene().name;
				
				aStar.BuildPath(sceneName,startPos,finishPos,npcMovementStep);
				
				foreach(var step in npcMovementStep)
				{
					disPlayMap.SetTile((Vector3Int)step.gridCoordinate,displayTile);
				}
			}else
			{
				if(npcMovementStep.Count > 0)
				{
					foreach(var step in npcMovementStep)
					{
						disPlayMap.SetTile((Vector3Int)step.gridCoordinate,null);
					}
					
					npcMovementStep.Clear();
				}
			}
		}

	}
}

