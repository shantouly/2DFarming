using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeUI : MonoBehaviour
{
    public RectTransform dayNightImae;
    public RectTransform clockParent;
    public Image seasonImage;
    public TextMeshProUGUI dateText;
    public TextMeshProUGUI timeText;
    public Sprite[] seasonSprites;

    private List<GameObject> blockChids = new List<GameObject>();

    private void Awake()
    {
        for(int i = 0;i < clockParent.childCount; i++)
        {
            blockChids.Add(clockParent.GetChild(i).gameObject);
            clockParent.GetChild(i).gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        EventHandler.GameMinuteEvent += OnGameMinuteEvent;
        EventHandler.GameDateEvent += OnGameDateEvent;
    }

    private void OnDisable()
    {
        EventHandler.GameMinuteEvent -= OnGameMinuteEvent;
        EventHandler.GameDateEvent -= OnGameDateEvent;
    }
 
    private void OnGameMinuteEvent(int minute, int hour, int day, Season season)
    {
        timeText.text = hour.ToString("00") + ":" + minute.ToString("00");
    }

    private void OnGameDateEvent(int hour, int day, int month, int year, Season season)
    {
        dateText.text = year + ":" + month.ToString("00") + ":" + day.ToString("00");
        seasonImage.sprite = seasonSprites[(int)season];

        SwitchHourImage(hour);
        SwitchDayNightImage(hour);
    }

    private void SwitchHourImage(int hour)
    {
        int index = hour / 4;

        if(index == 0)
        {
            foreach(var item in blockChids)
            {
                item.SetActive(false);
            }
        }
        else
        {
            for(int i = 0;i< blockChids.Count; i++)
            {
                if(i < index + 1)
                {
                    blockChids[i].SetActive(true);
                }
                else
                {
                    blockChids[i].SetActive(false);
                }
            }
        }
    }

    private void SwitchDayNightImage(int hour)
    {
        Vector3 target = new Vector3(0, 0, hour * 15 - 90);              // 24个小时，360°，一个小时转15°
        dayNightImae.DORotate(target, 1f, RotateMode.Fast);
    }
}
