using UnityEngine;
using DG.Tweening;
/// <summary>
/// ���Ƶ����ϵ���ľ���߲�ʲô��͸���ȵ�
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
    /// ���ò�͸��
    /// </summary>
    public void FadeIn()
    {
        Color targetColor = new Color(1, 1, 1, 1);
        spriteRenderer.DOColor(targetColor, Settings.fadeDruation);
    }

    /// <summary>
    ///  ����͸��
    /// </summary>
    public void FadeOut()
    {
        Color targetColor = new Color(1, 1, 1, Settings.fadeAlpha);
        spriteRenderer.DOColor(targetColor, Settings.fadeDruation);
    }
}
