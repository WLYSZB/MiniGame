using UnityEngine;

/// <summary>
/// Dialogue scene transition - simple version
/// </summary>
public class DialogueSceneTransition : MonoBehaviour
{
    public DialogueSystem dialogueSystem;
    public string nextScene = "Prologue";

    private bool hasStartedDialogue = false;

    void Awake()
    {
        // Disable this script if dialogue already started
        if (hasStartedDialogue)
        {
            enabled = false;
            return;
        }
    }

    void Start()
    {
        Debug.Log("DialogueSceneTransition.Start called, hasStartedDialogue=" + hasStartedDialogue);

        if (hasStartedDialogue)
        {
            Debug.Log("Dialogue already started, skipping");
            return;
        }

        hasStartedDialogue = true;
        StartDialogue();
    }

    void StartDialogue()
    {
        if (dialogueSystem == null)
        {
            Debug.LogError("DialogueSystem is not assigned!");
            return;
        }

        Debug.Log("Starting dialogue: prologue_start");
        dialogueSystem.StartDialogue("prologue_start", () =>
        {
            Debug.Log("Dialogue complete, loading next scene: " + nextScene);
            SceneLoader.Instance?.LoadScene(nextScene);
        });
    }
}
