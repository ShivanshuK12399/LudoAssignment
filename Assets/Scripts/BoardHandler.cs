using UnityEngine;
using System.Collections.Generic;
using System.Scripts;
using static System.Scripts.GameManager;

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
    private List<PieceController> allPieces=new List<PieceController>(); // active pieces


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
        // Setting array size for pieces
        greenPieces = new GameObject[GameManager.Instance.numberOfPiecesPerPlayer];
        bluePieces = new GameObject[GameManager.Instance.numberOfPiecesPerPlayer];
        for (int i = 0; i < GameManager.Instance.numberOfPiecesPerPlayer; i++)
        {
            greenPieces[i]= Instantiate(GameManager.Instance.greenPlayerController.piecePrefab);
            bluePieces[i] = Instantiate(GameManager.Instance.bluePlayerController.piecePrefab);
        }

        PieceController[] piecesFound = FindObjectsByType<PieceController>(FindObjectsSortMode.InstanceID);
        foreach (var token in piecesFound)
        {
            allPieces.Add(token);
        }


        // Setting pieces intitial location
        foreach (GameObject token in greenPieces)
            PlacePiecesAtStart(token, Player.Green);

        foreach (GameObject token in bluePieces)
            PlacePiecesAtStart(token, Player.Blue);
    }

    public void PlacePiecesAtStart(GameObject token,Player player)
    {
        // Sets pieces to its start location

        List<Transform>initialPoints = (player == Player.Green) ? initialGreenPoints : initialBluePoints;
        GameObject[] pieces = (player == Player.Green) ? greenPieces : bluePieces;

        token.transform.position = initialPoints[System.Array.IndexOf(pieces, token)].position;
    }

    public List<PieceController> GetOpponentPieceOnTile(Transform movingPieceCurrentTile, PieceController movingPiece)
    {
        // Give list of pieces on tile which is to be captured

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
