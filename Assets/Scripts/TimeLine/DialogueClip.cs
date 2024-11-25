using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class DialogueClip : PlayableAsset, ITimelineClipAsset
{
	public ClipCaps clipCaps => ClipCaps.None;		// 表示不能进行高级的操作如：循环，剪辑
	public DialogueBehaviour dialogue = new DialogueBehaviour();

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		var playable = ScriptPlayable<DialogueBehaviour>.Create(graph,dialogue);
		return playable;
	}
}
