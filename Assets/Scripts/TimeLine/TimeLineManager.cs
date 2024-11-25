using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class TimeLineManager : Singleton<TimeLineManager>
{
	public PlayableDirector startDirector;
	private PlayableDirector currentDirector;
	private bool isDone;
	public bool IsDone {set => isDone = value;}
	private bool isPause;
	public bool isFirstLoad = true;

	protected override void Awake()
	{
		base.Awake();
		currentDirector = startDirector;
	}
	
	void OnEnable()
	{
		EventHandler.AfterSceneUnloadEvent += OnCallAfterSceneUnloadEvent;
	}
	
	void OnDisable()
	{
		EventHandler.AfterSceneUnloadEvent -= OnCallAfterSceneUnloadEvent;
	}

	private void OnCallAfterSceneUnloadEvent()
	{
		currentDirector = FindObjectOfType<PlayableDirector>();
		if(currentDirector != null && isFirstLoad)
		{
			Debug.Log("11111111");
			currentDirector.Play();
			isFirstLoad = false;
		}
	}

	void Update()
	{
		if(isPause && Input.GetKeyDown(KeyCode.Space))
		{
			isPause = false;
			currentDirector.playableGraph.GetRootPlayable(0).SetSpeed(1d);
		}
	}
	
	public void PauseTimeLine(PlayableDirector director)
	{
		currentDirector = director;
		currentDirector.playableGraph.GetRootPlayable(0).SetSpeed(0d);
		isPause = true;
	}
}
