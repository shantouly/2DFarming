using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Fram.Dialogue
{
	public class DialogueUI : MonoBehaviour
	{
		public GameObject dialogueBox;
		public Image faceLeft,faceRight;
		public Text nameLeft,nameRight;
		public Text dialogueContent;
		public GameObject continueBox;
		
		void OnEnable()
		{
			EventHandler.ShowDialogueEvent += OnShowDialogueEvent;
		}
		
		void OnDisable()
		{
			EventHandler.ShowDialogueEvent -= OnShowDialogueEvent;
		}

		private void OnShowDialogueEvent(DialoguePiece piece)
		{
			StartCoroutine(ShowDialogue(piece));
		}

		/// <summary>
		/// 开启对话的线程
		/// </summary>
		/// <param name="piece"></param>
		/// <returns></returns>
		private IEnumerator ShowDialogue(DialoguePiece piece)
		{
			if(piece != null)
			{
				dialogueBox.SetActive(true);
				continueBox.SetActive(false);
				
				dialogueContent.text = string.Empty;
				if(piece.name != string.Empty)
				{
					if(piece.onLeft)
					{
						faceLeft.gameObject.SetActive(true);
						faceRight.gameObject.SetActive(false);
						faceLeft.sprite = piece.faceImage;
						nameLeft.text = piece.name;
					}else
					{
						faceRight.gameObject.SetActive(true);
						faceLeft.gameObject.SetActive(false);
						faceRight.sprite = piece.faceImage;
						nameRight.text = piece.name;
					}
				}else
				{
					faceLeft.gameObject.SetActive(false);
					faceRight.gameObject.SetActive(false);
					nameLeft.gameObject.SetActive(false);
					nameRight.gameObject.SetActive(false);
				}	
				
				yield return dialogueContent.DOText(piece.dialogueString, 1f).WaitForCompletion();

				piece.isDone = true;

				if (piece.hasToPause && piece.isDone)
					continueBox.SetActive(true);
			}else
			{
				dialogueBox.SetActive(false);
				yield break;
			}
		}
	}
}
