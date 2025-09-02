using TMPro;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.Services.Core;
using Unity.Services.Relay;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Networking.Transport.Relay;

public class MatchRelay : MonoBehaviour
{
    public Button hostButton;
    public Button joinButton;
    public TextMeshProUGUI joiningStatus;
    public TMP_InputField codeInputField;


    async void Start()
    {
        await UnityServices.InitializeAsync(); // wait for syncing players

        if (!AuthenticationService.Instance.IsSignedIn) // already signed in
        {
            // we need to sign in with google,apple,facbook,etc but lets do it anonymously
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        hostButton.onClick.AddListener(HostRelay);
        joinButton.onClick.AddListener(JoinRelay);
    }


    async void HostRelay() // called when click on host button
    {
        DataManager.Instance.joinCode = await StartHostRelay();
    }
    async void JoinRelay() // called when click on join button
    {
        await StartClientRelay(codeInputField.text);
    }


    async Task<string> StartHostRelay(int maxConnections = 2)
    {
        try
        {
            joiningStatus.text = "Creating room...";
            // allocation - stores allocation details for clients to connect
            // await - suspends execution until the CreateAllocationAsync() task is complet
            // CreateAllocationAsync - requests Unity's Relay Service to allow host to set up serverless multiplayer session.
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);


            // creates a RelayServerData object that contains all necessary connection details.
            // dtls is protocal used for secure connection
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));


            // calls Unity's Relay Service to generate a unique join code for the session
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            if (NetworkManager.Singleton.StartHost())
            {
                NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
                return joinCode;
            }
            else return null;
        }
        catch
        {
            joiningStatus.text = "Creating allocation failed";
            throw;
        }
    }

    async Task<bool> StartClientRelay(string joinCode)
    {
        try
        {
            joiningStatus.text = "Joining...";
            // joinAllocation - Stores details about the joined Relay session like relay server address
            // await - Ensures the method waits for the Relay allocation to complete before continuing
            // joinAllocationSync - Calls Unity Relay Service to join a game session
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

            NetworkManager.Singleton.StartClient();
            return true;
        }
        catch
        {
            joiningStatus.text = "Failed to join";
            return false;
        }
    }
}
