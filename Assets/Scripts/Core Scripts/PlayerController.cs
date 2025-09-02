using System.Collections.Generic;
using System.Linq;
using System.Scripts;
using Unity.Netcode;
using UnityEngine;
using static System.Scripts.GameManager;

public class PlayerController : NetworkBehaviour
{
    public NetworkVariable<PlayerType> playerType = new NetworkVariable<PlayerType>
    (
       PlayerType.None,
       NetworkVariableReadPermission.Everyone,    // who can read
       NetworkVariableWritePermission.Server      // who can write
    );

    [System.Serializable]
    public class PlayerSetup
    {
        public PlayerType type;
        public GameObject piecePrefab;
    }

    [Header("Components")]
    public GameObject selectedPiece;
    public GameObject piecePrefab;
    public List<PlayerSetup> playerSetups;

    public int stepsToMove = 0;
    public int homeCount = 0;


    public override void OnNetworkSpawn()
    {
        if (IsServer) // only server decides
        {
            if (OwnerClientId == NetworkManager.ServerClientId)
                playerType.Value = PlayerType.Green;
            else
                playerType.Value = PlayerType.Blue;
        }

        //Debug.Log($"{playerType.Value} Player spawned.");

        var setup = playerSetups.First(s => s.type == playerType.Value);
        piecePrefab = setup.piecePrefab;
        
        Instance.RegisterPlayerController(this); // register this player controller in game manager
        Instance.clientHistory.Add(this.gameObject); // keep track of clients by their clientId index
    }


    // select token from click
    public void SelectPiece(GameObject token)
    {
        if (!IsOwner) return; // Not my local player

        PieceController piece = token.GetComponent<PieceController>();

        if (playerType.Value != Instance.currentPlayer)
        {
            Debug.Log("Not your turn!");
            return;
        }

        if (!Instance.DoesPieceBelongToCurrentPlayer(piece))
        {
            Debug.Log("Selected piece not belongs to the current player");
            return;
        }

        if(TurnSystem.Instance.hasMoved.Value)
        {
            Debug.Log("Already moved a piece");
            return;
        }

        Debug.Log("Piece selected");
        selectedPiece = token;
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
            if (TurnSystem.Instance.rolledSix.Value)
            {
                Instance.StartTurnServerRpc(Instance.currentPlayer);  // Extra turn
            }
            else
            {
                //Debug.Log($"Player switched from {this}");
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


    public void CheckWinCondition(GameObject piece)
    {
        if (piece.GetComponent<PieceController>().hasReachedHome)
            homeCount++;

        if (homeCount >= Instance.numberOfPiecesPerPlayer) // number of pieces per player in game
        {
            Instance.PlayerWonServerRpc(Instance.currentPlayer);
        }
    }
}
