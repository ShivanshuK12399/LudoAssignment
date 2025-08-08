using System;
using UnityEngine;
using static PieceController;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public event System.Action<TurnSystem.Player> OnPlayerWon;
    public event Action OnMatchRestarted;

    public PlayerController greenPlayerController;
    public PlayerController bluePlayerController;
    public TurnSystem.Player currentPlayer;
    public bool gameEnded = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Assign owners to tokens
        AssignTokenOwners();
        StartTurn(TurnSystem.Player.Green);
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

    public void StartTurn(TurnSystem.Player player)
    {
        //print($"Current player: {player}");
        currentPlayer = player;
        TurnSystem.Instance.StartTurn(player);
    }

    public void SwitchTurn()
    {
        currentPlayer = (currentPlayer == TurnSystem.Player.Green)? TurnSystem.Player.Blue : TurnSystem.Player.Green;
        StartTurn(currentPlayer);
    }

    public PlayerController GetCurrentPlayer()
    {
        return currentPlayer == TurnSystem.Player.Green ? greenPlayerController : bluePlayerController;
    }

    public bool DoesPieceBelongToCurrentPlayer(PieceController piece)
    {
        return (currentPlayer == TurnSystem.Player.Green && piece.pieceOwner == TurnSystem.Player.Green)
            || (currentPlayer == TurnSystem.Player.Blue && piece.pieceOwner == TurnSystem.Player.Blue);
    }

    public void PlayerWon(TurnSystem.Player player)
    {
        Debug.Log($"Player {player} wins!");
        OnPlayerWon?.Invoke(player);

        // Stop game or show win screen later
        gameEnded = true;
        TurnSystem.Instance.dice.SetDiceInteractive(false);
    }

    public void RestartMatch() // for future updates...
    {
        gameEnded = false;

        // BoardHandler.Instance.ResetBoard();
        // TurnSystem.Instance.ResetTurns();
        // TurnSystem.Instance.dice.SetDiceInteractive(false);
        // OnMatchRestarted?.Invoke();
    }

}
