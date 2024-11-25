using System;
using System.Collections;
using System.Collections.Generic;
using Fram.AStar;
using Fram.Save;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class NPCMovement : MonoBehaviour,ISaveable
{
	public ScheduleDataList_SO scheduleData;					// 里面存放着scheduleDetails
	private SortedSet<ScheduleDetails> scheduleSet;				// 对我Details里面的优先级进行排序
	private ScheduleDetails currentScheduleDetails;
	[SerializeField]
	public string currentScene;
	private string targetScene;
	private Vector3Int currentGridPos;
	private Vector3Int targetGridPos;
	private Vector3Int nextGridPos;
	private Vector3 nextWorldPos;
	
	public string StartScene{ set => currentScene = value;}
	
	[Header("移动属性")]
	public float normalSpeed = 2f;
	private float minSpeed = 1;
	private float maxSpeed = 3;
	private Vector2 dir;
	public bool isMoving;
	public bool interactable;
	
	private Rigidbody2D rb;
	private SpriteRenderer spriteRenderer;
	private BoxCollider2D coll;
	private Animator anim;
	private Grid grid;
	private Season currentSeason;
	private bool isInitialized;
	private bool npcMove;
	private bool sceneLoaded;
	private bool isFirstLoad;
	
	
	// 动画计时器
	private float animationBreakTime;
	private bool canPlayStopAnimation;
	private AnimationClip stopAnimationClip;
	public AnimationClip blankAnimationClip;		
	private AnimatorOverrideController animOverride;
	
	private Stack<MovementStep> movementSteps;
	private TimeSpan GameTime => TimeManager.Instance.GameTime;

	public string GUID => GetComponent<DataGUID>().guid;
	private Coroutine npcMoveRoutine;

	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		coll = GetComponent<BoxCollider2D>();
		anim = GetComponent<Animator>();
		grid = FindObjectOfType<Grid>();
		movementSteps = new Stack<MovementStep>();
		scheduleSet = new SortedSet<ScheduleDetails>();
		
		animOverride = new AnimatorOverrideController(anim.runtimeAnimatorController);
		anim.runtimeAnimatorController = animOverride;
		
		// 初始化ScheduleSet
		foreach(var schedule in scheduleData.scheduleList)
		{
			scheduleSet.Add(schedule);
		}
	}
	
	void Start()
	{
		ISaveable saveable = this;
		saveable.RegisterSaveable();
	}
	
	void OnEnable()
	{
		EventHandler.AfterSceneUnloadEvent += OnAfterSceneUnloadEvent;
		EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
		EventHandler.GameMinuteEvent += OnGameMinuteEvent;
		EventHandler.StartNewGameEvent += OnStartNewGameEvent;
		EventHandler.EndGameEvent += OnEndGameEvent;
	}

	void OnDisable()
	{
		EventHandler.AfterSceneUnloadEvent -= OnAfterSceneUnloadEvent;
		EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
		EventHandler.GameMinuteEvent -= OnGameMinuteEvent;
		EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
		EventHandler.EndGameEvent -= OnEndGameEvent;
	}


	void Update()
	{
		if(sceneLoaded)
			SwitchAnimation();
			
		// 计时器
		animationBreakTime -= Time.deltaTime;
		canPlayStopAnimation = animationBreakTime < 0;
	}

	void FixedUpdate()
	{
		if(sceneLoaded)
		{
			Movement();
		}
	}
	
	private void OnStartNewGameEvent(int obj)
	{
		isInitialized = false;
		isFirstLoad = true;
	}
	
	private void OnBeforeSceneUnloadEvent()
	{
		sceneLoaded = false;
	}
	
	private void OnAfterSceneUnloadEvent()
	{
		CheckVisible();
		
		if(!isInitialized)
		{
			InitNPC();
			isInitialized = true;
		}
		
		sceneLoaded = true;
		if(!isFirstLoad)
		{
			currentGridPos = grid.WorldToCell(transform.position);
			var schedule = new ScheduleDetails(0,0,0,0,currentSeason,targetScene,(Vector2Int)targetGridPos,stopAnimationClip,interactable);
			BuildPath(schedule);
			isFirstLoad = true;
		}
	}
	
	private void OnEndGameEvent()
	{
		sceneLoaded = false;
		npcMove = false;
		if(npcMoveRoutine != null)
			StopCoroutine(npcMoveRoutine);
	}
	/// <summary>
	///  该方法是根据我的ScheduleDataList里面设置的ScheduleDetails来切换我当前季节的NPC所要发生的动画
	/// </summary>
	/// <param name="minute"></param>
	/// <param name="hour"></param>
	/// <param name="season"></param>
	private void OnGameMinuteEvent(int minute, int hour, int day, Season season)
	{
		int time = (hour * 100) + minute;
		currentSeason = season;
		
		ScheduleDetails matchSchedule = null;
		foreach(var schedule in scheduleSet)
		{
			if(time == schedule.Time)
			{
				//这里不等于0是因为我想在该季节下的多少点多少分钟进行该动画，如果有Day的话，就只能执行一次了。
				if(day != schedule.day && schedule.day!=0)
					continue;
				if(season != schedule.season)
					continue;
				matchSchedule = schedule;
			}else if(time > schedule.Time)
			{
				break;
			}
		}
		
		if(matchSchedule != null)
		{
			BuildPath(matchSchedule);
		}
	}
	
	private void CheckVisible()
	{
		if(currentScene == SceneManager.GetActiveScene().name)
		{
			SetActiveInScene();
		}else
		{
			SetInavtiveInScene();
		}
	}
	
	private void InitNPC()
	{
		targetScene = currentScene;
		
		// 保持在当前坐标的网格中心
		currentGridPos = grid.WorldToCell(transform.position);
		transform.position = new Vector3(currentGridPos.x + Settings.gridCellSize / 2,currentGridPos.y + Settings.gridCellSize / 2,0);
		
		targetGridPos = currentGridPos;
	}
	
	/// <summary>
	/// NPC移动
	/// </summary>
	private void Movement()
	{
		if(!npcMove)
		{
			if(movementSteps.Count > 0)
			{
				MovementStep step = movementSteps.Pop();
				
				currentScene = step.sceneName;
				//Debug.Log("Movement:::"+currentScene);
				CheckVisible();
				
				nextGridPos = (Vector3Int)step.gridCoordinate;
				TimeSpan stepTime = new TimeSpan(step.hour,step.minute,step.second);
				
				MoveToGridPosition(nextGridPos,stepTime);
			}else if(!isMoving && canPlayStopAnimation)
			{
				StartCoroutine(SetStopAnimation());
			}
		}
	}
	
	private void MoveToGridPosition(Vector3Int gridPos,TimeSpan stepTime)
	{
		npcMoveRoutine =  StartCoroutine(MoveRoutine(gridPos,stepTime));
	}
	
	private IEnumerator MoveRoutine(Vector3Int gridPos,TimeSpan stepTime)
	{
		npcMove = true;
		nextWorldPos = GetWorldPosition(gridPos);
		
		// 还有时间用来移动
		if(stepTime > GameTime)
		{
			// 说明可以缓慢地移动
			float timeToMove = (float)(stepTime.TotalSeconds - GameTime.TotalSeconds);
			
			float distance = Vector3.Distance(transform.position,nextWorldPos);
			
			float speed = MathF.Max(minSpeed,(distance / timeToMove / Settings.secondThresHold));
			
			if(speed <= maxSpeed)
			{
				while(Vector3.Distance(transform.position,nextWorldPos) > Settings.pixelSize)
				{
					dir = (nextWorldPos - transform.position).normalized;
					
					Vector2 posOffset = new Vector2(dir.x * speed * Time.fixedDeltaTime,dir.y * speed * Time.fixedDeltaTime);
					rb.MovePosition(rb.position + posOffset);
					yield return new WaitForFixedUpdate();
				}
			}
		}
		
		// 如果时间已经到了，直接瞬移
		rb.position = nextWorldPos;
		currentGridPos = gridPos;
		nextGridPos = currentGridPos;
		
		npcMove = false;
	}
	
	/// <summary>
	/// 创建NOC的移动路径
	/// </summary>
	/// <param name="scheduleDetails"></param>
	public void BuildPath(ScheduleDetails scheduleDetails)
	{
		movementSteps.Clear();
		currentScheduleDetails = scheduleDetails;
		targetGridPos = (Vector3Int)scheduleDetails.targetGridPos;
		stopAnimationClip = scheduleDetails.clipAtStop;
		this.interactable = scheduleDetails.interactable;
		if(scheduleDetails.targetScene == currentScene)
		{
			AStar.Instance.BuildPath(scheduleDetails.targetScene,(Vector2Int)currentGridPos,scheduleDetails.targetGridPos,movementSteps);
			Debug.Log("1111");
		}else if(currentScene != scheduleDetails.targetScene)
		{
			SceneRoute sceneRoute = NPCManager.Instance.GetSceneRoute(currentScene,scheduleDetails.targetScene);
			
			if(sceneRoute != null)
			{
				for(int i = 0; i < sceneRoute.scenePathList.Count;i++)
				{
					Vector2Int fromPos,gotoPos;
					ScenePath path = sceneRoute.scenePathList[i];
					
					if(path.fromGridCell.x >= Settings.maxGridSize)
					{
						fromPos = (Vector2Int)currentGridPos;
					}else
					{
						fromPos = path.fromGridCell;
					}
					
					if(path.gotoGridCell.x >= Settings.maxGridSize)
					{
						gotoPos = scheduleDetails.targetGridPos;
					}else
					{
						gotoPos = path.gotoGridCell;
					}
					
					Debug.Log("BuildPath:::"+path.sceneName);
					AStar.Instance.BuildPath(path.sceneName,fromPos,gotoPos,movementSteps);
				}
			}
		}
		
		if(movementSteps.Count > 1)
		{
			// 更新每一步移动的时间戳
			UpdateTimeOnPath();
		}
	}
	
	/// <summary>
	/// 更新游戏的时间戳
	/// </summary>
	private void UpdateTimeOnPath()
	{
		MovementStep previousStep = null;
		TimeSpan currentGameTime = GameTime;
		
		foreach(MovementStep step in movementSteps)
		{
			if(previousStep == null)
				previousStep = step;
			
			step.hour = currentGameTime.Hours;
			step.minute = currentGameTime.Minutes;
			step.second = currentGameTime.Seconds;
			
			TimeSpan gridMovementStepTime;
			// 这里计算到下一个网格所需要的时间，最后除以secondThresHold是因为这是我游戏里面的时间戳，要以游戏里面设置的ThresHold为准
			if(MoveInDiagonal(step,previousStep))
			{
				gridMovementStepTime = new TimeSpan(0,0,(int)(Settings.gridCellDiagonalSize / normalSpeed / Settings.secondThresHold)+1);
			}else
			{
				gridMovementStepTime = new TimeSpan(0,0,(int)(Settings.gridCellSize / normalSpeed / Settings.secondThresHold)+1);
			}
			currentGameTime = currentGameTime.Add(gridMovementStepTime);
			
			previousStep = step;
		}
	}
	
	private bool MoveInDiagonal(MovementStep currentStep,MovementStep previousStep)
	{
		return (currentStep.gridCoordinate.x != previousStep.gridCoordinate.x) && (currentStep.gridCoordinate.y != previousStep.gridCoordinate.y);
	}
	
	/// <summary>
	/// 返回世界坐标
	/// </summary>
	/// <param name="gridPos"></param>
	/// <returns></returns>
	private Vector3 GetWorldPosition(Vector3Int gridPos)
	{
		Vector3 worldPos = grid.CellToWorld(gridPos);
		return new Vector3(worldPos.x + Settings.gridCellSize / 2,worldPos.y + Settings.gridCellSize / 2);
	}
	
	/// <summary>
	/// NPC移动时的动画设置
	/// </summary>
	private void SwitchAnimation()
	{
		//Debug.Log(transform.position + ":::::::" + GetWorldPosition(targetGridPos));
		isMoving = transform.position != GetWorldPosition(targetGridPos);
		//Debug.Log(isMoving);
		anim.SetBool("IsMoving",isMoving);
		if(isMoving)
		{
			anim.SetBool("Exit",true);
			anim.SetFloat("DirX",dir.x);
			anim.SetFloat("DirY",dir.y);
		}else
		{
			anim.SetBool("Exit",false);
		}
	}
	
	private IEnumerator SetStopAnimation()
	{
		// 设置移动完成之后将NPC朝下
		anim.SetFloat("DirX",0);
		anim.SetFloat("DirY",-1);
		
		animationBreakTime = Settings.animationBreakTime;
		
		if(stopAnimationClip != null)
		{
			animOverride[blankAnimationClip] = stopAnimationClip;
			anim.SetBool("EventAnimation",true);
			yield return null;
			anim.SetBool("EventAnimation",false);
			animOverride[stopAnimationClip] = blankAnimationClip;
		}
	}
	
	#region 设置NPC的显示
	private void SetActiveInScene()
	{
		spriteRenderer.enabled = true;
		coll.enabled = true;
		// 影子打开
		//transform.GetChild(0).gameObject.SetActive(true);
	}
	
	private void SetInavtiveInScene()
	{
		spriteRenderer.enabled = false;
		coll.enabled = false;
		// 影子关闭
		//transform.GetChild(0).gameObject.SetActive(false);
	}

	public GameSaveData gameSaveData()
	{
		GameSaveData saveData = new GameSaveData();
		saveData.characterPosDict = new Dictionary<string, SerializableVector3>();
		saveData.characterPosDict.Add("targetGridPosition",new SerializableVector3(targetGridPos));
		saveData.characterPosDict.Add("currentPosition",new SerializableVector3(transform.position));
		saveData.dataSceneName = currentScene;
		saveData.targetScene = this.targetScene;
		if(stopAnimationClip!=null)
		{
			saveData.animationInstanceID = stopAnimationClip.GetInstanceID();
		}
		saveData.interactable = this.interactable;
		saveData.timeDict = new Dictionary<string, int>();
		saveData.timeDict.Add("currentSeason",(int)currentSeason);
		return saveData;
	}

	public void RestoreData(GameSaveData saveData)
	{
		
		isInitialized = true;
		isFirstLoad = false;
		
		currentScene = saveData.dataSceneName;
		targetScene = saveData.targetScene;
		
		Vector3 pos = saveData.characterPosDict["currentPosition"].ToVector3();
		Vector3Int gridPos =(Vector3Int)saveData.characterPosDict["targetGridPosition"].ToVector2Int();
		
		transform.position = pos;
		targetGridPos = gridPos;
		
		if(saveData.animationInstanceID != 0)
		{
			this.stopAnimationClip = Resources.InstanceIDToObject(saveData.animationInstanceID) as AnimationClip;
		}
		this.interactable = saveData.interactable;
		this.currentSeason =(Season)saveData.timeDict["currentSeason"];
	}
	#endregion
}
