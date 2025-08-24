using Unity.Netcode;
using UnityEngine;
using static System.Scripts.GameManager;


namespace System.Scripts
{
    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance;
        public event System.Action<PlayerType> OnPlayerWon;
        public enum PlayerType { None, Green, Blue }
        //public event Action OnMatchRestarted;

        [Header("Components")]
        public PlayerController greenPlayerController;
        public PlayerController bluePlayerController;
        public PlayerType currentPlayer;

        [Space(15)]
        public int numberOfPlayers = 2; // Currently only supports 2 players
        public int numberOfPiecesPerPlayer = 2; // Number of pieces per player
        public bool gameEnded = false;

        public PlayerController[] allPlayers 
        {
            get { return new PlayerController[] { greenPlayerController, bluePlayerController }; } 
        }

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        [ServerRpc(RequireOwnership =false)]
        public void StartTurnServerRpc(PlayerType player)
        {
            if (!IsHost) return;
            StartTurnClientRpc(player);
        }

        [ClientRpc]
        public void StartTurnClientRpc(PlayerType player)
        {
            //print($"Current player: {player}");
            currentPlayer = player;
            UpdatePiecesZ();
            TurnSystem.Instance.StartTurn(player);
        }

        public void SwitchTurn()
        {
            //print($"Switching turn from {currentPlayer}" );
            currentPlayer = (currentPlayer == PlayerType.Green) ? PlayerType.Blue : PlayerType.Green;
            StartTurnServerRpc(currentPlayer);
        }

        public void UpdatePiecesZ() // Update Z position of pieces based on current player
        {
            foreach (var player in allPlayers) // allPlayers is a list of PlayerControllers
            {
                bool isCurrent = (player == GetCurrentPlayer());
                var pieces = (player.playerType.Value == PlayerType.Green) ? BoardHandler.Instance.greenPieces : BoardHandler.Instance.bluePieces;

                foreach (var piece in pieces)
                {
                    piece.GetComponent<PieceController>().SetPieceZ(isCurrent);
                }
            }
        }

        public void RegisterPlayerController(PlayerController pc)
        {
            if (pc.playerType.Value == PlayerType.Green)
                greenPlayerController = pc;
            else if (pc.playerType.Value == PlayerType.Blue)
                bluePlayerController = pc;
        }

        public PlayerController GetCurrentPlayer()
        {
            return currentPlayer == PlayerType.Green ? greenPlayerController : bluePlayerController;
        }

        public PlayerType GetLocalPlayer()
        {
            if (IsHost) // this works for host
            {
                if (OwnerClientId == NetworkManager.Singleton.LocalClientId)
                    return PlayerType.Green;
                else
                    return PlayerType.Blue;
            }
            else // this works for client
            {
                if (NetworkManager.ServerClientId == NetworkManager.Singleton.LocalClientId)
                    return PlayerType.Green;
                else
                    return PlayerType.Blue;
            }
        }

        public bool DoesPieceBelongToCurrentPlayer(PieceController piece)
        {
            return (currentPlayer == PlayerType.Green && piece.pieceOwner == PlayerType.Green)
                || (currentPlayer == PlayerType.Blue && piece.pieceOwner == PlayerType.Blue);
        }

        public void PlayerWon(PlayerType player)
        {
            Debug.Log($"Player {player} wins!");
            OnPlayerWon?.Invoke(player);

            // Stop game or show win screen later
            gameEnded = true;
            TurnSystem.Instance.dice.SetDiceInteractive(false);
        }

        public void RestartMatch() // for future updates...
        {
            gameEnded = false;

            // BoardHandler.Instance.ResetBoard();
            // TurnSystem.Instance.ResetTurns();
            // TurnSystem.Instance.dice.SetDiceInteractive(false);
            // OnMatchRestarted?.Invoke();
        }

    }
}

