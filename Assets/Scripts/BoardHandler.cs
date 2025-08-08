using UnityEngine;
using System.Collections.Generic;

public class BoardHandler : MonoBehaviour
{
    [Header("Initial Points")]
    public List<Transform> initialGreenPoints;
    public List<Transform> initialBluePoints;

    [Header("Path Points")]
    public List<Transform> greenPathPoints;
    public List<Transform> bluePathPoints;

    [Header("Pieces")]
    public GameObject[] greenPieces;
    public GameObject[] bluePieces;

    void Start()
    {
        PlacePiecesAtStart();
    }

    public Transform GetTileAt(int index, List<Transform> pathPoints)
    {
        if (index >= 0 && index < pathPoints.Count)
            return pathPoints[index];
        return null;
    }

    void PlacePiecesAtStart()
    {
        foreach (GameObject token in greenPieces)
            token.transform.position = initialGreenPoints[System.Array.IndexOf(greenPieces, token)].position;

        foreach (GameObject token in bluePieces)
            token.transform.position = initialBluePoints[System.Array.IndexOf(bluePieces, token)].position;
    }

}
