using UnityEngine;
using Unity.Netcode;


namespace System.Scripts
{
    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance;
        public event System.Action<PlayerType> OnPlayerWon;
        public enum PlayerType { Green, Blue }
        //public event Action OnMatchRestarted;

        [Header("Components")]
        public PlayerController greenPlayerController;
        public PlayerController bluePlayerController;
        public PlayerType currentPlayer;

        [Space(15)]
        public int numberOfPlayers = 2; // Currently only supports 2 players
        public int numberOfPiecesPerPlayer = 2; // Number of pieces per player
        public bool gameEnded = false;

        private PlayerController[] allPlayers;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        void Start()
        {
            allPlayers = new PlayerController[] { greenPlayerController, bluePlayerController };
        }

        [ClientRpc]
        public void StartTurnClientRpc(PlayerType player)
        {
            //print($"Current player: {player}");
            currentPlayer = player;
            UpdatePiecesZ();
            TurnSystem.Instance.StartTurn(player);
        }

        public void RegisterPlayerController(PlayerController pc)
        {
            if (pc.playerType == PlayerType.Green)
                greenPlayerController = pc;
            else if (pc.playerType == PlayerType.Blue)
                bluePlayerController = pc;
        }

        public void SwitchTurn()
        {
            currentPlayer = (currentPlayer == PlayerType.Green) ? PlayerType.Blue : PlayerType.Green;
            StartTurnClientRpc(currentPlayer);
        }

        public PlayerController GetCurrentPlayer()
        {
            return currentPlayer == PlayerType.Green ? greenPlayerController : bluePlayerController;
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

        public void UpdatePiecesZ() // Update Z position of pieces based on current player
        {
            foreach (var player in allPlayers) // allPlayers is a list of PlayerControllers
            {
                bool isCurrent = (player == GetCurrentPlayer());
                var pieces = (player.playerType == PlayerType.Green) ? BoardHandler.Instance.greenPieces : BoardHandler.Instance.bluePieces;

                foreach (var piece in pieces)
                {
                    piece.GetComponent<PieceController>().SetPieceZ(isCurrent);
                }
            }
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

