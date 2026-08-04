using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Setup")]
    public int boardWidth;
    public int boardHeight;

    [Header("Player-chosen Settings")]
    public Color boardColor;
    public Color player1Color;
    public Color player2Color;

    Board gameBoard;
    Player player1;
    Player player2;
    int currentPlayer;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this.gameObject);
        }
        else {
            Instance = this;
        }
    }

    void Start()
    {
        gameBoard = new Board(boardWidth, boardHeight, boardColor);
        player1 = new Player(player1Color);
        player2 = new Player(player2Color);
        currentPlayer = 1; // TODO: random

        Display.Instance.DrawFullBoard(gameBoard, player1, player2);
    }

    void OnEnable() {
        BoardInputHandler.Instance.MouseClicked += HandlePlayerInput;
    }

    void OnDisable() {
        BoardInputHandler.Instance.MouseClicked -= HandlePlayerInput;
    }

    void HandlePlayerInput(Vector2 mousePos) {
        int xPos = Mathf.FloorToInt(mousePos.x);

        if (gameBoard.PlaceCoin(xPos, currentPlayer, out int yPos)) {
            Player player = (currentPlayer == 1) ? player1 : player2;

            Display.Instance.DrawTile(gameBoard, xPos, yPos, player);

            // check win condition
            if (gameBoard.CheckWin(xPos, yPos, currentPlayer)) {
                EndGame();
            }

            UpdateCurrentPlayer();
        }
    }

    void UpdateCurrentPlayer() {
        currentPlayer = (currentPlayer == 1) ? 2 : 1; // TODO: beautiful
    }

    void EndGame() {
        Debug.Log("victory player " + currentPlayer);
    }
    
}
