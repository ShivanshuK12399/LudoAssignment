using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DataManager : MonoBehaviour
{
    // carry game mode information between scenes
    public static DataManager Instance;

    public int matchEntryFee;
    public enum GameMode { Singlplayer,Paidmatch};
    public GameMode gameMode;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Hook gameMode to DropDown
        MainMenuUI.Instance.gameModeDropdown.onValueChanged.AddListener(index => 
        {
            gameMode = (GameMode)index;
        });

        MainMenuUI.Instance.gameModeDropdown.value = (int)gameMode; // Dropdown first value will be set by gameMode 
    }

}
