using System.Collections;
using System.Collections.Generic;
using Fram.Save;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fram.Transition
{
	public class TransitionManager : Singleton<TransitionManager>,ISaveable
	{
		[SceneName]
		public string startSceneName = string.Empty;

		private CanvasGroup fadeCanvasGroup;
		private bool isFade = true;

		public string GUID => GetComponent<DataGUID>().guid;

		protected override void Awake()
		{
			base.Awake();
			
			SceneManager.LoadScene("UI",LoadSceneMode.Additive);
		}

		// private IEnumerator Start()
		// {         
		// 	ISaveable saveable = this;
		// 	saveable.RegisterSaveable();
			
		// 	// 这个也是加载场景，也要调用加载场景之后的事件。同时也是需要获取边界
		// 	fadeCanvasGroup = FindObjectOfType<CanvasGroup>();
		// 	yield return LoadSceneActive(startSceneName);
		// 	EventHandler.CallAfterSceneUnloadEvent();
		// }
		
		private void Start() {
			ISaveable saveable = this;
			saveable.RegisterSaveable();
			
			fadeCanvasGroup = FindObjectOfType<CanvasGroup>();
		}

		private void OnEnable()
		{
			EventHandler.TransitionEvent += OnTransitionEvent;
			EventHandler.StartNewGameEvent += OnStartNewGameEvent;
			EventHandler.EndGameEvent += OnEndGameEvent;
		}


		private void OnDisable()
		{
			EventHandler.TransitionEvent -= OnTransitionEvent;
			EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
			EventHandler.EndGameEvent -= OnEndGameEvent;
		}


		private void OnStartNewGameEvent(int obj)
		{
			StartCoroutine(LoadSaveDataScene(startSceneName));
		}

		private void OnEndGameEvent()
		{
			StartCoroutine(UnLoadScene());
		}
		
		private void OnTransitionEvent(string sceneName, Vector3 targetPos)
		{
			if(isFade)
				StartCoroutine(Transition(sceneName, targetPos));
		}
		
		private IEnumerator UnLoadScene()
		{
			EventHandler.CallBeforeSceneUnloadEvent();
			yield return Fade(1f);
			yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
			yield return Fade(0);
		}

		/// <summary>
		/// 用于实现场景的切换功能
		/// </summary>
		/// <param name="sceneName">目标场景的名字</param>
		/// <param name="targetPosition">目标的位置</param>
		/// <returns></returns>
		private IEnumerator Transition(string sceneName,Vector3 targetPosition)
		{
			EventHandler.CallBeforeSceneUnloadEvent();
			yield return Fade(1);
			yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());

			yield return LoadSceneActive(sceneName);

			EventHandler.CallMoveToPosition(targetPosition);

			EventHandler.CallAfterSceneUnloadEvent();
			yield return Fade(0);
		}

		/// <summary>
		/// 加载初始的场景并设置为激活状态
		/// </summary>
		/// <param name="sceneName">初始场景的名字</param>
		/// <returns></returns>
		private IEnumerator LoadSceneActive(string sceneName)
		{
			yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

			// 这里当加载的时候加载的场景是在所有已加载的场景的末尾，所以直接count - 1即可
			Scene newScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

			SceneManager.SetActiveScene(newScene);
		}

		/// <summary>
		/// 场景切换的淡入淡出
		/// </summary>
		/// <param name="targetAlpha">目标透明度</param>
		/// <returns></returns>
		public IEnumerator Fade(float targetAlpha)
		{
			isFade = false;
			fadeCanvasGroup.blocksRaycasts = true;

			float speed = Mathf.Abs(fadeCanvasGroup.alpha - targetAlpha) / Settings.fadeAlpha;

			while (!Mathf.Approximately(fadeCanvasGroup.alpha, targetAlpha))
			{
				fadeCanvasGroup.alpha = Mathf.MoveTowards(fadeCanvasGroup.alpha, targetAlpha, speed * Time.deltaTime);
				yield return null;
			}

			isFade = true;
			fadeCanvasGroup.blocksRaycasts = false;
		}
		
		private IEnumerator LoadSaveDataScene(string sceneName)
		{
			yield return Fade(1f);
			
			if(SceneManager.GetActiveScene().name != "PersistentScene")
			{
				EventHandler.CallBeforeSceneUnloadEvent();
				yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
			}
			
			yield return LoadSceneActive(sceneName);
			EventHandler.CallAfterSceneUnloadEvent();
			yield return Fade(0);
		}

		public GameSaveData gameSaveData()
		{
			GameSaveData saveData = new GameSaveData();
			saveData.dataSceneName = SceneManager.GetActiveScene().name;
			
			return saveData;
		}

		public void RestoreData(GameSaveData saveData)
		{
			// 加载游戏进度场景
			StartCoroutine(LoadSaveDataScene(saveData.dataSceneName));
		}
	}
}

