using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class DialogueClip : PlayableAsset, ITimelineClipAsset
{
	public ClipCaps clipCaps => ClipCaps.None;		// ��ʾ���ܽ��и߼��Ĳ����磺ѭ��������
	public DialogueBehaviour dialogue = new DialogueBehaviour();

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		var playable = ScriptPlayable<DialogueBehaviour>.Create(graph,dialogue);
		return playable;
	}
}
