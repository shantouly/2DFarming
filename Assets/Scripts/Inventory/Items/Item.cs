using System.Collections;
using System.Collections.Generic;
using Fram.CropPlate;
using UnityEngine;

namespace Fram.Inventory
{
	public class Item : MonoBehaviour
	{
		public int itemID;
		public ItemDetails itemDetails;
		private SpriteRenderer spriteRenderer;
		private new BoxCollider2D collider;

		private void Awake()
		{
			spriteRenderer = GetComponentInChildren<SpriteRenderer>();
			collider = GetComponent<BoxCollider2D>();
		}

		private void Start()
		{
			if (itemID != 0)
			{
				Init(itemID);
			}
		}

		public void Init(int ID)
		{
			itemID = ID;
			// Inventory��ȡ��ǰ����
			itemDetails = InventoryManager.Instance.GetItemDetails(itemID);

			if (itemDetails != null)
			{
				spriteRenderer.sprite = itemDetails.itemOnWorldSprite == null ? itemDetails.itemIcon : itemDetails.itemOnWorldSprite;
				itemDetails.itemOnWorldSprite = itemDetails.itemOnWorldSprite == null ? itemDetails.itemIcon : itemDetails.itemOnWorldSprite;
				// �����޸�Collider�е�offset����Ϊ��Щ�趨�������ӣ���ê�������Ϊbottom��Ҫ��offset�޸�
				Vector2 newSize = new Vector2(spriteRenderer.sprite.bounds.size.x, spriteRenderer.sprite.bounds.size.y);
				collider.size = newSize;
				collider.offset = new Vector2(0, spriteRenderer.sprite.bounds.center.y);
			}
			
			// ������ӲݵĻ����������ű�
			if(itemDetails.itemtype == itemType.ReapableScenery)
			{
				gameObject.AddComponent<ReapItem>();
				gameObject.GetComponent<ReapItem>().InitCropData(itemDetails.itemID);
				gameObject.AddComponent<ItemInteractive>();
			}
		}
	}
}
