using System.Collections.Generic;
using System.Linq;
using System.Scripts;
using Unity.Netcode;
using UnityEditor.U2D.Aseprite;
using UnityEngine;
using static System.Scripts.GameManager;

public class PlayerController : NetworkBehaviour
{
    public int stepsToMove = 0;
    public int homeCount = 0;

    [Header("Components")]
    public PlayerType playerType;
    public GameObject selectedPiece;
    public GameObject piecePrefab;
    [SerializeField] private List<GameObject> myPieces = new List<GameObject>();

    [System.Serializable]
    public class PlayerSetup
    {
        public PlayerType type;
        public GameObject piecePrefab;
    }
    public List<PlayerSetup> playerSetups;


    public override void OnNetworkSpawn()
    {
        if (IsHost) // this works for host
        {
            if (OwnerClientId == NetworkManager.Singleton.LocalClientId)
                playerType = PlayerType.Green;
            else
                playerType = PlayerType.Blue;
        }
        else if (IsOwner) // this works for client
        {
            if (NetworkManager.ServerClientId == NetworkManager.Singleton.LocalClientId)
                playerType = PlayerType.Green;
            else
                playerType = PlayerType.Blue;
        }

        // Find setup entry for this playerType
        var setup = playerSetups.First(s => s.type == playerType);
        piecePrefab = setup.piecePrefab;

        Debug.Log($"{playerType} Player spawned.");

        GameManager.Instance.RegisterPlayerController(this);
    }

    public void SetMyPieces(GameObject[] pieces)
    {
        myPieces.Clear();
        myPieces.AddRange(pieces);
        Debug.Log($"{playerType} received {myPieces.Count} pieces");
    }

    public List<GameObject> GetMyPieces() => myPieces;

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
        GameObject[] pieces= Instance.currentPlayer == PlayerType.Green ? BoardHandler.Instance.greenPieces : BoardHandler.Instance.bluePieces;
        
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

        if (homeCount >= Instance.numberOfPiecesPerPlayer) // number of pieces per player in game
        {
            Instance.PlayerWon(Instance.currentPlayer);
        }
    }
}
