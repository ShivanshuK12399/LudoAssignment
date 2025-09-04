using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace System.Scripts
{
    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance;
        public event System.Action<PlayerType, int> EndScreen;
        public enum PlayerType { None=-1, Green, Blue, Red, Yellow }
        //public event Action OnMatchRestarted;

        [Header("Components")]
        public GameSceneUI gameSceneUI;
        public PlayerController greenPlayerController;
        public PlayerController bluePlayerController;
        public PlayerController redPlayerController;
        public PlayerController yellowPlayerController;
        public PlayerType currentPlayer;

        public PlayerController[] allPlayers
        {
            get { return new PlayerController[] { greenPlayerController, bluePlayerController, redPlayerController, yellowPlayerController }; }
        }

        [Space(15)]
        public int totalPlayers; // Currently only supports 2 players
        public int totalPiecesPerPlayer; // Number of pieces per player
        public bool gameEnded = false;
        public List<GameObject> clientHistory = new List<GameObject>();


        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            totalPlayers=DataManager.Instance.totalPlayers;
            totalPiecesPerPlayer=DataManager.Instance.totalPiecesPerPlayer;
        }

        [ServerRpc(RequireOwnership = false)]
        public void StartTurnServerRpc(PlayerType player)
        {
            if (!IsHost) return;
            StartTurnClientRpc(player);
        }

        [ClientRpc]
        public void StartTurnClientRpc(PlayerType player)
        {
            //Debug.Log($"Current player: {player}");
            currentPlayer = player;
            UpdatePiecesZ();
            TurnSystem.Instance.StartTurn(player);
        }



        [ServerRpc(RequireOwnership = false)]
        public void PlayerWonServerRpc(PlayerType player)
        {
            PlayerWonClientRpc(player);
        }

        [ClientRpc]
        public void PlayerWonClientRpc(PlayerType player)
        {
            Debug.Log($"Player {player} wins!");
            EndScreen?.Invoke(player, 1); // 1 = wins

            // Stop game or show win screen later
            gameEnded = true;
            TurnSystem.Instance.dice.SetDiceInteractive(false);
        }



        public void SwitchTurn()
        {
            //Debug.Log($"Switching turn from {currentPlayer}" );
            currentPlayer = (PlayerType)(((int)currentPlayer + 1) % totalPlayers);
            if(currentPlayer == PlayerType.None) currentPlayer = PlayerType.Green; // Skip None

            StartTurnServerRpc(currentPlayer);
        }

        public void UpdatePiecesZ() // Update Z position of pieces based on current player
        {
            foreach (var player in allPlayers) // allPlayers is a list of PlayerControllers
            {
                if (player == null) continue; // Skip if player is not assigned

                bool isCurrent = (player == GetCurrentPlayer());
                var pieces = new GameObject[0];
                switch (player.playerType.Value)
                {
                    case PlayerType.Green:
                        pieces = BoardHandler.Instance.greenPieces;
                        break;
                    case PlayerType.Blue:
                        pieces = BoardHandler.Instance.bluePieces;
                        break;
                    case PlayerType.Red:
                        pieces = BoardHandler.Instance.redPieces;
                        break;
                    case PlayerType.Yellow:
                        pieces = BoardHandler.Instance.yellowPieces;
                        break;
                    default:
                        Debug.Log("Invalid Player");
                        break;
                }

                foreach (var piece in pieces)
                {
                    piece.GetComponent<PieceController>().SetPieceZ(isCurrent);
                }
            }
        }

        public void RegisterPlayerController(PlayerController pc)
        {
            switch (pc.playerType.Value)
            {
                case PlayerType.Green:
                    greenPlayerController = pc;
                    break;
                case PlayerType.Blue:
                    bluePlayerController = pc;
                    break;
                case PlayerType.Red:
                    redPlayerController = pc;
                    break;
                case PlayerType.Yellow:
                    yellowPlayerController = pc;
                    break;
                default:
                    Debug.Log("Invalid Player");
                    break;
            }
            //print($"registerd: {pc.playerType.Value}"); 
        }

        public void OnClientDisconnects(ulong clientId)
        {
            PlayerController player = clientHistory[(int)clientId].GetComponent<PlayerController>();

            gameSceneUI.ShowEndPanel(player.playerType.Value, 0); // 0 = disconnects
        }

        public void RestartMatch() // for future updates...
        {
            gameEnded = false;

            // BoardHandler.Instance.ResetBoard();
            // TurnSystem.Instance.ResetTurns();
            // TurnSystem.Instance.dice.SetDiceInteractive(false);
            // OnMatchRestarted?.Invoke();
        }



        public PlayerController GetCurrentPlayer()
        {
            switch (currentPlayer)
            {
                case PlayerType.Green:
                    return greenPlayerController;
                case PlayerType.Blue:
                    return bluePlayerController;
                case PlayerType.Red:
                    return redPlayerController;
                case PlayerType.Yellow:
                    return yellowPlayerController;
                default:
                    Debug.Log("Invalid Current Player");
                    return null;
            }
        }
        public bool DoesPieceBelongToCurrentPlayer(PieceController piece)
        {
            return (currentPlayer == PlayerType.Green && piece.pieceOwner == PlayerType.Green)
                || (currentPlayer == PlayerType.Blue && piece.pieceOwner == PlayerType.Blue)
                || (currentPlayer == PlayerType.Red && piece.pieceOwner == PlayerType.Red)
                || (currentPlayer == PlayerType.Yellow && piece.pieceOwner == PlayerType.Yellow);
        }
    }
}

