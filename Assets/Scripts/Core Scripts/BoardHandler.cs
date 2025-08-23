using System.Collections.Generic;
using System.Linq;
using System.Scripts;
using Unity.Netcode;
using UnityEditor.U2D.Aseprite;
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
        if (!IsHost) return; // Only the Host/Server spawns

        // declaring array size
        greenPieces = new GameObject[GameManager.Instance.numberOfPiecesPerPlayer];
        bluePieces = new GameObject[GameManager.Instance.numberOfPiecesPerPlayer];

        // --- Get PlayerControllers ---
        var greenPlayer = GameManager.Instance.greenPlayerController;
        var bluePlayer = GameManager.Instance.bluePlayerController;

        // --- Green Pieces ---
        for (int i = 0; i < GameManager.Instance.numberOfPiecesPerPlayer; i++)
        {
            var piece = Instantiate(greenPlayer.piecePrefab, initialGreenPoints[i].position, Quaternion.identity);
            var netObj = piece.GetComponent<NetworkObject>();

            // Assign ownership to Green Player’s client
            netObj.SpawnWithOwnership(greenPlayer.OwnerClientId);

            greenPieces[i] = piece;
        }

        // --- Blue Pieces ---
        for (int i = 0; i < GameManager.Instance.numberOfPiecesPerPlayer; i++)
        {
            var piece = Instantiate(bluePlayer.piecePrefab, initialBluePoints[i].position, Quaternion.identity);
            var netObj = piece.GetComponent<NetworkObject>();

            // Assign ownership to Blue Player’s client
            netObj.SpawnWithOwnership(bluePlayer.OwnerClientId);

            bluePieces[i] = piece;
        }

        // --- Collect all pieces ---
        allPieces.Clear();
        PieceController[] piecesFound = FindObjectsByType<PieceController>(FindObjectsSortMode.InstanceID);
        foreach (var token in piecesFound)
        {
            allPieces.Add(token);
        }

        // --- Let PlayerControllers know their pieces ---
        greenPlayer.SetMyPieces(greenPieces);
        bluePlayer.SetMyPieces(bluePieces);

        // --- Sync with all clients ---
        ulong[] greenIds = greenPieces.Select(p => p.GetComponent<NetworkObject>().NetworkObjectId).ToArray();
        ulong[] blueIds = bluePieces.Select(p => p.GetComponent<NetworkObject>().NetworkObjectId).ToArray();
        ulong[] allIds = allPieces.Select(p => p.GetComponent<NetworkObject>().NetworkObjectId).ToArray();

        SyncPiecesClientRpc(greenIds, blueIds, allIds);
    }


    [ClientRpc]
    private void SyncPiecesClientRpc(ulong[] greenIds, ulong[] blueIds, ulong[] allIds)
    {
        if (IsHost) return; // Host already has the arrays filled

        print("Client Syncing Pieces...");

        // --- rebuild arrays for every client ---
        greenPieces = greenIds
            .Select(id => NetworkManager.Singleton.SpawnManager.SpawnedObjects[id].gameObject)
            .ToArray();

        bluePieces = blueIds
            .Select(id => NetworkManager.Singleton.SpawnManager.SpawnedObjects[id].gameObject)
            .ToArray();

        allPieces.Clear();
        foreach (var id in allIds)
        {
            var go = NetworkManager.Singleton.SpawnManager.SpawnedObjects[id].gameObject;
            allPieces.Add(go.GetComponent<PieceController>());
        }

        // --- assign pieces to local PlayerController ---
        var myPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerController>();
        if (myPlayer.playerType.Value == PlayerType.Green)
            myPlayer.SetMyPieces(greenPieces);
        else if (myPlayer.playerType.Value == PlayerType.Blue)
            myPlayer.SetMyPieces(bluePieces);
    }


    public void PlacePiecesAtStart(GameObject token,PlayerType player)
    {
        // Sets pieces to its start location

        List<Transform>initialPoints = (player == PlayerType.Green) ? initialGreenPoints : initialBluePoints;
        GameObject[] pieces = (player == PlayerType.Green) ? greenPieces : bluePieces;

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
