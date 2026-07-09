using UnityEngine;

/// <summary>
/// Dialogue scene transition control - singleton to prevent duplicate calls
/// </summary>
public class DialogueSceneTransition : MonoBehaviour
{
    public DialogueSystem dialogueSystem;
    public string nextScene = "Prologue";

    private static bool hasInstance = false;

    void Start()
    {
        Debug.Log("DialogueSceneTransition.Start called");

        // Prevent duplicate instances
        if (hasInstance)
        {
            Debug.Log("Another DialogueSceneTransition already exists, destroying this one");
            Destroy(gameObject);
            return;
        }

        hasInstance = true;

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

    void OnDestroy()
    {
        hasInstance = false;
    }
}
