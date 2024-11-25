using Cinemachine;
using UnityEngine;

public class SwitchBounds : MonoBehaviour
{

   //TODO:切换场景后更改调用
    private void OnEnable()
    {
        EventHandler.AfterSceneUnloadEvent += SwitchConfinerShape;
    }
    private void OnDisable()
    {
        EventHandler.AfterSceneUnloadEvent -= SwitchConfinerShape;
    }

    private void SwitchConfinerShape()//设置摄像机边界
    {
        PolygonCollider2D confinerShape = GameObject.FindGameObjectWithTag("BoundsConfiner").GetComponent<PolygonCollider2D>();

        CinemachineConfiner confiner = GetComponent<CinemachineConfiner>();

        //The 2D shape within which the camera is to be contained.
        confiner.m_BoundingShape2D = confinerShape;

        //Call this if the bounding shape's points change at runtime
        confiner.InvalidatePathCache();//清楚缓存
    }
}
