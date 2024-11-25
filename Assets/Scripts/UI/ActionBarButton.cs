using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fram.Inventory
{
	[RequireComponent(typeof(SlotUI))]
	public class ActionBarButton : MonoBehaviour
	{
		public KeyCode key;
		private SlotUI slotUI;
		private bool canUse;
		void Awake()
		{
			slotUI = GetComponent<SlotUI>();
		}
		
		void OnEnable()
		{
			EventHandler.UpdateGameStateEvent += OnUpdateGameStateEvent;
			EventHandler.CallUpdateGameStateEvent(GameState.GamePlay);
		}
		
		void OnDisable()
		{
			EventHandler.UpdateGameStateEvent -= OnUpdateGameStateEvent;
		}

		private void OnUpdateGameStateEvent(GameState state)
		{
			canUse = state == GameState.GamePlay;
		}

		void Update()
		{
			if(Input.GetKeyDown(key) && canUse)
			{
				if(slotUI.itemDetails!=null)
				{
					slotUI.isSelected = !slotUI.isSelected;
					if(slotUI.isSelected)
					{
						slotUI.inventoryUI.UpdateSlotHighLight(slotUI.slotIndex);
					}else
					{
						slotUI.inventoryUI.UpdateSlotHighLight(-1);
					}
					
					EventHandler.CallItemSelectedEvent(slotUI.itemDetails,slotUI.isSelected);
				}
			}
		}
	}
}

