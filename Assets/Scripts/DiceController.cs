using UnityEngine;
﻿using System.Scripts;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections; // If using UI Button (optional)

public class DiceController : NetworkBehaviour
{
    [Header("Components")]
    public SpriteRenderer diceRenderer; // Drag your dice sprite object here
    public Animator animator; // Unity Animator with rolling animation

    [Header("Dice Faces")]
    public Sprite[] diceFaces; // 6 sprites for 1-6 faces (ordered)

    [Space(15)]
    public bool getDiceNumManually; // chech YES if want to select num manually
    public int rolledNumber = 0; // To be used by PlayerController

    private bool isRolling = false;
    private bool canRoll = true;


    void OnMouseDown()
    {
        // Only allow the active player (and local owner) to roll
        var currentPlayer = GameManager.Instance.GetCurrentPlayer();
        if (!currentPlayer.IsOwner) return; // Not my local player
        if (currentPlayer.playerType.Value != GameManager.Instance.currentPlayer) return; // Not my turn

        if (!isRolling && canRoll)
        {
            RollDiceServerRpc();
        }
    }

    [ServerRpc(RequireOwnership =false)]
    public void RollDiceServerRpc()
    {
        if (getDiceNumManually) { } // set the rolled number manually in inspector
        else rolledNumber = Random.Range(1, 6); // Random number between 1-6

        RollDiceClientRpc(rolledNumber);
    }

    [ClientRpc]
    public void RollDiceClientRpc(int rolledNumber)
    {
        animator.enabled = true;
        isRolling = true;
        canRoll = false;
        animator.Play("DiceRoll", -1, 0f); // Name of your dice animation clip
        StartCoroutine(OnDiceAnimationComplete(rolledNumber, 0.25f));
    }

    private IEnumerator OnDiceAnimationComplete(int rolledNumber, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (rolledNumber < 1 || rolledNumber > 6) diceRenderer.sprite = diceFaces[0]; // Fallback to 1 if rolled num is invalid
        else diceRenderer.sprite = diceFaces[rolledNumber - 1]; // Show result face

        animator.enabled = false;
        isRolling = false;

        Debug.Log($"Rolled: {rolledNumber}");

        TurnSystem.Instance.OnDiceRolled(rolledNumber);
        SetDiceInteractive(false);
    }

    public void SetDiceInteractive(bool state)
    {
        canRoll = state;
    }
}
