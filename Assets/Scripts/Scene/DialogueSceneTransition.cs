using UnityEngine;

/// <summary>
/// Dialogue scene transition control
/// </summary>
public class DialogueSceneTransition : MonoBehaviour
{
    public DialogueSystem dialogueSystem;
    public string nextScene = "Prologue";
    private bool hasStarted = false;

    void Start()
    {
        Debug.Log("DialogueSceneTransition.Start called");

        if (hasStarted)
        {
            Debug.Log("Already started, skipping");
            return;
        }

        hasStarted = true;

        if (dialogueSystem != null)
        {
            dialogueSystem.StartDialogue("prologue_start", () =>
            {
                SceneLoader.Instance?.LoadScene(nextScene);
            });
        }
        else
        {
            Debug.LogError("DialogueSystem is not assigned!");
        }
    }
}
