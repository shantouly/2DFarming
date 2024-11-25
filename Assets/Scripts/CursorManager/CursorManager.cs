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

	// 鼠标检测
	private Camera mainCamera;
	private Grid currentGrid;
	private Vector3 mouseWorldPos;
	private Vector3Int mouseGridPos;
	private bool cursorEnable;                              // 用于判断鼠标是否可以使用
	private bool cursorPositionValid;                       // 用于判断鼠标在该网格下是否可以执行
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
		
		// 当我点击了action Bar里面的item的时候，才执行这下面的方法
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
	/// 检查玩家是否按下鼠标左键并且该网格的位置是否有效
	/// </summary>
	private void CheckPlayerInput()
	{
		if(Input.GetMouseButtonDown(0) && cursorPositionValid)
		{
			// 执行方法
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
	/// 设置鼠标的图片
	/// </summary>
	/// <param name="sprite">
	/// </param>
	private void SetCursorImage(Sprite sprite)
	{
		cursorImage.sprite = sprite;
		cursorImage.color = new Color(1, 1, 1, 1);
	}

	/// <summary>
	/// 设置鼠标可用
	/// </summary>
	private void SetCursorValid()
	{
		cursorPositionValid = true;
		cursorImage.color = new Color(1, 1, 1, 1);
		buildImage.color = new Color(1, 1, 1, 0.5f);
	}

	/// <summary>
	/// 设置鼠标不可用
	/// </summary>
	private void SetCursorInvalid()
	{
		cursorPositionValid = false;
		cursorImage.color = new Color(1, 0, 0, 4f);
		buildImage.color = new Color(1, 0, 0, 1);
	}

	/// <summary>
	/// 当我选择背包或action Bar里面的物品时，指针也要相应的进行变化
	/// </summary>
	/// <param name="itemDetails">物品的信息</param>
	/// <param name="isSelected">是否被选中</param>
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
		
		// 建造图片跟随鼠标移动
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
	/// 判断鼠标是否与UI进行了互动，这里的互动指的是是否悬停或进入到UI中
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
