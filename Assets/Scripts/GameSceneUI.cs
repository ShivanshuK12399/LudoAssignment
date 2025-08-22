using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Scripts;
using static System.Scripts.GameManager;

public class GameSceneUI : MonoBehaviour
{
    [Header("Network UI")]
    public GameObject networkPanel;
    public Button HostBtn;
    public Button ClientBtn;

    [Header("Win Screen")]
    public GameObject winScreen;
    public TMP_Text winText, paidMatchText;

    [Space(15)]
    public Button mainMenuBtn;


    void Start()
    {
        networkPanel.SetActive(true);
        // hook main menu button
        mainMenuBtn.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("MainMenu");
        });

        HostBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            networkPanel.SetActive(false);
        });

        ClientBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            networkPanel.SetActive(false);
        });


        if (DataManager.Instance!=null) // checking game mode 
        {
            GameMode(DataManager.Instance.gameMode);
        }

        Instance.OnPlayerWon += ShowWinScreen;
    }

    void ShowWinScreen(GameManager.PlayerType winner)
    {
        winText.text = $"{winner} Player Wins!";
        winScreen.SetActive(true);
    }

    void GameMode(DataManager.GameMode gameMode)
    {
        float price = (float)(DataManager.Instance.matchEntryFee + (0.9 * DataManager.Instance.matchEntryFee));

        if (DataManager.Instance.gameMode == DataManager.GameMode.Paidmatch)
        {
            paidMatchText.text = $"Paid Match - \r\nWinner gets ₹{price} (after 10% fee)";
            return;
        }
        else paidMatchText.text = gameMode.ToString();
    }
}
