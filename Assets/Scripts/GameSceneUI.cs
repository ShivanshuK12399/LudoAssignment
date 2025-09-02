using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Scripts;
using static System.Scripts.GameManager;
using System.Collections;

public class GameSceneUI : MonoBehaviour
{
    [Header("Network UI")]
    public GameObject networkPanel;
    public Button HostBtn;
    public Button ClientBtn;
    public TextMeshProUGUI joincodeText;

    [Header("Win Screen")]
    public GameObject endPanel;
    public TMP_Text gameEndedText;

    [Space(15)]
    public Button mainMenuBtn;


    void Start()
    {
        //networkPanel.SetActive(true);
        // hook main menu button
        mainMenuBtn.onClick.AddListener(() =>
        {
            StartCoroutine(Disconnect());
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

        Instance.EndScreen += ShowEndPanel;
        joincodeText.text = DataManager.Instance.joinCode;
    }

    public void ShowEndPanel(PlayerType player, int index) // index: 0 = disconnects, 1 = wins
    {
        switch (index)
        {
            case 0:
                // disconnects
                gameEndedText.text = $"{player} Player \nDisconnects!";
                break;

            case 1: 
                // wins
                gameEndedText.text = $"{player} Player Wins!";
                break;

            default:
                break;
        }
        endPanel.SetActive(true);
    }

    IEnumerator Disconnect()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            yield return null;
            Destroy(NetworkManager.Singleton.gameObject);
        }

        // After shutdown, load MainMenu
        SceneManager.LoadScene("MainMenu");
    }

}
