using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCFunction : MonoBehaviour
{
	public IventoryBag_SO shopData;
	private bool isOpen;
	[SerializeField] private Button closeBtn;
	
	// void Awake()
	// {
	// 	GameObject btn = GameObject.Find("Esc");
	// 	Debug.Log(btn.name);
	// 	closeBtn = btn.GetComponent<Button>();
	// 	closeBtn.onClick.AddListener(CloseBag);
	// }
	
	void Update()
	{
		if(isOpen && Input.GetKeyDown(KeyCode.Escape))
		{
			// ¹Ø±Õ±³°ü
			CloseBag();
		}
	}
	
	public void OpenBag()
	{
		isOpen = true;
		
		EventHandler.CallBaseBagOpenEvent(slotType.Shop,shopData);
		EventHandler.CallUpdateGameStateEvent(GameState.GamePause);
	}
	
	public void CloseBag()
	{
		isOpen = false;
		
		EventHandler.CallBaseBagCloseEvent(slotType.Shop,shopData);
		EventHandler.CallUpdateGameStateEvent(GameState.GamePlay);
	}
}
