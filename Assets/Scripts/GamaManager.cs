using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player Controllers")]
    public PlayerController greenPlayerController;
    public PlayerController bluePlayerController;


    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Assign owners to tokens
        AssignTokenOwners();
    }

    void AssignTokenOwners()
    {
        foreach (GameObject token in BoardHandler.Instance.greenPieces)
        {
            PieceController piece = token.GetComponent<PieceController>();
            piece.playerController = greenPlayerController;
        }

        foreach (GameObject token in BoardHandler.Instance.bluePieces)
        {
            PieceController piece = token.GetComponent<PieceController>();
            piece.playerController = bluePlayerController;
        }
    }

    public PlayerController GetCurrentPlayer()
    {
        if (TurnSystem.Instance.currentPlayer == TurnSystem.Player.Green)
            return greenPlayerController;
        else
            return bluePlayerController;
    }

}
