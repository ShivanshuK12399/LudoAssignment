using System;
using System.Scripts;
using Unity.Netcode;
using UnityEngine;
using static System.Scripts.GameManager;

public class TurnSystem : NetworkBehaviour
{
    public static TurnSystem Instance { get; private set; }


    [Header("Components")]
    public DiceController dice;

    [Header("Dice Holders")]
    [SerializeField] private Transform greenDiceHolder;
    [SerializeField] private Transform blueDiceHolder;

    public event Action<PlayerType> OnTurnChanged;
    public NetworkVariable<bool> rolledSix = new(false);
    public NetworkVariable<bool> hasMovedAfterSix = new(false);


    void Awake()
    {
        Instance = this;
    }


    public void StartTurn(PlayerType player)
    {
        //print($"I {GameManager.Instance.GetLocalPlayer()} is owner");
        dice.rolledNumber = 0;
        GameManager.Instance.GetCurrentPlayer().stepsToMove = 0;
        RolledSixServerRpc(false); //rolledSix = false;
        HasMovedAfterSixServerRpc(false); //hasMovedAfterSix = false;

        if (GameManager.Instance.gameEnded) return; // Don't change turn if game ended

        MoveDiceToPlayer(player);      // Move dice to correct holder


        // Only allow the dice to be interactive for the local active player
        var currentPlayer = GameManager.Instance.GetCurrentPlayer();
        if (currentPlayer.IsOwner) dice.SetDiceInteractive(true);  // Allow roll at start
        else dice.SetDiceInteractive(false);

        OnTurnChanged?.Invoke(player);
        //Debug.Log($"Turn: {player}");
    }

    public void OnDiceRolled(int number)
    {
        // when dice is rolled its gets called

        if (!IsHost) return; // Only host should process turn logic

        RolledSixServerRpc(number == 6); //rolledSix = (number == 6);
        HasMovedAfterSixServerRpc(false); //hasMovedAfterSix = false;

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
        // When piece movement is completed its gets called

        HasMovedAfterSixServerRpc(true); // hasMovedAfterSix = true;
        dice.rolledNumber = 0;
        GameManager.Instance.GetCurrentPlayer().stepsToMove = 0;

        if (rolledSix.Value)
        {
            // Grant extra turn
            dice.SetDiceInteractive(true);
            return;
        }
    }

    private void MoveDiceToPlayer(PlayerType player) 
    {
        // Moves dice parent to current payer

        Transform holder = (player == PlayerType.Green) ? greenDiceHolder : blueDiceHolder;
        //dice.transform.SetParent(holder);
        //dice.transform.localPosition = new Vector3(0, 0, -0.5f);
        dice.transform.position = new Vector3(holder.position.x, holder.position.y, holder.position.z - 0.5f);
    }

    [ServerRpc(RequireOwnership =false)]
    void RolledSixServerRpc(bool value)
    {
        rolledSix.Value = value;
    }

    [ServerRpc(RequireOwnership =false)]
    void HasMovedAfterSixServerRpc(bool value)
    {
        hasMovedAfterSix.Value = value;
    }
}
