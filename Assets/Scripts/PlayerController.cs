using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public int stepsToMove = 0;

    [Header("Components")]
    public GameObject selectedToken;
    public TurnSystem turnSystem;
    public BoardHandler boardHandler;


    // select token from click
    public void SelectToken(GameObject token)
    {
        PieceController piece = token.GetComponent<PieceController>();
        if (!turnSystem.IsCurrentPlayerPiece(piece)) return;

        selectedToken = token;
        //Debug.Log($"Selected token: {token.name}");
        MoveSelectedPiece();
    }

    public void MoveSelectedPiece()
    {
        if (selectedToken == null || stepsToMove <= 0)
        {
            Debug.LogWarning("No token selected or invalid step count.");
            return;
        }

        PieceController piece = selectedToken.GetComponent<PieceController>();
        selectedToken = null; // Clear selection after moving

        if (piece != null)
        {
            piece.MoveBySteps(stepsToMove);
        }

        // When movement is done, decide if extra turn or switch
        piece.onMovementComplete = () =>
        {
            if (turnSystem.rolledSix)
            {
                turnSystem.StartTurn(turnSystem.currentPlayer); // Extra turn
            }
            else
            {
                turnSystem.SwitchTurn(); // Normal switch
            }
        };

    }

    public bool HasValidMove(int steps)
    {
        foreach (var token in (turnSystem.currentPlayer == TurnSystem.Player.Green ? boardHandler.greenPieces : boardHandler.bluePieces))
        {
            var piece = token.GetComponent<PieceController>();
            if (piece.CanMove(steps))
                return true;
        }
        return false;
    }

}
