using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerItemFade : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        ItemFader[] fader = collision.GetComponentsInChildren<ItemFader>();
        
        if(fader.Length > 0)
        {
            foreach(var item in fader)
            {
                item.FadeOut();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        ItemFader[] fader = collision.GetComponentsInChildren<ItemFader>();

        if(fader.Length > 0)
        {
            foreach(var item in fader)
            {
                item.FadeIn();
            }
        }
    }
}
