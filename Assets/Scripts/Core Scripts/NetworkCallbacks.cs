using System.Runtime.CompilerServices;
using System.Scripts;
using Unity.Netcode;
using UnityEngine;

public class NetworkCallbacks : MonoBehaviour
{
    private void Start()
    {
        NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
    }

    private void HandleServerStarted()
    {
        if (NetworkManager.Singleton.IsHost)
            Debug.Log("✅ Host started the game.");
    }

    private void HandleClientConnected(ulong clientId)
    {
        Debug.Log($"🔗 Client {clientId} connected.");

        if (NetworkManager.Singleton.IsHost && NetworkManager.Singleton.ConnectedClients.Count == 2)
        {
            Debug.Log("2 players connected, Preparing Board...");
            BoardHandler.Instance.PrepareBoard();
            GameManager.Instance.StartTurnServerRpc(GameManager.PlayerType.Green);
        }
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        Debug.Log($"❌ Client {clientId} disconnected.");
    }
}
