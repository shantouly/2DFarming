using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using Fram.Save;
using UnityEngine;

public class Player : MonoBehaviour,ISaveable
{
	private Rigidbody2D rb;

	public float speed;
	private float inputX;
	private float inputY;
	private Vector2 movementInput;
	private Animator[] anims;
	private bool isMoving;
	private bool inputDisable;
	// 使用工具的变量
	private float mouseX;
	private float mouseY;

	public string GUID => GetComponent<DataGUID>().guid;

	//private bool useTool;

	private void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		anims = GetComponentsInChildren<Animator>();
		inputDisable = true;
	}
	
	void Start()
	{
		ISaveable saveable = this;
		saveable.RegisterSaveable();
	}
	
	private void Update()
	{
		if(inputDisable == false)
		{
			PlayerInput();
		}
		else
		{
			isMoving = false;
		}
		SwitchAnimation();
	}

	private void FixedUpdate()
	{
		if(inputDisable == false)
		{
			Movement();
		}
	}

	private void OnEnable()
	{
		EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
		EventHandler.AfterSceneUnloadEvent += OnAfterSceneUnloadEvent;
		EventHandler.MoveToPosition += OnMoveToPosition;
		EventHandler.MouseClickEvent += OnMouseClickEvent;
		EventHandler.UpdateGameStateEvent += OnUpdateGameStateEvent;
		EventHandler.StartNewGameEvent += OnStartNewGameEvent;
		EventHandler.EndGameEvent += OnCallEndGameEvent;
	}
	
	private void OnDisable()
	{
		EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
		EventHandler.AfterSceneUnloadEvent -= OnAfterSceneUnloadEvent;
		EventHandler.MoveToPosition -= OnMoveToPosition;
		EventHandler.MouseClickEvent -= OnMouseClickEvent;
		EventHandler.UpdateGameStateEvent -= OnUpdateGameStateEvent;
		EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
		EventHandler.EndGameEvent -= OnCallEndGameEvent;
	}


	/// <summary>
	/// 开始一个新游戏需要做的事情：设置玩家是不能进行移动的，初始化玩家的坐标
	/// </summary>
	/// <param name="index"></param>
	private void OnStartNewGameEvent(int index)
	{
		inputDisable = false;
		transform.position = transform.position;
	}
	
	/// <summary>
	/// 结束的时候希望玩家是不能进行移动的
	/// </summary>
	private void OnCallEndGameEvent()
	{
		inputDisable = true;
	}

	private void OnUpdateGameStateEvent(GameState gameState)
	{
		inputDisable = gameState switch
		{
			GameState.GamePlay => false,
			GameState.GamePause => true,
			_=>false
		};
	}

	private void OnMouseClickEvent(Vector3 mouseWorldPos, ItemDetails itemDetails)
	{
		// TODO先执行动画
		if(itemDetails.itemtype != itemType.Seed && itemDetails.itemtype != itemType.Commodity && itemDetails.itemtype != itemType.Furniture){
			mouseX = mouseWorldPos.x - transform.position.x;
			mouseY = mouseWorldPos.y - (transform.position.y + 0.85f);

			if (Mathf.Abs(mouseX) > Mathf.Abs(mouseY))
				mouseY = 0;
			else
				mouseX = 0;

			StartCoroutine(UseToolRoutine(mouseWorldPos, itemDetails));
		}else
		{
			EventHandler.CallExecuteActionAfterAnimation(mouseWorldPos, itemDetails);
		}
	}
	
	private IEnumerator UseToolRoutine(Vector3 mouseWorldPos, ItemDetails itemDetails)
	{
		//useTool = true;
		inputDisable = true;
		yield return null;
		foreach(var anim in anims)
		{
			anim.SetTrigger("useTool");
			anim.SetFloat("InputX", mouseX);
			anim.SetFloat("InputY", mouseY);
		}

		yield return new WaitForSeconds(0.45f);
		EventHandler.CallExecuteActionAfterAnimation(mouseWorldPos, itemDetails);
		yield return new WaitForSeconds(0.25f);
		// 执行完之后才可以移动
		//useTool = false;
		inputDisable = false;
	}

	private void OnMoveToPosition(Vector3 pos)
	{
		transform.position = pos;
	}

	private void OnAfterSceneUnloadEvent()
	{
		inputDisable = false;
	}

	private void OnBeforeSceneUnloadEvent()
	{
		inputDisable = true;
	}

	private void PlayerInput()
	{
		inputX = Input.GetAxisRaw("Horizontal");
		inputY = Input.GetAxisRaw("Vertical");

		if(inputX!=0 && inputY != 0)
		{
			inputX *= 0.7f;
			inputY *= 0.7f;
		}

		if (Input.GetKey(KeyCode.LeftShift))
		{
			inputX *= 0.5f;
			inputY *= 0.5f;
		}
		movementInput = new Vector2(inputX, inputY);
		isMoving = movementInput != Vector2.zero;
	}

	private void Movement()
	{
		rb.MovePosition(rb.position + movementInput * speed * Time.deltaTime);
	}

	private void SwitchAnimation()
	{

		foreach(Animator anim in anims)
		{
			anim.SetBool("IsMoving", isMoving);
			anim.SetFloat("mouseX", mouseX);
			anim.SetFloat("mouseY", mouseY);
			if (isMoving)
			{
				anim.SetFloat("InputX", inputX);
				anim.SetFloat("InputY", inputY);
			}
		}
	}

	/// <summary>
	/// 存储player的坐标
	/// </summary>
	/// <returns></returns>
	public GameSaveData gameSaveData()
	{
		GameSaveData SaveData = new GameSaveData();
		SaveData.characterPosDict = new Dictionary<string, SerializableVector3>();
		SaveData.characterPosDict.Add(this.name,new SerializableVector3(transform.position));
		
		return SaveData;
	}

	/// <summary>
	/// 返回存储的坐标
	/// </summary>
	/// <param name="saveData"></param>
	public void RestoreData(GameSaveData saveData)
	{
		var targetPos = saveData.characterPosDict[this.name];
		transform.position = targetPos.ToVector3();
	}
}
