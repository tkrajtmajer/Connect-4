using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Setup")]
    public int boardWidth;
    public int boardHeight;

    [Header("Player Settings")]
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
        gameBoard = new Board(boardWidth, boardHeight);
        player1 = new Player(player1Color);
        player2 = new Player(player2Color);
        currentPlayer = 1; // TODO: random

        Display.Instance.DrawBoard(gameBoard, player1, player2);
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
            Display.Instance.DrawBoard(gameBoard, player1, player2);

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
