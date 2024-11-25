using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using Cinemachine;
using Fram.Dialogue;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(NPCMovement))]
[RequireComponent(typeof(BoxCollider2D))]
public class DialogueController : MonoBehaviour
{
	public UnityEvent finishEvent;
	private NPCMovement nPCMovement;
	public List<DialoguePiece> dialoguePieces;
	private Stack<DialoguePiece> dialoguePiecesStack;	
	private bool canTalk;
	private bool isTalking;
	private GameObject ui_Sign;
	
	void Awake()
	{
		InitStack();
		ui_Sign = transform.GetChild(1).gameObject;
		nPCMovement = gameObject.GetComponent<NPCMovement>();
	}
	
	void OnTriggerEnter2D(Collider2D other)
	{
		if(other.gameObject.CompareTag("Player"))
		{
			canTalk = !nPCMovement.isMoving && nPCMovement.interactable;
		}
	}
	
	void OnTriggerExit2D(Collider2D other)
	{
		if(other.gameObject.CompareTag("Player"))
		{
			canTalk = false;
		}
	}
	
	void Update()
	{
		ui_Sign.SetActive(canTalk);
		if(canTalk && Input.GetKeyDown(KeyCode.Space) && !isTalking)
		{
			StartCoroutine(DialogueRoutine());
		}
	}
	
	private IEnumerator DialogueRoutine()
	{
		isTalking = true;
		
		if(dialoguePiecesStack.TryPop(out DialoguePiece result))
		{
			EventHandler.CallShowDialogueEvent(result);
			EventHandler.CallUpdateGameStateEvent(GameState.GamePause);
			yield return new WaitUntil(()=>result.isDone);
			isTalking = false;
		}else
		{
			isTalking = false;
			EventHandler.CallUpdateGameStateEvent(GameState.GamePlay);
			EventHandler.CallShowDialogueEvent(null);
			InitStack();

			if(finishEvent!=null)
			{
				finishEvent.Invoke();
				canTalk = false;
			}
		}
	}
	
	private void InitStack()
	{
		dialoguePiecesStack = new Stack<DialoguePiece>();
		
		for(int i = dialoguePieces.Count - 1;i > -1;i--)
		{
			dialoguePieces[i].isDone = false;
			dialoguePiecesStack.Push(dialoguePieces[i]);
		}
	}	
}
