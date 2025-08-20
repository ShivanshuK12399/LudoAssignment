using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Scripts;
using static System.Scripts.GameManager;

public class PieceController : MonoBehaviour
{
    public Transform CurrentTile { get; private set; }
    public System.Action onMovementComplete;

    [Header("Components")]
    public PlayerController playerController;

    [Space(15)]
    public Player pieceOwner;
    public bool hasReachedHome = false;

    private int currentTileIndex = -1; // -1 = not on board yet
    private float moveSpeed = 6f;

    private void Start()
    {
        playerController = (pieceOwner==Player.Green) ? Instance.greenPlayerController: Instance.bluePlayerController;
    }

    void OnMouseDown()
    {
        if (playerController != null)
        {
            playerController = Instance.GetCurrentPlayer();
            playerController.SelectPiece(gameObject);
        }
    }

    public void UpdateTile(Transform newTile)
    {
        // tracks piece tile
        CurrentTile = newTile;
    }

    public void MoveBySteps(int steps)
    {
        if (BoardHandler.Instance == null) return;

        if (currentTileIndex == -1 && steps != 6)
        {
            Debug.Log("Need 6 to enter board.");
            return;
        }

        // Get correct path based on piece color
        var path = pieceOwner == Player.Green ? BoardHandler.Instance.greenPathPoints : BoardHandler.Instance.bluePathPoints;

        StartCoroutine(MoveAlongPath(path, steps));
    }

    IEnumerator MoveAlongPath(System.Collections.Generic.List<Transform> path, int steps)
    {
        if (currentTileIndex == -1) steps = 1; // Move only 1 step when get on board from base

        while (steps > 0)
        {
            int nextIndex = currentTileIndex + 1;
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

            currentTileIndex = nextIndex;
            steps--;

            if (currentTileIndex + 1 == path.Count) // currentTileIndex + 1 is used beacuse indexing start from 0
            {
                Debug.Log($"{name} has reached home.");
                hasReachedHome = true;

                GameObject[] pieces = Instance.currentPlayer == Player.Green ? BoardHandler.Instance.greenPieces : BoardHandler.Instance.bluePieces;
                Instance.GetCurrentPlayer().CheckWinCondition(pieces);
                TurnSystem.Instance.StartTurn(Instance.currentPlayer); // Extra turn

                goto Skip;  // skipping updates(like TurnChange, DiceChange) on piece reaching home
            }

            yield return new WaitForSeconds(0.1f);
        }

        // Movement complete
        
        onMovementComplete?.Invoke();  // listinig this event in Player controller
        UpdateTile(path[currentTileIndex]);
        CheckCapture();
Skip:
        TurnSystem.Instance.OnPieceMoved();
    }

    public void CheckCapture()
    {
        Transform currentTile = CurrentTile;
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
                //Debug.Log($"Captured {piece}");
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

        currentTileIndex = -1;
        Player player=(pieceOwner==Player.Green)? Player.Green : Player.Blue;

        BoardHandler.Instance.PlacePiecesAtStart(this.gameObject, player);
        UpdateTile(null);
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

        if (currentTileIndex == -1)
            return steps == 6;

        return currentTileIndex + steps < BoardHandler.Instance.pathPointsCount;
    }

    public void ResetPiece()
    {
        /*currentStep = 0;
        isOnBoard = false;
        hasReachedHome = false;*/
    }

}
