using System.Collections;
using System.Collections.Generic;
using Fram.Map;
using UnityEngine;

namespace Fram.AStar
{
	public class AStar : Singleton<AStar>
	{
		private GridNodes gridNodes;
		private Node startNode;
		private Node targetNode;
		private int gridWidth;
		private int gridHeight;
		private int originX;
		private int originY;
		
		private List<Node> openNodeList;			// 当前选中的Node的周围的8个Node
		private HashSet<Node> closedNodelist;		// 所有被选中的这个点
		
		private bool PathFound;
		
		/// <summary>
		/// 构建路径更新stack中的每一步
		/// </summary>
		/// <param name="sceneName"></param>
		/// <param name="startPos"></param>
		/// <param name="endPos"></param>
		/// <param name="npcMovementStep"></param>
		public void BuildPath(string sceneName,Vector2Int startPos,Vector2Int endPos,Stack<MovementStep> npcMovementStep)
		{
			PathFound = false;
			
			if(GenerateGridNodes(sceneName,startPos,endPos))
			{
				//Debug.Log(true);
				// 查找最短路径
				if(FindShortestPath())
				{
					// 构建NPC移动路径
					UpdatePathMovementStepStack(sceneName,npcMovementStep);
				}
				
			}
		}
		
		/// <summary>
		/// 构建网格节点信息，初始化两个列表
		/// </summary>
		/// <param name="sceneName">场景的名字</param>
		/// <param name="startPos">起始的坐标点</param>
		/// <param name="endPos">目标点</param>
		/// <returns></returns>
		private bool GenerateGridNodes(string sceneName,Vector2Int startPos,Vector2Int endPos)
		{
			// 如果该地图存在的话，初始化我的网格中的信息
			if(GridManager.Instance.GetGridDimensions(sceneName,out Vector2Int gridDimensions,out Vector2Int gridOrigin))
			{
				// 根据瓦片地图范围构建网格移动节点范围数组
				gridNodes = new GridNodes(gridDimensions.x,gridDimensions.y);
				gridWidth = gridDimensions.x;
				gridHeight = gridDimensions.y;
				originX = gridOrigin.x;
				originY = gridOrigin.y;
				
				// 列表初始化
				openNodeList = new List<Node>();
				
				closedNodelist = new HashSet<Node>();		
			}else
				return false;
				
			// gridNodes里面的Node数组是从0，0开始的，所以要减去原点这个偏移量
			startNode = gridNodes.GetGridNode(startPos.x - originX ,startPos.y - originY);
			targetNode = gridNodes.GetGridNode(endPos.x - originX ,endPos.y - originY);
			
			// 判断该网格是否是障碍
			for(int x = 0;x < gridWidth; x++)
			{
				for(int y = 0;y < gridHeight; y++)
				{
					string key = (x + originX) + "x" + (y + originY) + "y" + sceneName;
					TileDetails tile = GridManager.Instance.GetTileDetails(key);
					
					if(tile!=null)
					{
						Node node = gridNodes.GetGridNode(x,y);
						
						if(tile.isNPCObstacle)
							node.isObstacle = true;
					}
				}
			}
			
			return true;
		}
	
		
		/// <summary>
		/// 寻找最近的路径
		/// </summary>
		/// <returns></returns>
		private bool FindShortestPath()
		{
			// 添加起点
			openNodeList.Add(startNode);
			
			while(openNodeList.Count>0)
			{
				// 节点进行排序，Node内涵比较函数
				openNodeList.Sort();
				
				// 找到最近的一个
				Node closeNode = openNodeList[0];	
				
				openNodeList.RemoveAt(0);
				closedNodelist.Add(closeNode);
				
				if(closeNode == targetNode)
				{
					PathFound = true;
					break;
				}
				
				// 循坏计算周围的八个Node并添加到OpenNodeList中
				EvaluateNeighbourNodes(closeNode);
			}
			
			return PathFound;
		}
		
		/// <summary>
		/// 评估周围八个点，并生成对应的gcost,hcost,fcost
		/// </summary>
		/// <param name="currentNode"></param>
		private void EvaluateNeighbourNodes(Node currentNode)
		{
			Vector2Int currentNodePos = currentNode.gridPosition;
			// 可行的，该Node不是障碍
			Node validNeighbourNode;
			
			for(int x = -1;x<=1;x++)
			{
				for(int y = -1;y<=1;y++)
				{
					if(x == 0 && y == 0)
					{
						continue;
					}
					validNeighbourNode = GetValidNeighbourNode(currentNodePos.x + x,currentNodePos.y + y);
					
					if(validNeighbourNode != null)
					{
						if(!openNodeList.Contains(validNeighbourNode))
						{
							validNeighbourNode.gCost = currentNode.gCost + GetDistance(currentNode,validNeighbourNode);
							validNeighbourNode.hCost = GetDistance(targetNode,validNeighbourNode);
							
							// 链接父节点
							validNeighbourNode.parentNode = currentNode;
							openNodeList.Add(validNeighbourNode);
						}
					}
				}
			}
		}
		
		/// <summary>
		/// 返回两点距离值
		/// </summary>
		/// <param name="nodeA"></param>
		/// <param name="nodeB"></param>
		/// <returns>返回两点距离值</returns>
		private int GetDistance(Node nodeA,Node nodeB)
		{
			int xDistance = Mathf.Abs(nodeA.gridPosition.x - nodeB.gridPosition.x);
			int yDistance = Mathf.Abs(nodeA.gridPosition.y - nodeB.gridPosition.y);
			
			
			if (xDistance > yDistance)
			{
				return 14 * yDistance + 10 * (xDistance - yDistance);
			}
			return 14 * xDistance + 10 * (yDistance - xDistance);
		}
		
		/// <summary>
		/// 找到有效的Node,非障碍，非已选择
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns>有效的Node</returns>
		private Node GetValidNeighbourNode(int x,int y)
		{
			if(x >= gridWidth || y >= gridHeight || x < 0 || y < 0)
				return null;
			
			Node neighbourNode = gridNodes.GetGridNode(x,y);
			
			if(neighbourNode.isObstacle || closedNodelist.Contains(neighbourNode))
				return null;
			else
				return neighbourNode;
		}
		
		/// <summary>
		/// 更新路径每一步的坐标和场景名字
		/// </summary>
		/// <param name="sceneName"></param>
		/// <param name="npcMovementStep"></param>
		private void UpdatePathMovementStepStack(string sceneName,Stack<MovementStep> npcMovementStep)
		{
			Node nextNode = targetNode;
			
			while(nextNode!=null)
			{
				MovementStep newStep = new MovementStep();
				newStep.sceneName = sceneName;
				newStep.gridCoordinate = new Vector2Int(nextNode.gridPosition.x + originX,nextNode.gridPosition.y + originY);
				
				//Debug.Log("newStep::"+newStep.sceneName);
				//压入堆栈
				npcMovementStep.Push(newStep);
				nextNode = nextNode.parentNode;
			}
		}
	}
}
