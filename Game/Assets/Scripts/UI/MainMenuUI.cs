using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private NameInputUI nameInputUI;

    public void OnStartClicked()
    {
        if (nameInputUI != null)
        {
            nameInputUI.Show();
        }
    }

    private void OnGUI()
    {
        if (nameInputUI != null && nameInputUI.IsVisible)
        {
            return;
        }

        var panelRect = new Rect(24f, 24f, 360f, 170f);
        GUI.Box(panelRect, "MiniGame Prologue Slice");
        GUI.Label(new Rect(panelRect.x + 16f, panelRect.y + 40f, 320f, 40f), "Enter a custom name, then start the prologue tutorial.");

        if (GUI.Button(new Rect(panelRect.x + 16f, panelRect.y + 96f, 320f, 40f), "Start"))
        {
            OnStartClicked();
        }
    }
}
