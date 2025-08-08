using UnityEngine;
using System.Collections;

public class PieceController : MonoBehaviour
{
    public System.Action onMovementComplete;

    [Header("Components")]
    public BoardHandler boardHandler;
    public PlayerController playerController;
    private TurnSystem turnSystem;

    public enum PlayerColor { Green, Blue }
    [Space(15)]
    public PlayerColor pieceColor;

    private int currentTileIndex = -1; // -1 = not on board yet

    public float moveSpeed = 4f;

    void Start()
    {
        turnSystem = playerController.turnSystem;
    }

    void OnMouseDown()
    {
        if (playerController != null)
        {
            playerController.SelectToken(gameObject);
        }
    }

    public void MoveBySteps(int steps)
    {
        if (boardHandler == null)
        {
            Debug.LogError("BoardHandler not found.");
            return;
        }

        if (currentTileIndex == -1)
        {
            if (steps < 6)
            {
                Debug.Log("Need 6 to enter board.");
                return;
            }
        }

        // Get correct path based on piece color
        var path = pieceColor == PlayerColor.Green ? boardHandler.greenPathPoints : boardHandler.bluePathPoints;

        StartCoroutine(MoveAlongPath(path, steps));
    }

    IEnumerator MoveAlongPath(System.Collections.Generic.List<Transform> path, int steps)
    {
        if (currentTileIndex == -1) steps = 1;

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
            yield return new WaitForSeconds(0.1f);
        }

        turnSystem.OnPieceMoved();
        onMovementComplete?.Invoke(); // Movement complete, listinig this event in Player controller
    }

    public bool CanMove(int steps)
    {
        if (currentTileIndex == -1)
            return steps == 6;

        return currentTileIndex + steps < boardHandler.pathPointsCount;
    }

}
