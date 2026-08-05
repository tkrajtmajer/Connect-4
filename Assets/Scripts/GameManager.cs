using UnityEngine;
using System;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Setup")]
    public int boardWidth;
    public int boardHeight;
    public TileProbList tileProbList = new TileProbList();
    public Dictionary<TileType, float> tileProbDict;

    [Header("Player-chosen Settings")]
    public Color boardColor;
    public Color player1Color;
    public Color player2Color;

    [Header("Debug")] // temp
    public Color colorRotate;
    public Color colorFlip;
    public Color colorBlowup;
    public Color colorSwap;

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
        tileProbDict = tileProbList.ToDictionary();

        gameBoard = new Board(boardWidth, boardHeight, boardColor, tileProbDict);
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
            bool redrawTile = false;

            if(gameBoard.GetCellType(xPos, yPos) != TileType.Normal) {
                TileType type = gameBoard.GetCellType(xPos, yPos);

                if(player.GivePlayerPowerUp(type, out int slotIdx)) {
                    Debug.Log("gave player tile " + type.ToString());

                    redrawTile = true;
                    HUD.Instance.AddPowerUp(currentPlayer, slotIdx, type);
                    gameBoard.UseTile(xPos, yPos); // TODO: gonna keep it here for now; probably best if it can still be used after for ex board flip
                }
            }

            Display.Instance.DrawCoin(gameBoard, xPos, boardHeight, yPos, player, redrawTile);

            // check win condition
            if (gameBoard.CheckWin(xPos, yPos, currentPlayer)) {
                EndGame();
            }

            UpdateCurrentPlayer();
        }
    }

    public void HandlePowerUp(TileType type, int playerId, int currentSlot) {

    }

    void UpdateCurrentPlayer() {
        currentPlayer = (currentPlayer == 1) ? 2 : 1; // TODO: beautiful
    }

    void EndGame() {
        Debug.Log("victory player " + currentPlayer);
    }
    
}

[Serializable]
public class TileProbList {
    public TileProbability[] tileProbabilities;

    public Dictionary<TileType, float> ToDictionary() {
        Dictionary<TileType, float> dict = new Dictionary<TileType, float>();
        foreach(var tp in tileProbabilities) dict[tp.tileType] = tp.tileProbability;
        return dict; 
    }
}

[Serializable]
public class TileProbability {
    public TileType tileType;
    public float tileProbability;
}
