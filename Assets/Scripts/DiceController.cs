using UnityEngine;
using UnityEngine.UI; // If using UI Button (optional)

public class DiceController : MonoBehaviour
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
        if (!isRolling && canRoll)
        {
            RollDice();
            animator.enabled = true;
        }
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
        if (getDiceNumManually) { } // set the rolled number manually in inspector
        else rolledNumber = Random.Range(1, 7); // Random number between 1-6

        if(rolledNumber < 1 || rolledNumber > 6) diceRenderer.sprite = diceFaces[0]; // Fallback to 1 if rolled num is invalid
        else diceRenderer.sprite = diceFaces[rolledNumber - 1]; // Show result face

        animator.enabled = false;
        isRolling = false;

        //Debug.Log($"Rolled: {rolledNumber}");
        TurnSystem.Instance.OnDiceRolled(rolledNumber);
        SetDiceInteractive(false);
    }

    public void SetDiceInteractive(bool state)
    {
        canRoll = state;
    }
}
