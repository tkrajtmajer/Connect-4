using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

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
    public Color boardBgColor;
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
    internal GameState gameState;

    TileType pendingPowerUpType;
    int pendingPowerUpSlot;
    int pendingPowerUpPlayerId;

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

        gameBoard = new Board(boardWidth, boardHeight, boardColor, boardBgColor, tileProbDict);
        player1 = new Player(player1Color);
        player2 = new Player(player2Color);
        currentPlayer = 1; // TODO: random
        gameState = GameState.Playing;

        Display.Instance.DrawFullBoard(gameBoard);
    }

    void OnEnable() {
        BoardInputHandler.Instance.MouseClicked += HandlePlayerInput;
    }

    void OnDisable() {
        BoardInputHandler.Instance.MouseClicked -= HandlePlayerInput;
    }

    void HandlePlayerInput(Vector2 mousePos) {

        mousePos += new Vector2(gameBoard.width / 2f, gameBoard.height / 2f);

        int xPos = Mathf.FloorToInt(mousePos.x);
        int yPos = Mathf.FloorToInt(mousePos.y);
        
        if(gameState == GameState.Playing) {
            if (gameBoard.PlaceCoin(xPos, currentPlayer, out int yOut)) {

                CheckPowerUps(xPos, yOut, currentPlayer, out bool redrawTile);
                DropCoin(xPos, yOut, boardHeight, currentPlayer, redrawTile);
                CheckWinCondition(xPos, yOut);
                UpdateCurrentPlayer();
            }
        }
        else if(gameState == GameState.Waiting) {
            Debug.Log("waiting");
            if(gameBoard.GetCellOccupancy(xPos, yPos) == currentPlayer) {
                // can use the powerup, else needs valid position
                if(pendingPowerUpType == TileType.BlowUp) {
                    HandleBlowup(pendingPowerUpType, pendingPowerUpSlot, pendingPowerUpPlayerId, xPos, yPos);
                    HUD.Instance.HideCancelPowerUp();
                }
                else if(pendingPowerUpType == TileType.SwapNeighbor) {
                    HandleSwap(pendingPowerUpType, pendingPowerUpSlot, pendingPowerUpPlayerId, xPos, yPos);
                    HUD.Instance.HideCancelPowerUp();
                }
            }
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
            EndGame(currentPlayer);
        }
    }

    void CheckWinConditionBoard() {
        // check win condition over entire board
        for(int xPos = 0; xPos < gameBoard.width; xPos++) {
            for(int yPos = 0; yPos < gameBoard.height; yPos++) {
                int cellOccupancyId = gameBoard.GetCellOccupancy(xPos, yPos);

                if (cellOccupancyId != 0 && gameBoard.CheckWin(xPos, yPos, cellOccupancyId)) {
                    EndGame(cellOccupancyId);
                }
            }
        }
    }

    public void HandlePowerUp(TileType type, int playerId, int slotIdx) {
        if (playerId != currentPlayer) return;

        switch (type) {
            case TileType.RotateBoard:
                StartCoroutine(HandleRotation(type, slotIdx, playerId));
                break;
            case TileType.FlipBoard:
                StartCoroutine(HandleFlip(type, slotIdx, playerId));
                break;

            case TileType.BlowUp:
                pendingPowerUpType = TileType.BlowUp;
                pendingPowerUpSlot = slotIdx;
                pendingPowerUpPlayerId = playerId;
                gameState = GameState.Waiting; // wait for additional input
                HUD.Instance.EnableCancellingPowerUp();
                break;
            
             case TileType.SwapNeighbor:
                pendingPowerUpType = TileType.SwapNeighbor;
                pendingPowerUpSlot = slotIdx;
                pendingPowerUpPlayerId = playerId;
                gameState = GameState.Waiting; // wait for additional input
                HUD.Instance.EnableCancellingPowerUp();
                break;

            default:
                break;
        }
    }

    IEnumerator HandleRotation(TileType type, int slotIdx, int playerId) {
        yield return StartCoroutine(Display.Instance.RotateBoard()); // wait for board to rotate visually, then update logic

        gameBoard.RotateBoard();
        UpdateHUD(type, slotIdx, playerId);
        Display.Instance.ResetRotation();
        RedrawBoard();
    }

    IEnumerator HandleFlip(TileType type, int slotIdx, int playerId) {
        yield return StartCoroutine(Display.Instance.FlipBoard());

        gameBoard.FlipBoard();
        UpdateHUD(type, slotIdx, playerId);
        Display.Instance.ResetFlip();
        RedrawBoard();
    }

    // TODO: add way to undo waiting condition
    void HandleBlowup(TileType type, int slotIdx, int playerId, int centerX, int centerY) {
        // add way to blow up coins either w animation or code(?)

        gameBoard.BlowUpCells(centerX, centerY);
        UpdateHUD(type, slotIdx, playerId);
        RedrawBoard();
    }

    void HandleSwap(TileType type, int slotIdx, int playerId, int centerX, int centerY) {

        gameBoard.RandomSwapNeighbor(centerX, centerY, playerId);
        UpdateHUD(type, slotIdx, playerId);
        RedrawBoard();
    }

    void UpdateHUD(TileType type, int slotIdx, int playerId) {
        Player player = GetPlayer(playerId);

        player.ClearSlot(slotIdx);
        HUD.Instance.RemovePowerUp(playerId, slotIdx);
    }

    void RedrawBoard() {
        Display.Instance.ClearCoins();
        Display.Instance.DrawFullBoard(gameBoard);
        RedropCoins();
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

    void EndGame(int winPlayerId) {
        Debug.Log("victory player " + winPlayerId);
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

public enum GameState {
    Playing,
    Waiting
}
