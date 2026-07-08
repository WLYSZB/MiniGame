using System;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private DialogueUI dialogueUI;

    private DialogueData currentData;
    private int currentIndex;
    private Action onFinished;
    private bool isPlaying;
    private float acceptInputAfter;

    private void Update()
    {
        if (!isPlaying || Time.unscaledTime < acceptInputAfter)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter) ||
            Input.GetMouseButtonDown(0))
        {
            OnNextClicked();
        }
    }

    public void Play(DialogueData dialogueData, Action finished)
    {
        if (dialogueUI == null)
        {
            Debug.LogWarning("[DialogueManager] dialogueUI is null – dialogue will not be displayed. Assign it in the Inspector.");
        }

        currentData = dialogueData;
        currentIndex = 0;
        onFinished = finished;
        acceptInputAfter = Time.unscaledTime + 0.1f;

        if (currentData == null || currentData.Lines == null || currentData.Lines.Length == 0)
        {
            isPlaying = false;
            dialogueUI?.Hide();
            onFinished?.Invoke();
            return;
        }

        isPlaying = true;
        ShowCurrentLine();
    }

    public void OnNextClicked()
    {
        if (!isPlaying)
        {
            return;
        }

        currentIndex++;
        if (currentData == null || currentIndex >= currentData.Lines.Length)
        {
            isPlaying = false;
            dialogueUI?.Hide();
            onFinished?.Invoke();
            return;
        }

        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        var line = currentData.Lines[currentIndex];
        var playerName = GameManager.Instance != null ? GameManager.Instance.GetPlayerName() : GameManager.DefaultPlayerName;
        dialogueUI?.Show(line.speaker, DialogueFormatter.Format(line.text, playerName));
    }
}
