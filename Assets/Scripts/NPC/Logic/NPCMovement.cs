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
	public ScheduleDataList_SO scheduleData;					// ��������scheduleDetails
	private SortedSet<ScheduleDetails> scheduleSet;				// ����Details��������ȼ���������
	private ScheduleDetails currentScheduleDetails;
	[SerializeField]
	public string currentScene;
	private string targetScene;
	private Vector3Int currentGridPos;
	private Vector3Int targetGridPos;
	private Vector3Int nextGridPos;
	private Vector3 nextWorldPos;
	
	public string StartScene{ set => currentScene = value;}
	
	[Header("�ƶ�����")]
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
	
	
	// ������ʱ��
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
		
		// ��ʼ��ScheduleSet
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
			
		// ��ʱ��
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
	///  �÷����Ǹ����ҵ�ScheduleDataList�������õ�ScheduleDetails���л��ҵ�ǰ���ڵ�NPC��Ҫ�����Ķ���
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
				//���ﲻ����0����Ϊ�����ڸü����µĶ��ٵ���ٷ��ӽ��иö����������Day�Ļ�����ֻ��ִ��һ���ˡ�
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
		
		// �����ڵ�ǰ�������������
		currentGridPos = grid.WorldToCell(transform.position);
		transform.position = new Vector3(currentGridPos.x + Settings.gridCellSize / 2,currentGridPos.y + Settings.gridCellSize / 2,0);
		
		targetGridPos = currentGridPos;
	}
	
	/// <summary>
	/// NPC�ƶ�
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
		
		// ����ʱ�������ƶ�
		if(stepTime > GameTime)
		{
			// ˵�����Ի������ƶ�
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
		
		// ���ʱ���Ѿ����ˣ�ֱ��˲��
		rb.position = nextWorldPos;
		currentGridPos = gridPos;
		nextGridPos = currentGridPos;
		
		npcMove = false;
	}
	
	/// <summary>
	/// ����NOC���ƶ�·��
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
			// ����ÿһ���ƶ���ʱ���
			UpdateTimeOnPath();
		}
	}
	
	/// <summary>
	/// ������Ϸ��ʱ���
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
			// ������㵽��һ����������Ҫ��ʱ�䣬������secondThresHold����Ϊ��������Ϸ�����ʱ�����Ҫ����Ϸ�������õ�ThresHoldΪ׼
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
	/// ������������
	/// </summary>
	/// <param name="gridPos"></param>
	/// <returns></returns>
	private Vector3 GetWorldPosition(Vector3Int gridPos)
	{
		Vector3 worldPos = grid.CellToWorld(gridPos);
		return new Vector3(worldPos.x + Settings.gridCellSize / 2,worldPos.y + Settings.gridCellSize / 2);
	}
	
	/// <summary>
	/// NPC�ƶ�ʱ�Ķ�������
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
		// �����ƶ����֮��NPC����
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
	
	#region ����NPC����ʾ
	private void SetActiveInScene()
	{
		spriteRenderer.enabled = true;
		coll.enabled = true;
		// Ӱ�Ӵ�
		//transform.GetChild(0).gameObject.SetActive(true);
	}
	
	private void SetInavtiveInScene()
	{
		spriteRenderer.enabled = false;
		coll.enabled = false;
		// Ӱ�ӹر�
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
