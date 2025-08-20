using System.Collections.Generic;
using System.Linq;
using System.Scripts;
using Unity.Netcode;
using UnityEngine;
using static System.Scripts.GameManager;

public class BoardHandler : NetworkBehaviour
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
    public List<PieceController> allPieces=new List<PieceController>(); // active pieces


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


    public void PrepareBoard()
    {
        if (!IsHost) return;

        // declaring array size
        greenPieces = new GameObject[GameManager.Instance.numberOfPiecesPerPlayer];
        bluePieces = new GameObject[GameManager.Instance.numberOfPiecesPerPlayer];

        // creating pieces at initial points in network
        for (int i = 0; i < GameManager.Instance.numberOfPiecesPerPlayer; i++)
        {
            greenPieces[i]= Instantiate(GameManager.Instance.greenPlayerController.piecePrefab, initialGreenPoints[i].position,Quaternion.identity);
            greenPieces[i].GetComponent<NetworkObject>().Spawn(true);

            bluePieces[i] = Instantiate(GameManager.Instance.bluePlayerController.piecePrefab, initialBluePoints[i].position, Quaternion.identity);
            bluePieces[i].GetComponent<NetworkObject>().Spawn(true);
        }

        PieceController[] piecesFound = FindObjectsByType<PieceController>(FindObjectsSortMode.InstanceID);
        foreach (var token in piecesFound)
        {
            allPieces.Add(token);
        }

        // collect IDs so clients can re-link objects
        ulong[] greenIds = greenPieces.Select(p => p.GetComponent<NetworkObject>().NetworkObjectId).ToArray();
        ulong[] blueIds = bluePieces.Select(p => p.GetComponent<NetworkObject>().NetworkObjectId).ToArray();
        ulong[] allIds = allPieces.Select(p => p.GetComponent<NetworkObject>().NetworkObjectId).ToArray();

        // sync with all clients
        SyncPiecesClientRpc(greenIds, blueIds, allIds);
    }

    [ClientRpc]
    private void SyncPiecesClientRpc(ulong[] greenIds, ulong[] blueIds, ulong[] allIds)
    {
        if (IsHost) return;

        print("Client Syncing Pieces...");
        // rebuild arrays for every client
        greenPieces = greenIds.Select(id => NetworkManager.Singleton.SpawnManager.SpawnedObjects[id].gameObject).ToArray();
        bluePieces = blueIds.Select(id => NetworkManager.Singleton.SpawnManager.SpawnedObjects[id].gameObject).ToArray();

        // rebuild allPieces list
        allPieces.Clear();
        foreach (var id in allIds)
        {
            var go = NetworkManager.Singleton.SpawnManager.SpawnedObjects[id].gameObject;
            allPieces.Add(go.GetComponent<PieceController>());
        }
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
