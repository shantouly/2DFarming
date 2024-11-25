using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemDetails
{
	public int itemID;
	public string ItemName;
	public itemType itemtype;

	public Sprite itemIcon;
	public Sprite itemOnWorldSprite;
	public string itemDescription;
	public int itemUseRadius;
	public bool canPickUp;
	public bool canDropped;
	public bool canCarried;
	public int itemPrice;

	[Range(0,1)]
	public float sellPercentage;
}

/// <summary>
/// �����Ҫ����Ա��������item����Ϊ��ҪID��Amount������ʾ����
/// </summary>
[System.Serializable]
public struct InventoryItem
{
	public int itemID;
	public int itemAmount;
}

[System.Serializable]
public class AnimatorType
{
	public PartType partType;
	public PartName partName;
	public AnimatorOverrideController overrideController;
}

[System.Serializable]
public class SerializableVector3
{
	public float x, y, z;

	public SerializableVector3(Vector3 pos)
	{
		x = pos.x;
		y = pos.y;
		z = pos.z;
	}

	public Vector3 ToVector3()
	{
		return new Vector3(x, y, z);
	}

	public Vector2Int ToVector2Int()

	{
		return new Vector2Int((int)x, (int)y);
	}
}

[System.Serializable]
public class SceneItem
{
	public int itemID;
	public SerializableVector3 position;
}

[System.Serializable]
public class SceneFurniture
{
	public int furnitureID;
	public SerializableVector3 position;
	public int boxIndex;
}

/// <summary>
/// �����ǵ�ͼ������Ҫ�����ԣ�������������꣬���������������
/// </summary>
[System.Serializable]
public class TileProperty
{
	public Vector2Int tileCordinate;
	public bool boolTypeValue;
	public GridType gridType;
}

[System.Serializable]
public class TileDetails
{
	public int gridX, gridY;                        // �����������
	// Type
	public bool canDig;
	public bool canDropItem;
	public bool canPlaceFurniture;
	public bool isNPCObstacle;

	public int daysSinceDug = -1;                   // �ϴ�����dig������
	public int daysSinceWatered = -1;               // �ϴ�����ˮ������
	public int seedItemID = -1;                     // ���ӵ�ID
	public int growthDays = -1;                     // ���������˶�����
	public int daysSinceLastHarvest = -1;           // �ϴ������ջ������
}
[System.Serializable]
public class NPCPosition
{
	public Transform npc;
	public string startScene;
	public Vector3 position;
}

[System.Serializable]
public class SceneRoute
{
	public string fromSceneName;
	public string gotoSceneName;
	public List<ScenePath> scenePathList;
}

[System.Serializable]
public class ScenePath
{
	public string sceneName;
	public Vector2Int fromGridCell;
	public Vector2Int gotoGridCell;
}