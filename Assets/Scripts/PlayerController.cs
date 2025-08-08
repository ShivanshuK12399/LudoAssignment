using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public int stepsToMove = 0;

    [Header("Components")]
    public GameObject selectedToken;
    public TurnSystem turnSystem;


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
        if (piece != null)
        {
            piece.MoveBySteps(stepsToMove);
        }
    }
}
