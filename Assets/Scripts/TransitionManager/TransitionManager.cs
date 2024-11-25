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
			
		// 	// ���Ҳ�Ǽ��س�����ҲҪ���ü��س���֮����¼���ͬʱҲ����Ҫ��ȡ�߽�
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
		/// ����ʵ�ֳ������л�����
		/// </summary>
		/// <param name="sceneName">Ŀ�곡��������</param>
		/// <param name="targetPosition">Ŀ���λ��</param>
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
		/// ���س�ʼ�ĳ���������Ϊ����״̬
		/// </summary>
		/// <param name="sceneName">��ʼ����������</param>
		/// <returns></returns>
		private IEnumerator LoadSceneActive(string sceneName)
		{
			yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

			// ���ﵱ���ص�ʱ����صĳ������������Ѽ��صĳ�����ĩβ������ֱ��count - 1����
			Scene newScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

			SceneManager.SetActiveScene(newScene);
		}

		/// <summary>
		/// �����л��ĵ��뵭��
		/// </summary>
		/// <param name="targetAlpha">Ŀ��͸����</param>
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
			// ������Ϸ���ȳ���
			StartCoroutine(LoadSaveDataScene(saveData.dataSceneName));
		}
	}
}

