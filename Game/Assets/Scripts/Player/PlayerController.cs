using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PrologueBoard board;

    public void BindBoard(PrologueBoard targetBoard)
    {
        board = targetBoard;
    }

    private void Update()
    {
        if (board == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            board.TryMovePlayer(Vector2Int.up);
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            board.TryMovePlayer(Vector2Int.down);
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            board.TryMovePlayer(Vector2Int.left);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            board.TryMovePlayer(Vector2Int.right);
        }
    }
}
