using UnityEngine;

public class NameInputUI : MonoBehaviour
{
    private string playerName = string.Empty;
    private bool isVisible;
    private bool focusNameField;

    public bool IsVisible => isVisible;

    public void Show()
    {
        playerName = string.Empty;
        isVisible = true;
        focusNameField = true;
    }

    public void OnConfirmClicked()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        GameManager.Instance.SetPlayerName(playerName);
        isVisible = false;
        GameManager.Instance.LoadPrologue();
    }

    private void OnGUI()
    {
        if (!isVisible)
        {
            return;
        }

        var panelRect = new Rect(24f, 210f, 360f, 180f);
        GUI.Box(panelRect, "Choose A Name");
        GUI.Label(new Rect(panelRect.x + 16f, panelRect.y + 38f, 320f, 24f), "Leave blank to use the default name.");

        GUI.SetNextControlName("PlayerNameField");
        playerName = GUI.TextField(new Rect(panelRect.x + 16f, panelRect.y + 72f, 320f, 30f), playerName, 24);

        if (focusNameField)
        {
            GUI.FocusControl("PlayerNameField");
            focusNameField = false;
        }

        if (GUI.Button(new Rect(panelRect.x + 16f, panelRect.y + 120f, 320f, 36f), "Confirm") ||
            (Event.current.type == EventType.KeyDown &&
                (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)))
        {
            OnConfirmClicked();
        }
    }
}
