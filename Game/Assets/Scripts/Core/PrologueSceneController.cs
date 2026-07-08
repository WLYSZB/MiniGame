using UnityEngine;

public class PrologueSceneController : MonoBehaviour
{
    [SerializeField] private DialogueData openingDialogue;
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private PrologueBoard board;

    private void Start()
    {
        if (board != null)
        {
            board.SetInputEnabled(false);
        }

        if (dialogueManager == null)
        {
            if (board != null)
            {
                board.SetInputEnabled(true);
            }

            return;
        }

        dialogueManager.Play(openingDialogue, OnDialogueFinished);
    }

    private void OnDialogueFinished()
    {
        if (board != null)
        {
            board.SetInputEnabled(true);
        }
    }
}
