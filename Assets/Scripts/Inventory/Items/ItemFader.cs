using UnityEngine;
using DG.Tweening;
/// <summary>
/// 控制地面上的树木或者草什么的透明度的
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class ItemFader : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// 设置不透明
    /// </summary>
    public void FadeIn()
    {
        Color targetColor = new Color(1, 1, 1, 1);
        spriteRenderer.DOColor(targetColor, Settings.fadeDruation);
    }

    /// <summary>
    ///  设置透明
    /// </summary>
    public void FadeOut()
    {
        Color targetColor = new Color(1, 1, 1, Settings.fadeAlpha);
        spriteRenderer.DOColor(targetColor, Settings.fadeDruation);
    }
}
