using UnityEngine;
using System.Collections;

/// <summary>
/// 操作教程提示 - 游戏开始时显示，首次推箱后淡出消失
/// </summary>
public class TutorialHint : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private bool dismissed = false;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    /// <summary>
    /// 由 PushableBox 首次推箱成功时调用
    /// </summary>
    public void Dismiss()
    {
        if (dismissed) return;
        dismissed = true;
        StartCoroutine(FadeOut(0.5f));
    }

    IEnumerator FadeOut(float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = 1 - (elapsed / duration);
            yield return null;
        }
        canvasGroup.alpha = 0;
        gameObject.SetActive(false);
    }
}
