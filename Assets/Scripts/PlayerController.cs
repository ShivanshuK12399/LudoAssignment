using NUnit.Framework;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public int stepsToMove = 0;

    [Header("Components")]
    public GameObject selectedPiece;


    // select token from click
    public void SelectPiece(GameObject token)
    {
        PieceController piece = token.GetComponent<PieceController>();
        if (!TurnSystem.Instance.IsCurrentPlayerPiece(piece)) return;

        selectedPiece = token;
        //Debug.Log($"Selected piece: {token.name}");
        MoveSelectedPiece();
    }

    public void MoveSelectedPiece()
    {
        if (selectedPiece == null || stepsToMove <= 0)
        {
            Debug.LogWarning("No piece selected or invalid step count.");
            return;
        }

        PieceController piece = selectedPiece.GetComponent<PieceController>();
        selectedPiece = null; // Clear selection after moving

        if (piece != null)
        {
            piece.MoveBySteps(stepsToMove);
        }

        // When movement is done, decide if extra turn or switch
        piece.onMovementComplete = () =>
        {
            if (TurnSystem.Instance.rolledSix)
            {
                TurnSystem.Instance.StartTurn(TurnSystem.Instance.currentPlayer); // Extra turn
            }
            else
            {
                TurnSystem.Instance.SwitchTurn(); // Normal switch
            }
        };

    }

    public bool HasValidMove(int steps)
    {
        GameObject[] pieces= TurnSystem.Instance.currentPlayer == TurnSystem.Player.Green ? BoardHandler.Instance.greenPieces : BoardHandler.Instance.bluePieces;
        
        foreach (var token in pieces)
        {
            var piece = token.GetComponent<PieceController>();
            if (piece.CanMove(steps))
                return true;
        }
        return false;
    }

}
