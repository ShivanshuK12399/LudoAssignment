using UnityEngine;
using UnityEngine.UI; // If using UI Button (optional)

public class DiceController : MonoBehaviour
{
    [Header("Components")]
    public PlayerController playerController;
    public TurnSystem turnSystem; 
    public SpriteRenderer diceRenderer; // Drag your dice sprite object here
    public Animator animator; // Unity Animator with rolling animation

    [Header("Dice Faces")]
    public Sprite[] diceFaces; // 6 sprites for 1-6 faces (ordered)

    public int rolledNumber = 0; // To be used by PlayerController

    private bool isRolling = false;
    private bool canRoll = false;

    void OnMouseDown()
    {
        if (!isRolling && canRoll)
        {
            RollDice();
            animator.enabled = true;
        }
    }

    public void SetDiceInteractive(bool state)
    {
        canRoll = state;
    }

    public void RollDice()
    {
        isRolling = true;
        rolledNumber = Random.Range(1, 7); // Random number between 1-6
        animator.Play("DiceRoll", -1, 0f); // Name of your dice animation clip
    }

    public void OnDiceAnimationComplete()
    {
        animator.enabled = false; 
        diceRenderer.sprite = diceFaces[rolledNumber - 1]; // Show result face
        isRolling = false;

        //Debug.Log($"Rolled: {rolledNumber}");
        turnSystem.OnDiceRolled(rolledNumber);
        SetDiceInteractive(false);
    }

}
