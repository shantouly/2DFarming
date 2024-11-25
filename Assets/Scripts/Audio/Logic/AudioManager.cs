using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : Singleton<AudioManager>
{
	[Header("音乐数据库")]
	public SoundDetailsList_SO soundDetailsData;
	public SceneSoundList_SO sceneSoundData;
	[Header("Audio Source")]
	public AudioSource ambientSource;
	public AudioSource gameSource;
	private Coroutine soundRoutine;
	
	[Header("Audio Mixer")]
	public AudioMixer audioMixer;
	[Header("SnapShots")]
	public AudioMixerSnapshot normalSnapShot;
	public AudioMixerSnapshot ambientSnapShot;
	public AudioMixerSnapshot muteSnapShot;
	private float musicTransitionSecond = 8f;
	// 这是一个属性
	public float musicStartSound => UnityEngine.Random.Range(5f,15f);
	
	void OnEnable()
	{
		EventHandler.AfterSceneUnloadEvent += OnAfterSceneUnloadEvent;
		EventHandler.PlaySoundEvent += OnPlaySoundEvent;
		EventHandler.EndGameEvent += OnEndGameEvent;
	}
	
	void OnDisable()
	{
		EventHandler.AfterSceneUnloadEvent -= OnAfterSceneUnloadEvent;
		EventHandler.PlaySoundEvent -= OnPlaySoundEvent;
		EventHandler.EndGameEvent -= OnEndGameEvent;
	}


	private void OnPlaySoundEvent(SoundName name)
	{
		var soundDetails = soundDetailsData.GetSoundDetails(name);
		EventHandler.CallInitSoundEffect(soundDetails);
	}

	private void OnAfterSceneUnloadEvent()
	{
		string sceneName = SceneManager.GetActiveScene().name;
		
		SceneSoundItem sceneSound =  sceneSoundData.GetSceneSoundItem(sceneName);
		if(sceneSound == null)
			return;
		
		SoundDetails ambient = soundDetailsData.GetSoundDetails(sceneSound.ambientMusic);
		SoundDetails music = soundDetailsData.GetSoundDetails(sceneSound.gameMusic);
		
		// PlayMusicSound(music);
		// PlayAmbientSound(ambient);
		if(soundRoutine != null)
		{
			StopCoroutine(soundRoutine);
		}
		soundRoutine =  StartCoroutine(PlaySoundRoutine(music,ambient));
	}
	
	/// <summary>
	/// 返回菜单的方法：将当前的音效关闭，并将静音的音效打开，确保打开菜单是没有音效的
	/// </summary>
	private void OnEndGameEvent()
	{
		StopCoroutine(soundRoutine);
		muteSnapShot.TransitionTo(1f);
	}
	
	private IEnumerator PlaySoundRoutine(SoundDetails music,SoundDetails ambient)
	{
		if(music != null && ambient != null)
		{
			PlayAmbientSound(ambient,1f);
			yield return new WaitForSeconds(musicStartSound);
			PlayMusicSound(music,musicTransitionSecond);
		}
	}
	
	private void PlayMusicSound(SoundDetails music,float transitionSecond)
	{
		audioMixer.SetFloat("Music Volume",ConvertSoundVolume(music.soundVolume));
		gameSource.clip = music.soundClip;
		if(gameSource.isActiveAndEnabled)
		{
			gameSource.Play();
		}
		
		normalSnapShot.TransitionTo(transitionSecond);
	} 
	
	private void PlayAmbientSound(SoundDetails ambient,float transitionSecond)
	{
		audioMixer.SetFloat("Ambient Volume",ConvertSoundVolume(ambient.soundVolume));
		ambientSource.clip = ambient.soundClip;
		if(ambientSource.isActiveAndEnabled)
		{
			ambientSource.Play();
		}	
		
		ambientSnapShot.TransitionTo(transitionSecond);
	}
	
	private float ConvertSoundVolume(float amount)
	{
		return (amount * 100 - 80);
	}
	
	public void SetMasterVolume(float volume)
	{
		audioMixer.SetFloat("Master Volume",ConvertSoundVolume(volume));
	}
}
