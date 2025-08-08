using UnityEngine;
using System.Collections.Generic;

public class BoardHandler : MonoBehaviour
{
    public static BoardHandler Instance { get; private set; }

    [Header("Initial Tiles")]
    public List<Transform> initialGreenPoints;
    public List<Transform> initialBluePoints;

    [Header("Path Tiles")]
    public List<Transform> greenPathPoints;
    public List<Transform> bluePathPoints;
    public List<Transform> safeTiles;


    [Header("Pieces")]
    public GameObject[] greenPieces;
    public GameObject[] bluePieces;
    public List<PieceController> allPieces; // active pieces


    [Space(15)]
    public int pathPointsCount = 38; // Total path points for each player


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // avoid duplicates
            return;
        }
        Instance = this;
    }


    void Start()
    {
        foreach (GameObject token in greenPieces)
            PlacePiecesAtStart(token, TurnSystem.Player.Green);

        foreach (GameObject token in bluePieces)
            PlacePiecesAtStart(token, TurnSystem.Player.Blue);
    }

    public void PlacePiecesAtStart(GameObject token,TurnSystem.Player player)
    {
        List<Transform>initialPoints = (player == TurnSystem.Player.Green) ? initialGreenPoints : initialBluePoints;
        GameObject[] pieces = (player == TurnSystem.Player.Green) ? greenPieces : bluePieces;

        token.transform.position = initialPoints[System.Array.IndexOf(pieces, token)].position;
    }

    public List<PieceController> GetOpponentPieceOnTile(Transform movingPieceCurrentTile, PieceController movingPiece)
    {
        List<PieceController> capturedPieces = new List<PieceController>();
        foreach (var piece in allPieces)
        {
            if (piece == movingPiece) continue;

            if (piece.CurrentTile == movingPieceCurrentTile && piece.playerController != movingPiece.playerController)
            {
                capturedPieces.Add(piece);
            }
        }
        return capturedPieces;
    }

    public bool IsSafeTile(Transform tile)
    {
        return safeTiles.Contains(tile);
    }

    public void ResetBoard()
    {
        /*foreach (GameObject token in greenPieces)
        {
            token.GetComponent<PieceController>().ResetPiece();
            token.transform.position = initialGreenPoints[System.Array.IndexOf(greenPieces, token)].position;
        }

        foreach (GameObject token in bluePieces)
        {
            token.GetComponent<PieceController>().ResetPiece();
            token.transform.position = initialBluePoints[System.Array.IndexOf(bluePieces, token)].position;
        }*/
    }

}
