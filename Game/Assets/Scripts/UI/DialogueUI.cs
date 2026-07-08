using UnityEngine;

public class DialogueUI : MonoBehaviour
{
    private bool isVisible;
    private string speaker = string.Empty;
    private string text = string.Empty;

    public void Show(string speakerName, string dialogueText)
    {
        speaker = speakerName ?? string.Empty;
        text = dialogueText ?? string.Empty;
        isVisible = true;
    }

    public void Hide()
    {
        isVisible = false;
        speaker = string.Empty;
        text = string.Empty;
    }

    private void OnGUI()
    {
        if (!isVisible)
        {
            return;
        }

        var panelRect = new Rect(24f, Screen.height - 190f, Screen.width - 48f, 166f);
        GUI.Box(panelRect, string.Empty);
        GUI.Label(new Rect(panelRect.x + 16f, panelRect.y + 16f, panelRect.width - 32f, 24f), speaker);
        GUI.Label(new Rect(panelRect.x + 16f, panelRect.y + 48f, panelRect.width - 32f, 72f), text);
        GUI.Label(new Rect(panelRect.x + 16f, panelRect.y + 126f, panelRect.width - 32f, 24f), "Click, Space, or Enter to continue.");
    }
}
