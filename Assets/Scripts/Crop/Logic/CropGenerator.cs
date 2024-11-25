using System.Collections;
using System.Collections.Generic;
using Fram.Map;
using UnityEngine;

public class CropGenerator : MonoBehaviour
{
	private Grid currentGrid;
	public int seedItemID;
	public int growthDays;
	
	void Awake()
	{
		currentGrid = FindObjectOfType<Grid>();
	}
	
	void OnEnable()
	{
		EventHandler.GenerateCropEvent += GenerateCrop;
	}
	
	void OnDisable()
	{
		EventHandler.GenerateCropEvent -= GenerateCrop;
	}
	
	/// <summary>
	/// Ԥ������Crop,����tileDetails�������Ϣ
	/// </summary>
	private void GenerateCrop()
	{
		Vector3Int cropGridPos = currentGrid.WorldToCell(transform.position);
		
		if(seedItemID != 0) 
		{
			TileDetails tile = GridManager.Instance.GetTileDetailsOnMousePosition(cropGridPos);
			
			if(tile == null)
			{
				tile = new TileDetails();
				tile.gridX = cropGridPos.x;
				tile.gridY = cropGridPos.y;
			}
			
			tile.daysSinceWatered = -1;
			tile.seedItemID = seedItemID;
			tile.growthDays = growthDays;
			
			GridManager.Instance.UpdateTileDetails(tile);
		}
	}
}
