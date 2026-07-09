using UnityEngine;

/// <summary>
/// Auto start dialogue when scene loads
/// </summary>
public class DialogueAutoStart : MonoBehaviour
{
    public DialogueSystem dialogueSystem;
    public string dialogueId = "prologue_start";
    public string nextScene = "Prologue";

    private bool hasStarted = false;

    void Start()
    {
        if (hasStarted)
        {
            Debug.Log("DialogueAutoStart: Already started, skipping");
            return;
        }

        if (dialogueSystem == null)
        {
            Debug.LogError("DialogueAutoStart: DialogueSystem not assigned!");
            return;
        }

        hasStarted = true;
        Debug.Log("DialogueAutoStart: Starting dialogue: " + dialogueId);

        dialogueSystem.StartDialogue(dialogueId, () =>
        {
            Debug.Log("DialogueAutoStart: Dialogue complete, loading: " + nextScene);
            SceneLoader.Instance?.LoadScene(nextScene);
        });
    }
}
