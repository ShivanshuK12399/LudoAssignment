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
    public NetworkVariable<PlayerType> playerType = new NetworkVariable<PlayerType>
    (
       PlayerType.None,
       NetworkVariableReadPermission.Everyone,    // who can read
       NetworkVariableWritePermission.Server      // who can write
    );
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
        if (IsServer) // only server decides
        {
            if (OwnerClientId == NetworkManager.ServerClientId)
                playerType.Value = PlayerType.Green;
            else
                playerType.Value = PlayerType.Blue;
        }

        Debug.Log($"{playerType} Player spawned.");

        var setup = playerSetups.First(s => s.type == playerType.Value);
        piecePrefab = setup.piecePrefab;
        GameManager.Instance.RegisterPlayerController(this);
    }

    public void SetMyPieces(GameObject[] pieces)
    {
        myPieces.Clear();
        myPieces.AddRange(pieces);
        //Debug.Log($"{playerType} received {myPieces.Count} pieces");
    }

    public List<GameObject> GetMyPieces() => myPieces;

    // select token from click
    public void SelectPiece(GameObject token)
    {
        if (!IsOwner) return; // Not my local player

        if (playerType.Value != Instance.currentPlayer)
        {
            Debug.Log("Not your turn!");
            return;
        }

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
            if (TurnSystem.Instance.rolledSix.Value)
            {
                Instance.StartTurnServerRpc(Instance.currentPlayer);  // Extra turn
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
