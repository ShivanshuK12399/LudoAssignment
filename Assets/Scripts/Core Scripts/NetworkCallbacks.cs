using System;
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
        {
            Debug.Log("✅ Host started the game.");
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        Debug.Log($"🔗 Client {clientId} connected.");

        if (NetworkManager.Singleton.IsHost && NetworkManager.Singleton.ConnectedClients.Count == 2)
        {
            foreach (ulong id in NetworkManager.Singleton.ConnectedClientsIds)
            {
                // creating player object for each connected client instead of NetworkManager's automatic spawning
                GameObject player = Instantiate(DataManager.Instance.playerPrefab);
                player.GetComponent<NetworkObject>().SpawnAsPlayerObject(id, true);
            }

            Debug.Log("2 players connected, Preparing Board...");
            BoardHandler.Instance.PrepareBoard();
            GameManager.Instance.StartTurnServerRpc(GameManager.PlayerType.Green);
        }
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        Debug.Log($"❌ Client {clientId} disconnected.");
        try
        {
            GameManager.Instance.OnClientDisconnects(clientId);
            GameManager.Instance.clientHistory.RemoveAt((int)clientId);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error handling client disconnect: {e.Message}");
        }
    }
}
