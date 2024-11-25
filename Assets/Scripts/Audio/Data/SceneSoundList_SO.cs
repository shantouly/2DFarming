using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneSoundList_SO", menuName = "Sound/SceneSoundList_SO", order = 0)]
public class SceneSoundList_SO : ScriptableObject {
	public List<SceneSoundItem> sceneSoundItemList;
	
	public SceneSoundItem GetSceneSoundItem(string sceneName)
	{
		return sceneSoundItemList.Find(s => s.sceneName == sceneName);
	}
}

[System.Serializable]
public class SceneSoundItem
{
	[SceneName] public string sceneName;
	public SoundName gameMusic;
	public SoundName ambientMusic;
}