using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings
{
	public const float fadeDruation = 0.35f;
	public const float fadeAlpha = 0.45f;

	// 时间相关
	public const float secondThresHold = 0.1f;
	public const int secondHold = 59;
	public const int minuteHold = 59;
	public const int hourHold = 23;
	public const int dayHold = 30;
	public const int seasonHold = 3;
	public const float fadeUIDruation = 1.5f;
 	public const float gridCellSize = 1;

	public const float gridCellDiagonalSize = 1.41f;

	public const float pixelSize = 0.05f;//20*20  占 1 unit
	
	public const float animationBreakTime = 5f;//动画间隔时间
	public const int maxGridSize = 9999;
	public const int reapCount = 2;
	
	// 灯光
	public const float lightChangeDruation = 25f;
	public static TimeSpan morningTime = new TimeSpan(5,0,0);
	public static TimeSpan nightTime = new TimeSpan(19,0,0);
	//public static Vector3 playerStartPos = new Vector3(54.2f,-15.64f,0);
	public const int playerMoney = 100;
}
