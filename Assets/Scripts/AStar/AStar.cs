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
		
		private List<Node> openNodeList;			// ��ǰѡ�е�Node����Χ��8��Node
		private HashSet<Node> closedNodelist;		// ���б�ѡ�е������
		
		private bool PathFound;
		
		/// <summary>
		/// ����·������stack�е�ÿһ��
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
				// �������·��
				if(FindShortestPath())
				{
					// ����NPC�ƶ�·��
					UpdatePathMovementStepStack(sceneName,npcMovementStep);
				}
				
			}
		}
		
		/// <summary>
		/// ��������ڵ���Ϣ����ʼ�������б�
		/// </summary>
		/// <param name="sceneName">����������</param>
		/// <param name="startPos">��ʼ�������</param>
		/// <param name="endPos">Ŀ���</param>
		/// <returns></returns>
		private bool GenerateGridNodes(string sceneName,Vector2Int startPos,Vector2Int endPos)
		{
			// ����õ�ͼ���ڵĻ�����ʼ���ҵ������е���Ϣ
			if(GridManager.Instance.GetGridDimensions(sceneName,out Vector2Int gridDimensions,out Vector2Int gridOrigin))
			{
				// ������Ƭ��ͼ��Χ���������ƶ��ڵ㷶Χ����
				gridNodes = new GridNodes(gridDimensions.x,gridDimensions.y);
				gridWidth = gridDimensions.x;
				gridHeight = gridDimensions.y;
				originX = gridOrigin.x;
				originY = gridOrigin.y;
				
				// �б��ʼ��
				openNodeList = new List<Node>();
				
				closedNodelist = new HashSet<Node>();		
			}else
				return false;
				
			// gridNodes�����Node�����Ǵ�0��0��ʼ�ģ�����Ҫ��ȥԭ�����ƫ����
			startNode = gridNodes.GetGridNode(startPos.x - originX ,startPos.y - originY);
			targetNode = gridNodes.GetGridNode(endPos.x - originX ,endPos.y - originY);
			
			// �жϸ������Ƿ����ϰ�
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
		/// Ѱ�������·��
		/// </summary>
		/// <returns></returns>
		private bool FindShortestPath()
		{
			// ������
			openNodeList.Add(startNode);
			
			while(openNodeList.Count>0)
			{
				// �ڵ��������Node�ں��ȽϺ���
				openNodeList.Sort();
				
				// �ҵ������һ��
				Node closeNode = openNodeList[0];	
				
				openNodeList.RemoveAt(0);
				closedNodelist.Add(closeNode);
				
				if(closeNode == targetNode)
				{
					PathFound = true;
					break;
				}
				
				// ѭ��������Χ�İ˸�Node����ӵ�OpenNodeList��
				EvaluateNeighbourNodes(closeNode);
			}
			
			return PathFound;
		}
		
		/// <summary>
		/// ������Χ�˸��㣬�����ɶ�Ӧ��gcost,hcost,fcost
		/// </summary>
		/// <param name="currentNode"></param>
		private void EvaluateNeighbourNodes(Node currentNode)
		{
			Vector2Int currentNodePos = currentNode.gridPosition;
			// ���еģ���Node�����ϰ�
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
							
							// ���Ӹ��ڵ�
							validNeighbourNode.parentNode = currentNode;
							openNodeList.Add(validNeighbourNode);
						}
					}
				}
			}
		}
		
		/// <summary>
		/// �����������ֵ
		/// </summary>
		/// <param name="nodeA"></param>
		/// <param name="nodeB"></param>
		/// <returns>�����������ֵ</returns>
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
		/// �ҵ���Ч��Node,���ϰ�������ѡ��
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns>��Ч��Node</returns>
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
		/// ����·��ÿһ��������ͳ�������
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
				//ѹ���ջ
				npcMovementStep.Push(newStep);
				nextNode = nextNode.parentNode;
			}
		}
	}
}
