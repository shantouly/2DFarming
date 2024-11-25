using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightController : MonoBehaviour
{
	public LightPattenList_SO lightData;
	private Light2D currentLight;
	private LightDetails currentLightDetails;
	
	void Awake()
	{
		currentLight = GetComponent<Light2D>();
	}
	
	// 实现切换灯光的方法
	public void ChangeLightShtft(Season season,LightShift lightShift,float timeDifference)
	{
		currentLightDetails = lightData.GetLightDetails(season,lightShift);
		
		if(timeDifference < Settings.lightChangeDruation)
		{
			var colorOffset = (currentLightDetails.LightColor - currentLight.color) / Settings.lightChangeDruation * timeDifference;
			currentLight.color += colorOffset;
			DOTween.To(()=>currentLight.color,c => currentLight.color = c,currentLightDetails.LightColor,Settings.lightChangeDruation - timeDifference);
			DOTween.To(()=>currentLight.intensity,i => currentLight.intensity = i,currentLightDetails.LightIntensity,Settings.lightChangeDruation - timeDifference);
		}else if(timeDifference >= Settings.lightChangeDruation)
		{
			currentLight.color = currentLightDetails.LightColor;
			currentLight.intensity = currentLightDetails.LightIntensity;
		}
	}
}
