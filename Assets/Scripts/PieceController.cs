using UnityEngine;
using System.Collections;

public class PieceController : MonoBehaviour
{
    public System.Action onMovementComplete;

    [Header("Components")]
    public PlayerController playerController;

    public enum PlayerColor { Green, Blue }
    [Space(15)]
    public PlayerColor pieceColor;

    private int currentTileIndex = -1; // -1 = not on board yet

    public float moveSpeed = 4f;

    void OnMouseDown()
    {
        if (playerController != null)
        {
            playerController.SelectPiece(gameObject);
        }
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
        var path = pieceColor == PlayerColor.Green ? BoardHandler.Instance.greenPathPoints : BoardHandler.Instance.bluePathPoints;

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
            yield return new WaitForSeconds(0.1f);
        }

        TurnSystem.Instance.OnPieceMoved();
        onMovementComplete?.Invoke(); // Movement complete, listinig this event in Player controller
    }

    public bool CanMove(int steps)
    {
        if (currentTileIndex == -1)
            return steps == 6;

        return currentTileIndex + steps < BoardHandler.Instance.pathPointsCount;
    }

}
