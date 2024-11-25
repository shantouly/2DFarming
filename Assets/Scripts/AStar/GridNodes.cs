using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fram.AStar
{
	public class GridNodes
	{
		private int width;
		private int height;
		private Node[,] gridNode;

		/// <summary>
		/// ���캯����ʼ���ڵ㷶Χ����
		/// </summary>
		/// <param name="width">��ͼ�Ŀ�</param>
		/// <param name="height">��ͼ�ĳ�</param>
		public GridNodes(int width,int height)
		{
			this.width = width;
			this.height = height;
			
			gridNode = new Node[width,height];
			
			for(int x = 0;x < width;x++)
			{
				for(int y = 0;y<height;y++)
				{
					gridNode[x,y] = new Node(new Vector2Int(x,y));
				}
			}
		}
		
		public Node GetGridNode(int xPos,int yPox)
		{
			if(xPos < width && yPox < height)
			{
				return gridNode[xPos,yPox];
			}
			
			Debug.Log("��������Χ");
			return null;
		}
	}
}

