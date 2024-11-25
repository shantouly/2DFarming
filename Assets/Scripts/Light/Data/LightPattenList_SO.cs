using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="LightPattenList_SO",menuName ="Light/Light Patten")]
public class LightPattenList_SO : ScriptableObject
{
	public List<LightDetails> lightPattenList;
	
	/// <summary>
	/// ���ݼ��ں����ڷ��صƹ�����
	/// </summary>
	/// <param name="season">��ǰ����</param>
	/// <param name="lightShift">��ǰ������</param>
	/// <returns></returns>
	public LightDetails GetLightDetails(Season season,LightShift lightShift)
	{
		return lightPattenList.Find(l => l.season == season && l.lightShift == lightShift);
	}
}

[System.Serializable]
public class LightDetails
{
	public Season season;
	public LightShift lightShift;
	public Color LightColor;
	public float LightIntensity;				
}
