using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuUI : MonoBehaviour
{
    public static MainMenuUI Instance;

    [Header("UI Elements")]
    public Button playButton;
    public TMP_Dropdown gameModeDropdown;
    public GameObject paidMatchPanel;

    [Space(15)]
    public float paymentAnimationTime;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // Show starting currency in drop down
        gameModeDropdown.options[1].text = $"Play for ₹{DataManager.Instance.matchEntryFee}";

        // Hook Play button
        playButton.onClick.AddListener(() =>
        {
            StartCoroutine(LoadGameScene());
        });
    }

    IEnumerator LoadGameScene()
    {
        switch (gameModeDropdown.value)
        {
            case 0: // Singleplayer
                SceneManager.LoadScene("GameScene");
                yield break;

            case 1: // Paid Match
                paidMatchPanel.SetActive(true);
                yield return new WaitForSeconds(paymentAnimationTime);
                SceneManager.LoadScene("GameScene");
                break;

            default:
                break;
        }
    }
}
