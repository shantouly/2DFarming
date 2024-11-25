using System.Collections;
using System.Collections.Generic;
using Fram.Transition;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	private GameObject UICanvas;
	public GameObject menuPrefab;
	public Button settingsBtn;
	public GameObject pausePanel;
	public Slider slider;
	void OnEnable()
	{
		EventHandler.AfterSceneUnloadEvent += OnAfterSceneUnloadEvent;
	}
	
	void OnDisable()
	{
		EventHandler.AfterSceneUnloadEvent -= OnAfterSceneUnloadEvent;
	}
	
	
	void Start()
	{
		UICanvas = GameObject.FindWithTag("UIManager");
		Instantiate(menuPrefab,UICanvas.transform);
		settingsBtn.onClick.AddListener(TogglePausePanel);
		slider.onValueChanged.AddListener(AudioManager.Instance.SetMasterVolume);
	}

	private void OnAfterSceneUnloadEvent()
	{
		if(UICanvas.transform.childCount > 0)
		{
			Destroy(UICanvas.transform.GetChild(0).gameObject);
		}
	}
	
	private void TogglePausePanel()
	{
		bool isOpen = pausePanel.activeInHierarchy;
		
		if(isOpen)
		{
			pausePanel.SetActive(false);
			Time.timeScale = 1;
		}else
		{
			System.GC.Collect();
			pausePanel.SetActive(true);
			Time.timeScale = 0;
		}
	}
	
	/// <summary>
	/// 返回主菜单的按钮触发的事件
	/// </summary>
	public void ReturnMenuCanvas()
	{
		Time.timeScale = 1;
		StartCoroutine(BackToMenu());
	}
	
	private IEnumerator BackToMenu()
	{
		pausePanel.SetActive(false);
		// yield return TransitionManager.Instance.Fade(1f);
		// yield return new WaitForSeconds(0.5f);
		// Instantiate(menuPrefab,UICanvas.transform);
		// yield return TransitionManager.Instance.Fade(0);
		EventHandler.CallEndGameEvent();
		yield return null;
		Instantiate(menuPrefab,UICanvas.transform);
	}
	
}
