using UnityEngine;
using System;

public class TurnSystem : MonoBehaviour
{
    [Header("Components")]
    public DiceController dice;
    public PlayerController playerController;

    [Header("Dice Holders")]
    [SerializeField] private Transform greenDiceHolder;
    [SerializeField] private Transform blueDiceHolder;

    public enum Player { Green, Blue }
    [Space(15)]
    public Player currentPlayer = Player.Green;
    public event Action<Player> OnTurnChanged;

    private bool rolledSix = false;
    private bool hasMovedAfterSix = false;


    void Start()
    {
        StartTurn(Player.Green);
    }

    public void OnDiceRolled(int number)
    {
        rolledSix = (number == 6);
        hasMovedAfterSix = false;

        playerController.stepsToMove = number;
    }

    public void OnPieceMoved()
    {
        hasMovedAfterSix = true;
        dice.rolledNumber = 0;
        playerController.stepsToMove = 0;

        if (rolledSix)
        {
            // Grant extra turn
            Debug.Log("Extra Turn!");
            return;
        }
        SwitchTurn();
    }

    public void TryEndTurn()
    {
        if (rolledSix && !hasMovedAfterSix)
        {
            // Wait for piece movement
            return;
        }
        else if (rolledSix && hasMovedAfterSix)
        {
            // Allow extra turn
            StartTurn(currentPlayer);
        }
        else
        {
            SwitchTurn();
        }
    }

    private void StartTurn(Player player)
    {
        currentPlayer = player;
        rolledSix = false;
        hasMovedAfterSix = false;

        dice.SetDiceInteractive(true); // Allow roll at start
        MoveDiceToPlayer(player);      // Move dice to correct holder

        OnTurnChanged?.Invoke(currentPlayer);
        Debug.Log($"Turn: {currentPlayer}");
    }

    private void SwitchTurn()
    {
        Player next = currentPlayer == Player.Green ? Player.Blue : Player.Green;
        StartTurn(next);
    }

    public bool IsCurrentPlayerPiece(PieceController piece)
    {
        return (currentPlayer == Player.Green && piece.pieceColor == PieceController.PlayerColor.Green)
            || (currentPlayer == Player.Blue && piece.pieceColor == PieceController.PlayerColor.Blue);
    }

    private void MoveDiceToPlayer(Player player)
    {
        Transform holder = (player == Player.Green) ? greenDiceHolder : blueDiceHolder;
        dice.transform.SetParent(holder);
        dice.transform.localPosition = new Vector3(0, 0, -0.5f);
    }
}
