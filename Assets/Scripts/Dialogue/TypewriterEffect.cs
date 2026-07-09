using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// 打字机效果 - 逐字显示文本
/// </summary>
public class TypewriterEffect : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public float typingSpeed = 0.03f;
    private Coroutine typingCoroutine;
    private bool isTyping = false;

    /// <summary>
    /// 开始打字效果
    /// </summary>
    public void TypeText(string text, System.Action onComplete = null)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(Type(text, onComplete));
    }

    IEnumerator Type(string text, System.Action onComplete)
    {
        isTyping = true;
        textComponent.text = "";
        foreach (char c in text)
        {
            textComponent.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
        onComplete?.Invoke();
    }

    /// <summary>
    /// 跳过打字，直接显示完整文本
    /// </summary>
    public void Skip()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        isTyping = false;
    }

    public bool IsTyping => isTyping;
}
