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

            CheckPowerUps(xPos, yPos, currentPlayer, out bool redrawTile);
            DropCoin(xPos, yPos, boardHeight, currentPlayer, redrawTile);
            CheckWinCondition(xPos, yPos);
            UpdateCurrentPlayer();
        }
    }

    public void CheckPowerUps(int xPos, int yPos, int playerId, out bool redrawTile) {
        Player player = GetPlayer(playerId);
        redrawTile = false;

        if(gameBoard.GetCellType(xPos, yPos) != TileType.Normal) {
            TileType type = gameBoard.GetCellType(xPos, yPos);

            if(player.GivePlayerPowerUp(type, out int slotIdx)) {
                Debug.Log("gave player tile " + type.ToString());

                redrawTile = true;
                HUD.Instance.AddPowerUp(playerId, slotIdx, type);
                gameBoard.UseTile(xPos, yPos); 
            }

            // Debug.Log("give powerup to player " + playerId);
        }
    }

    void DropCoin(int xPos, int yPos, int yInit, int playerId, bool redrawTile) {
        Player player = GetPlayer(playerId);

        Display.Instance.DrawCoin(gameBoard, xPos, yInit, yPos, player, redrawTile);
    }

    void CheckWinCondition(int xPos, int yPos) {
        // check win condition
        if (gameBoard.CheckWin(xPos, yPos, currentPlayer)) {
            EndGame();
        }
    }

    void CheckWinConditionBoard() {
        // check win condition over entire board
        for(int xPos = 0; xPos < gameBoard.width; xPos++) {
            for(int yPos = 0; yPos < gameBoard.height; yPos++) {
                if (gameBoard.CheckWin(xPos, yPos, currentPlayer)) {
                    EndGame();
                }
            }
        }
    }

    public void HandlePowerUp(TileType type, int playerId, int slotIdx) {
        if (playerId != currentPlayer) return;

        Player player = GetPlayer(playerId);

        gameBoard.UsePowerUp(type);
        player.ClearSlot(slotIdx);
        HUD.Instance.RemovePowerUp(playerId, slotIdx);
        Display.Instance.DrawFullBoard(gameBoard, player1, player2); // TODO: not always true

        if(type == TileType.RotateBoard || type == TileType.FlipBoard) RedropCoins();
    }

    void RedropCoins() {
        // drop coins after or flip rotation

        for(int x = 0; x < gameBoard.width; x++) {
            for(int y = 0; y < gameBoard.height; y++) {
                int playerId = gameBoard.GetCellOccupancy(x, y);

                if(playerId != 0) {
                    // re-drop the coin
                    gameBoard.SetCellOccupancy(x, y, 0);
                    gameBoard.PlaceCoin(x, playerId, out int yOut);

                    CheckPowerUps(x, yOut, playerId, out bool redrawTile);
                    DropCoin(x, yOut, y, playerId, redrawTile);
                }
            }
        }

        CheckWinConditionBoard();
    }

    void UpdateCurrentPlayer() {
        currentPlayer = (currentPlayer == 1) ? 2 : 1;
    }

    Player GetPlayer(int playerId) {
        return playerId == 1 ? player1 : player2;
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
