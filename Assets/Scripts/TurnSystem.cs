using System;
using UnityEngine;
using static TurnSystem;

public class TurnSystem : MonoBehaviour
{
    public static TurnSystem Instance { get; private set; }


    [Header("Components")]
    public DiceController dice;

    [Header("Dice Holders")]
    [SerializeField] private Transform greenDiceHolder;
    [SerializeField] private Transform blueDiceHolder;

    public enum Player { Green, Blue }
    [Space(15)]
    public Player currentPlayer = Player.Green;
    public event Action<Player> OnTurnChanged;

    public bool rolledSix = false;
    private bool hasMovedAfterSix = false;

    void Awake()
    {
        Instance = this;
    }


    void Start()
    {
        StartTurn(Player.Green);
    }

    public void OnDiceRolled(int number)
    {
        rolledSix = (number == 6);
        hasMovedAfterSix = false;

        GameManager.Instance.GetCurrentPlayer().stepsToMove = number;

        bool hasMovableToken = GameManager.Instance.GetCurrentPlayer().HasValidMove(number);

        if (!hasMovableToken)
        {
            Debug.Log("No valid tokens to move. Switching turn...");
            Invoke(nameof(SwitchTurn), 0.5f);
        }
    }

    public void OnPieceMoved()
    {
        hasMovedAfterSix = true;
        dice.rolledNumber = 0;
        GameManager.Instance.GetCurrentPlayer().stepsToMove = 0;

        if (rolledSix)
        {
            // Grant extra turn
            dice.SetDiceInteractive(true);
            return;
        }
    }

    public void StartTurn(Player player)
    {
        dice.rolledNumber = GameManager.Instance.GetCurrentPlayer().stepsToMove = 0;
        currentPlayer = player;
        rolledSix = false;
        hasMovedAfterSix = false;

        MoveDiceToPlayer(player);      // Move dice to correct holder
        dice.SetDiceInteractive(true); // Allow roll at start

        OnTurnChanged?.Invoke(currentPlayer);
        Debug.Log($"Turn: {currentPlayer}");
    }

    public void SwitchTurn()
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
