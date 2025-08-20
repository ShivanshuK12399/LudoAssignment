using UnityEngine;
using Unity.Netcode;


namespace System.Scripts
{
    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance;
        public event System.Action<Player> OnPlayerWon;
        public enum Player { Green, Blue }
        //public event Action OnMatchRestarted;

        [Header("Components")]
        public PlayerController greenPlayerController;
        public PlayerController bluePlayerController;
        public Player currentPlayer;

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

            // Start the game with the first turn
            StartTurn(Player.Green);
        }

        public void StartTurn(Player player)
        {
            print($"Current player: {player}");
            currentPlayer = player;
            UpdatePiecesZ();
            TurnSystem.Instance.StartTurn(player);
        }

        public void SwitchTurn()
        {
            currentPlayer = (currentPlayer == Player.Green) ? Player.Blue : Player.Green;
            StartTurn(currentPlayer);
        }

        public PlayerController GetCurrentPlayer()
        {
            return currentPlayer == Player.Green ? greenPlayerController : bluePlayerController;
        }

        public bool DoesPieceBelongToCurrentPlayer(PieceController piece)
        {
            return (currentPlayer == Player.Green && piece.pieceOwner == Player.Green)
                || (currentPlayer == Player.Blue && piece.pieceOwner == Player.Blue);
        }

        public void PlayerWon(Player player)
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
                var pieces = (player.player == Player.Green) ? BoardHandler.Instance.greenPieces : BoardHandler.Instance.bluePieces;

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

