using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Scripts;
using static System.Scripts.GameManager;

public class PieceController : NetworkBehaviour
{
    public System.Action onMovementComplete;

    [Header("Components")]
    public PlayerController playerController;

    [Space(15)]
    public PlayerType pieceOwner;
    public bool hasReachedHome = false;

    public NetworkVariable<int> currentTileIndex = new NetworkVariable<int>(
        -1,  // -1 = not on board yet
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private float moveSpeed = 6f;

    private void Start()
    {
        playerController = (pieceOwner==PlayerType.Green) ? Instance.greenPlayerController: Instance.bluePlayerController;
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
        var path = pieceOwner == PlayerType.Green ? BoardHandler.Instance.greenPathPoints : BoardHandler.Instance.bluePathPoints;

        StartCoroutine(MoveAlongPath(path, steps));
    }

    IEnumerator MoveAlongPath(List<Transform> path, int steps)
    {
        if (currentTileIndex.Value == -1) steps = 1; // Move only 1 step when get on board from base

        while (steps > 0)
        {
            int nextIndex = currentTileIndex.Value + 1;
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

            ChangeCurrentTileIndexServerRpc(nextIndex);
            steps--;

            if (currentTileIndex.Value + 1 == path.Count) // currentTileIndex + 1 is used beacuse indexing start from 0
            {
                Debug.Log($"{name} has reached home.");
                hasReachedHome = true;

                GameObject[] pieces = Instance.currentPlayer == PlayerType.Green ? BoardHandler.Instance.greenPieces : BoardHandler.Instance.bluePieces;
                Instance.GetCurrentPlayer().CheckWinCondition(pieces);
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
            print("Captured opponent");
            //print($"Player switched from {this}");
            Instance.SwitchTurn();
        }
    }

    public void SendToBase()
    {
        // Send captured piece to its respective base

        ChangeCurrentTileIndexServerRpc(-1);
        PlayerType player=(pieceOwner==PlayerType.Green)? PlayerType.Green : PlayerType.Blue;

        ulong pieceId=this.gameObject.GetComponent<NetworkObject>().NetworkObjectId;
        BoardHandler.Instance.PlacePiecesAtStartServerRpc(pieceId, player);
    }

    public Transform GetCurrentTile()
    {
        var path = pieceOwner == PlayerType.Green ?
                   BoardHandler.Instance.greenPathPoints :
                   BoardHandler.Instance.bluePathPoints;

        return (currentTileIndex.Value >= 0 && currentTileIndex.Value < path.Count)
            ? path[currentTileIndex.Value]
            : null;
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
