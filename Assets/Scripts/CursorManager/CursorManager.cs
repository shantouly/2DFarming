using System.Collections;
using System.Collections.Generic;
using Fram.CropPlate;
using Fram.Inventory;
using Fram.Map;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CursorManager : MonoBehaviour
{
	public Sprite normal, tool, seed, item;

	private Sprite currentSprite;

	private Image cursorImage;
	private Image buildImage;
	private RectTransform cursorCanvas;

	// �����
	private Camera mainCamera;
	private Grid currentGrid;
	private Vector3 mouseWorldPos;
	private Vector3Int mouseGridPos;
	private bool cursorEnable;                              // �����ж�����Ƿ����ʹ��
	private bool cursorPositionValid;                       // �����ж�����ڸ��������Ƿ����ִ��
	private ItemDetails currentItem;
	private Transform player => FindObjectOfType<Player>().transform;

	private void OnEnable()
	{
		EventHandler.ItemSelectedEvent += OnItemSelectedEvent;
		EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
		EventHandler.AfterSceneUnloadEvent += OnAfterSceneUnloadEvent;
	}

	private void Start()
	{
		cursorCanvas = GameObject.FindWithTag("CursorCanvas").GetComponent<RectTransform>();
		cursorImage = cursorCanvas.GetChild(0).GetComponent<Image>();
		buildImage = cursorCanvas.GetChild(1).GetComponent<Image>();
		currentSprite = normal;
		SetCursorImage(normal);

		mainCamera = Camera.main;
	}

	private void OnDisable()
	{
		EventHandler.ItemSelectedEvent -= OnItemSelectedEvent;
		EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
		EventHandler.AfterSceneUnloadEvent -= OnAfterSceneUnloadEvent;
	}

	private void Update()
	{
		if (cursorCanvas == null) return;
		cursorImage.transform.position = Input.mousePosition;
		
		// ���ҵ����action Bar�����item��ʱ�򣬲�ִ��������ķ���
		if(!InteractWithUI() && cursorEnable)
		{
			SetCursorImage(currentSprite);
			CheckCursorValid();
			CheckPlayerInput();
		}
		else
		{
			SetCursorImage(normal);
			buildImage.gameObject.SetActive(false);
		}
	}

	/// <summary>
	/// �������Ƿ������������Ҹ������λ���Ƿ���Ч
	/// </summary>
	private void CheckPlayerInput()
	{
		if(Input.GetMouseButtonDown(0) && cursorPositionValid)
		{
			// ִ�з���
			EventHandler.CallMouseClickEvent(mouseWorldPos, currentItem);
		}
	}

	private void OnBeforeSceneUnloadEvent()
	{
		cursorEnable = false;
	}

	private void OnAfterSceneUnloadEvent()
	{
		currentGrid = FindObjectOfType<Grid>();
	}

	/// <summary>
	/// ��������ͼƬ
	/// </summary>
	/// <param name="sprite">
	/// </param>
	private void SetCursorImage(Sprite sprite)
	{
		cursorImage.sprite = sprite;
		cursorImage.color = new Color(1, 1, 1, 1);
	}

	/// <summary>
	/// ����������
	/// </summary>
	private void SetCursorValid()
	{
		cursorPositionValid = true;
		cursorImage.color = new Color(1, 1, 1, 1);
		buildImage.color = new Color(1, 1, 1, 0.5f);
	}

	/// <summary>
	/// ������겻����
	/// </summary>
	private void SetCursorInvalid()
	{
		cursorPositionValid = false;
		cursorImage.color = new Color(1, 0, 0, 4f);
		buildImage.color = new Color(1, 0, 0, 1);
	}

	/// <summary>
	/// ����ѡ�񱳰���action Bar�������Ʒʱ��ָ��ҲҪ��Ӧ�Ľ��б仯
	/// </summary>
	/// <param name="itemDetails">��Ʒ����Ϣ</param>
	/// <param name="isSelected">�Ƿ�ѡ��</param>
	private void OnItemSelectedEvent(ItemDetails itemDetails, bool isSelected)
	{
		if(isSelected == false)
		{
			currentItem = null;
			cursorEnable = false;
			currentSprite = normal;
		}
		else
		{
			currentItem = itemDetails;
			currentSprite = itemDetails.itemtype switch
			{
				itemType.Seed => seed,
				itemType.Commodity => item,
				itemType.ChopTool => tool,
				itemType.HoeTool => tool,
				itemType.WaterTool => tool,
				itemType.BreakTool => tool,
				itemType.ReapTool => tool,
				itemType.Furniture => tool,
				itemType.CollectTool => tool,
				_ => normal
			};
			cursorEnable = true;
			
			if(itemDetails.itemtype == itemType.Furniture)
			{
				buildImage.gameObject.SetActive(true);
				buildImage.sprite = itemDetails.itemOnWorldSprite;
			}
		}
	}

	private void CheckCursorValid()
	{
		mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCamera.transform.position.z));
		mouseGridPos = currentGrid.WorldToCell(mouseWorldPos);

		var playerGridPos = currentGrid.WorldToCell(player.position);
		
		// ����ͼƬ��������ƶ�
		buildImage.rectTransform.position = Input.mousePosition;
		if(Mathf.Abs(playerGridPos.x - mouseGridPos.x) > currentItem.itemUseRadius || Mathf.Abs(playerGridPos.y - mouseGridPos.y) > currentItem.itemUseRadius)
		{
			SetCursorInvalid();
			return;
		}
		//Debug.Log(mouseWorldPos + ".........." + mouseGridPos);
		TileDetails currentTile = GridManager.Instance.GetTileDetailsOnMousePosition(mouseGridPos);

		if (currentTile != null)
		{
			CropDetails currentCrop = CropManager.Instance.GetCropDetails(currentTile.seedItemID);
			Crop crop = GridManager.Instance.GetCropObject(mouseWorldPos);
			switch (currentItem.itemtype)
			{
				case itemType.Seed:
					if(currentTile.daysSinceDug>-1 && currentTile.seedItemID==-1) SetCursorValid();else SetCursorInvalid();
					break;
				case itemType.Commodity:
					if (currentTile.canDropItem && currentItem.canDropped) SetCursorValid(); else SetCursorInvalid();
					break;
				case itemType.HoeTool:
					if (currentTile.canDig) SetCursorValid(); else SetCursorInvalid();
					break;
				case itemType.WaterTool:
					if (currentTile.daysSinceDug > -1 && currentTile.daysSinceWatered == -1) SetCursorValid(); else SetCursorInvalid();
					break;
				case itemType.BreakTool:
				case itemType.ChopTool:
				if(crop!=null)
				{
					if(crop.canHarvest&&crop.cropDetails.CheckToolAvailable(currentItem.itemID))
					SetCursorValid(); else SetCursorInvalid();
				}else
				{
					SetCursorInvalid();
				}
					break;
				case itemType.CollectTool:
					if(currentCrop!=null)
					{
						if(currentCrop.CheckToolAvailable(currentItem.itemID))
						{
							if(currentTile.growthDays >= currentCrop.TotalGrowthDays) SetCursorValid();else SetCursorInvalid();
						}
					}else
					{
						SetCursorInvalid();
					}
					break;		
				case itemType.ReapTool:
					if(GridManager.Instance.HaveReapableItemsInRadius(mouseWorldPos,currentItem)) SetCursorValid();else SetCursorInvalid();
					break;	
				case itemType.Furniture:
					buildImage.gameObject.SetActive(true);
					var bluePrintDetails = InventoryManager.Instance.bluePrintDataList_SO.GetBluePrintDetails(currentItem.itemID);
					if(currentTile.canPlaceFurniture && InventoryManager.Instance.CheckStock(currentItem.itemID) && !HaveFurnitureInRaidus(bluePrintDetails)) 
						SetCursorValid();
					else 
						SetCursorInvalid();
					break;			
			}
		}
		else
		{
			SetCursorInvalid();
		}
	}
	
	private bool HaveFurnitureInRaidus(BluePrintDetails bluePrintDetails)
	{
		var buildItem = bluePrintDetails.buildPrefab;
		Vector2 point = mouseWorldPos;
		var size = buildItem.GetComponent<BoxCollider2D>().size;
		
		var collider = Physics2D.OverlapBox(point,size,0);
		if(collider != null)
			return collider.GetComponent<Furniture>();
		else
			return false;
	}

	/// <summary>
	/// �ж�����Ƿ���UI�����˻���������Ļ���ָ�����Ƿ���ͣ����뵽UI��
	/// </summary>
	/// <returns></returns>
	private bool InteractWithUI()
	{
		if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
		{
			return true;
		}
		else
		{
			return false;
		}
	}
}
