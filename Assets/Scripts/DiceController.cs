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
    private bool canRoll = true;

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
        canRoll = false;
        animator.Play("DiceRoll", -1, 0f); // Name of your dice animation clip
        Invoke(nameof(OnDiceAnimationComplete), 0.25f);
    }

    public void OnDiceAnimationComplete()
    {
        rolledNumber = Random.Range(5, 7); // Random number between 1-6
        animator.enabled = false; 
        diceRenderer.sprite = diceFaces[rolledNumber - 1]; // Show result face
        isRolling = false;

        //Debug.Log($"Rolled: {rolledNumber}");
        turnSystem.OnDiceRolled(rolledNumber);
        SetDiceInteractive(false);
    }

}
