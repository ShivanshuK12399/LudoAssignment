using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DataManager : MonoBehaviour
{
    // carry game mode information between scenes
    public static DataManager Instance;

    public string joinCode;
    public GameObject playerPrefab;

    [Space(15)]
    public int totalPlayers = 2;
    public int totalPiecesPerPlayer = 2;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
