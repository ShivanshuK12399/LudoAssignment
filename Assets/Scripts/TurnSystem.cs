using System;
using UnityEngine;

public class TurnSystem : MonoBehaviour
{
    public static TurnSystem Instance { get; private set; }


    [Header("Components")]
    public DiceController dice;

    [Header("Dice Holders")]
    [SerializeField] private Transform greenDiceHolder;
    [SerializeField] private Transform blueDiceHolder;

    public enum Player { Green, Blue }
    public event Action<TurnSystem.Player> OnTurnChanged;
    public bool rolledSix = false;
    public bool hasMovedAfterSix = false;

    void Awake()
    {
        Instance = this;
    }


    public void StartTurn(Player player)
    {
        dice.rolledNumber = GameManager.Instance.GetCurrentPlayer().stepsToMove = 0;
        rolledSix = false;
        hasMovedAfterSix = false;

        if (GameManager.Instance.gameEnded) return; // Don't change turn if game ended

        MoveDiceToPlayer(player);      // Move dice to correct holder
        dice.SetDiceInteractive(true); // Allow roll at start

        OnTurnChanged?.Invoke(player);
        Debug.Log($"Turn: {player}");
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
    void SwitchTurn()
    {
        //print($"Player switched from {this}");
        GameManager.Instance.SwitchTurn(); 
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

    private void MoveDiceToPlayer(Player player)
    {
        Transform holder = (player == Player.Green) ? greenDiceHolder : blueDiceHolder;
        dice.transform.SetParent(holder);
        dice.transform.localPosition = new Vector3(0, 0, -0.5f);
    }
}
