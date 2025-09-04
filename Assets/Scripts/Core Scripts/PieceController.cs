using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static System.Scripts.GameManager;

public class PieceController : NetworkBehaviour
{
    public NetworkVariable<int> currentTileIndex = new NetworkVariable<int>
    (
        -1,  // -1 = not on board yet
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    public Action onMovementComplete;

    [Header("Components")]
    public PlayerController playerController;

    [Space(15)]
    public PlayerType pieceOwner;
    public bool hasReachedHome = false;
    private float moveSpeed = 6f;
    private int localIndex = -1; // used to get piece localation as network variable delay in syncing


    public override void OnNetworkSpawn()
    {
        switch (pieceOwner)
        {
            case PlayerType.Green:
                playerController = Instance.greenPlayerController;
                break;
            case PlayerType.Blue:
                playerController = Instance.bluePlayerController;
                break;
            case PlayerType.Red:
                playerController = Instance.redPlayerController;
                break;
            case PlayerType.Yellow:
                playerController = Instance.yellowPlayerController;
                break;
            default:
                Debug.Log("Invalid Player");
                break;
        }
    }

    void OnMouseDown()
    {
        if (playerController != null)
        {
            playerController = Instance.GetCurrentPlayer();
            playerController.SelectPiece(gameObject);
        }
    }


    public void MoveBySteps(int steps)
    {
        if (BoardHandler.Instance == null) return;

        if (currentTileIndex.Value == -1 && steps != 6)
        {
            Debug.Log("Need 6 to enter board.");
            return;
        }

        // Get correct path based on piece color
        List<Transform> path = new List<Transform>();
        switch (pieceOwner)
        {
            case PlayerType.Green:
                path = BoardHandler.Instance.greenPathPoints;
                break;
            case PlayerType.Blue:
                path = BoardHandler.Instance.bluePathPoints;
                break;
            case PlayerType.Red:
                path = BoardHandler.Instance.redPathPoints;
                break;
            case PlayerType.Yellow:
                path = BoardHandler.Instance.yellowPathPoints;
                break;
            default:
                Debug.Log("Invalid Player");
                break;
        }

        TurnSystem.Instance.HasMovedServerRpc(true);
        StartCoroutine(MoveAlongPath(path, steps));
    }

    IEnumerator MoveAlongPath(List<Transform> path, int steps)
    {
        if (currentTileIndex.Value == -1) steps = 1; // Move only 1 step when get on board from base

        localIndex= currentTileIndex.Value;
        ChangeCurrentTileIndexServerRpc(localIndex+steps); // setting future tile index before movement cuz network var updates with delay

        while (steps > 0)
        {
            int nextIndex = localIndex + 1;
            if (nextIndex >= path.Count)
            {
                Debug.Log($"{name} has reached the end.");
                yield break;
            }

            Vector3 targetPos = path[nextIndex].position;
            while (Vector3.Distance(transform.position, targetPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                yield return null;
            }

            localIndex = nextIndex;
            steps--;

            if (localIndex + 1 == path.Count) // currentTileIndex + 1 is used beacuse indexing start from 0
            {
                Debug.Log($"{name} has reached home.");
                hasReachedHome = true;

                Instance.GetCurrentPlayer().CheckWinCondition(this.gameObject);
                Instance.StartTurnServerRpc(Instance.currentPlayer);  // Extra turn

                goto Skip;  // skipping updates(like TurnChange, DiceChange) on piece reaching home
            }

            yield return new WaitForSeconds(0.1f);
        }

        // Movement complete
        onMovementComplete?.Invoke();  // listinig this event in Player controller
        CheckCapture();
Skip:
        TurnSystem.Instance.OnPieceMoved();
    }



    public void CheckCapture()
    {
        Transform currentTile = GetCurrentTile();

        if (BoardHandler.Instance.IsSafeTile(currentTile) || this.hasReachedHome)
        {
            // Don't capture piece if on safe tile or reached home
            return;
        }

        List<PieceController> opponent = BoardHandler.Instance.GetOpponentPieceOnTile(currentTile, this);

        if (opponent.Count>0)
        {
            foreach(PieceController piece in opponent)
            {
                piece.SendToBase();
            }
            Debug.Log("Captured opponent");
            //Debug.Log($"Player switched from {this}");
            Instance.SwitchTurn();
        }
    }

    public void SendToBase()
    {
        // Send captured piece to its respective base

        ChangeCurrentTileIndexServerRpc(-1);

        ulong pieceId=this.gameObject.GetComponent<NetworkObject>().NetworkObjectId;
        BoardHandler.Instance.PlacePiecesAtStartServerRpc(pieceId, pieceOwner);
    }



    [ServerRpc(RequireOwnership = false)]
    public void ChangeCurrentTileIndexServerRpc(int value)
    {
        currentTileIndex.Value = value;
    }
    public void SetPieceZ(bool isCurrentPlayer)
    {
        // Make current player's piece appear above the opponent's

        Vector3 pos = transform.position;
        pos.z = isCurrentPlayer ? -2f : -1f;
        transform.position = pos;
    }
    public Transform GetCurrentTile()
    {
        List<Transform> path = new List<Transform>();
        switch (pieceOwner)
        {
            case PlayerType.Green:
                path = BoardHandler.Instance.greenPathPoints;
                break;
            case PlayerType.Blue:
                path = BoardHandler.Instance.bluePathPoints;
                break;
            case PlayerType.Red:
                path = BoardHandler.Instance.redPathPoints;
                break;
            case PlayerType.Yellow:
                path = BoardHandler.Instance.yellowPathPoints;
                break;
            default:
                Debug.Log("Invalid Player");
                break;
        }

        return (currentTileIndex.Value >= 0 && currentTileIndex.Value < path.Count)
            ? path[currentTileIndex.Value]
            : null;
    }
    public bool CanMove(int steps)
    {
        // Checks if player can move

        if (currentTileIndex.Value == -1)
            return steps == 6;

        return currentTileIndex.Value + steps < BoardHandler.Instance.pathPointsCount;
    }
    public void ResetPiece()
    {
        /*currentStep = 0;
        isOnBoard = false;
        hasReachedHome = false;*/
    }

}
