using UnityEngine;
using System.Scripts;
using static System.Scripts.GameManager;

public class PlayerController : MonoBehaviour
{
    public int stepsToMove = 0;
    public int homeCount = 0;

    [Header("Components")]
    public GameObject piecePrefab;
    public GameObject selectedPiece;
    public Player player;


    // select token from click
    public void SelectPiece(GameObject token)
    {
        PieceController piece = token.GetComponent<PieceController>();

        // Check if the selected piece belongs to the current player
        if (!GameManager.Instance.DoesPieceBelongToCurrentPlayer(piece))
        {
            print("Selected piece not belongs to the current player");
            return;
        }

        print("Piece selected");
        selectedPiece = token;
        MoveSelectedPiece();
    }


    public void MoveSelectedPiece()
    {
        if (selectedPiece == null || stepsToMove < 0)
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
                TurnSystem.Instance.StartTurn(Instance.currentPlayer); // Extra turn
            }
            else
            {
                //print($"Player switched from {this}");
                GameManager.Instance.SwitchTurn(); // Normal switch
            }
        };

    }

    public bool HasValidMove(int steps)
    {
        GameObject[] pieces= Instance.currentPlayer == Player.Green ? BoardHandler.Instance.greenPieces : BoardHandler.Instance.bluePieces;
        
        foreach (var token in pieces)
        {
            var piece = token.GetComponent<PieceController>();
            if (piece.CanMove(steps))
                return true;
        }
        return false;
    }

    public void CheckWinCondition(GameObject[] pieces)
    {
        foreach (GameObject token in pieces)
        {
            if (token.GetComponent<PieceController>().hasReachedHome)
                homeCount++;
        }

        if (homeCount >= GameManager.Instance.numberOfPiecesPerPlayer) // number of pieces per player in game
        {
            GameManager.Instance.PlayerWon(Instance.currentPlayer);
        }
    }
}
