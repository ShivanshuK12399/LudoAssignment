using System;
using System.Scripts;
using Unity.Netcode;
using UnityEngine;
using static System.Scripts.GameManager;

public class TurnSystem : NetworkBehaviour
{
    public static TurnSystem Instance { get; private set; }
    public event Action<PlayerType> OnTurnChanged;

    public NetworkVariable<bool> rolledSix = new(false);
    public NetworkVariable<bool> hasMoved = new(false);

    [Header("Components")]
    public DiceController dice;
    [SerializeField] private Transform greenDiceHolder;
    [SerializeField] private Transform blueDiceHolder;


    void Awake()
    {
        Instance = this;
    }


    public void StartTurn(PlayerType player)
    {
        //print($"I {GameManager.Instance.GetLocalPlayer()} is owner");
        dice.rolledNumber = GameManager.Instance.GetCurrentPlayer().stepsToMove = 0;
        
        RolledSixServerRpc(false);
        HasMovedServerRpc(false);

        if (GameManager.Instance.gameEnded) return; // Don't change turn if game ended

        MoveDiceToPlayer(player);      // Move dice to correct holder

        // Only allow the dice to be interactive for the local active player
        var currentPlayer = GameManager.Instance.GetCurrentPlayer();
        if (currentPlayer.IsOwner) dice.SetDiceInteractive(true);  // Allow roll at start
        else dice.SetDiceInteractive(false);

        OnTurnChanged?.Invoke(player);
        //Debug.Log($"Turn: {player}");
    }

    private void MoveDiceToPlayer(PlayerType player)
    {
        // Moves dice parent to current payer

        Transform holder = (player == PlayerType.Green) ? greenDiceHolder : blueDiceHolder;
        //dice.transform.SetParent(holder);
        //dice.transform.localPosition = new Vector3(0, 0, -0.5f);
        dice.transform.position = new Vector3(holder.position.x, holder.position.y, holder.position.z - 0.5f);
    }



    public void OnDiceRolled(int number)
    {
        // when dice is rolled its gets called
        GameManager.Instance.GetCurrentPlayer().stepsToMove = number;

        if (!IsHost) return; // Only host should process turn logic

        RolledSixServerRpc(number == 6);
        HasMovedServerRpc(false);

        bool hasMovableToken = GameManager.Instance.GetCurrentPlayer().HasValidMove(number);

        if (!hasMovableToken)
        {
            //Debug.Log("No valid tokens to move. Switching turn...");
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

        dice.rolledNumber = 0;
        GameManager.Instance.GetCurrentPlayer().stepsToMove = 0;

        if (rolledSix.Value)
        {
            // Grant extra turn
            dice.SetDiceInteractive(true);
            return;
        }
    }



    [ServerRpc(RequireOwnership =false)]
    void RolledSixServerRpc(bool value)
    {
        rolledSix.Value = value;
    }

    [ServerRpc(RequireOwnership =false)]
    public void HasMovedServerRpc(bool value)
    {
        hasMoved.Value = value;
    }
}
