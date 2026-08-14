using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Setup")]
    public int boardWidth;
    public int boardHeight;
    public TileProbList tileProbList = new TileProbList();
    public Dictionary<TileType, float> tileProbDict;
    public bool isOnlineGame;

    Board gameBoard;
    Player player1;
    Player player2;
    int currentPlayer;
    internal GameState gameState;
    Dictionary<ulong, int> clientIdToPlayerId = new Dictionary<ulong, int>(); // save ref between client id and player nr
    System.Random rng = new System.Random();

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
        if(!isOnlineGame) {
            InitializeGame();
            RedrawBoard();
            HUD.Instance.ResetHUD();
            HUD.Instance.UpdateCurrPlayer("Start player ", currentPlayer);
        }
    }

    void InitializeGame() {
        tileProbDict = tileProbList.ToDictionary();

        gameBoard = new Board(boardWidth, boardHeight, tileProbDict);
        player1 = new Player();
        player2 = new Player();
        currentPlayer = rng.Next(1, 3);
        gameState = GameState.Playing;
        Time.timeScale = 1f;
    }

    void OnEnable() {
        BoardInputHandler.Instance.MouseClicked += HandlePlayerInput;
        HUD.Instance.CancelledPowerup += HandleCancelPowerup;
    }

    void OnDisable() {
        BoardInputHandler.Instance.MouseClicked -= HandlePlayerInput;
        HUD.Instance.CancelledPowerup -= HandleCancelPowerup;
    }

    void HandlePlayerInput(Vector2 mousePos) {

        if(gameState == GameState.Animation || gameState == GameState.GameOver) return;

        mousePos += new Vector2(gameBoard.width / 2f, gameBoard.height / 2f);

        int xPos = Mathf.FloorToInt(mousePos.x);
        int yPos = Mathf.FloorToInt(mousePos.y);
        
        if(gameState == GameState.Playing) {
            if (isOnlineGame) {
                RequestDropCoinRpc(xPos);
            }
            else {
                if (gameBoard.PlaceCoin(xPos, currentPlayer, out int yOut)) {
                    StartCoroutine(ApplyDroppedCoin(xPos, yOut, currentPlayer));
                }
            }
        }
        else if(gameState == GameState.Waiting) {
            // Debug.Log("waiting");
            if(gameBoard.GetCellOccupancy(xPos, yPos) == currentPlayer) {
                // can use the powerup, else needs valid position
                if (isOnlineGame) {
                    RequestPowerUpRpc(pendingPowerUpType, pendingPowerUpSlot, xPos, yPos);
                }
                else {
                    if(pendingPowerUpType == TileType.BlowUp) {
                        StartCoroutine(HandleBlowup(pendingPowerUpType, pendingPowerUpSlot, pendingPowerUpPlayerId, xPos, yPos));
                    }
                    else if(pendingPowerUpType == TileType.SwapNeighbor) {
                        HandleSwap(pendingPowerUpType, pendingPowerUpSlot, pendingPowerUpPlayerId, xPos, yPos);
                    }
                }
                gameState = GameState.Playing;
                HUD.Instance.HideCancelPowerUp();
            }
        }
    }

    IEnumerator ApplyDroppedCoin(int xPos, int yPos, int playerId) {
        gameState = GameState.Animation;
        yield return StartCoroutine(DropCoin(xPos, yPos, boardHeight, playerId));
        gameState = GameState.Playing;

        CheckPowerUps(xPos, yPos, playerId, out bool redrawTile);
        if(redrawTile) Display.Instance.RedrawNormalTile(xPos, yPos);
        CheckWinCondition(xPos, yPos);
        UpdateCurrentPlayer();

        // Debug.Log("current player " + currentPlayer);
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

    IEnumerator DropCoin(int xPos, int yPos, int yInit, int playerId) {
        SFXManager.Instance.PlayClip(SFXManager.Instance.coinDropClip);
        yield return StartCoroutine(Display.Instance.DrawCoin(gameBoard, xPos, yInit, yPos, playerId));
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
        if (GetPlayer(currentPlayer).usedPowerUpInTurn == true) return; // only allow one powerup per turn

        switch (type) {
            case TileType.RotateBoard:
                if (isOnlineGame) RequestPowerUpRpc(type, slotIdx, -1, -1);
                else StartCoroutine(HandleRotation(type, slotIdx, playerId));
                break;
            case TileType.FlipBoard:
                if (isOnlineGame) RequestPowerUpRpc(type, slotIdx, -1, -1);
                else StartCoroutine(HandleFlip(type, slotIdx, playerId));
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
        gameBoard.RotateBoard();
        GetPlayer(playerId).usedPowerUpInTurn = true;
        UpdateHUD(type, slotIdx, playerId);

        SFXManager.Instance.PlayClip(SFXManager.Instance.rotateClip); 
        gameState = GameState.Animation;
        yield return StartCoroutine(Display.Instance.RotateBoard());

        Display.Instance.ResetRotation();
        RedrawBoard();
        RedropCoins();
        gameState = GameState.Playing;
    }

    IEnumerator HandleFlip(TileType type, int slotIdx, int playerId) {
        gameBoard.FlipBoard();
        GetPlayer(playerId).usedPowerUpInTurn = true;
        UpdateHUD(type, slotIdx, playerId);

        SFXManager.Instance.PlayClip(SFXManager.Instance.flipClip); 
        gameState = GameState.Animation;
        yield return StartCoroutine(Display.Instance.FlipBoard());

        Display.Instance.ResetFlip();
        RedrawBoard();
        RedropCoins();
        gameState = GameState.Playing;
    }

    // TODO: add way to undo waiting condition
    IEnumerator HandleBlowup(TileType type, int slotIdx, int playerId, int centerX, int centerY) {
        // add way to blow up coins either w animation or code(?)

        gameBoard.BlowUpCells(centerX, centerY);
        GetPlayer(playerId).usedPowerUpInTurn = true;
        UpdateHUD(type, slotIdx, playerId);

        SFXManager.Instance.PlayClip(SFXManager.Instance.blowupClip); 
        gameState = GameState.Animation;
        yield return StartCoroutine(Display.Instance.BlowUpTiles(gameBoard, centerX, centerY));

        RedrawBoard();
        RedropCoins();
        gameState = GameState.Playing;
    }

    void HandleSwap(TileType type, int slotIdx, int playerId, int centerX, int centerY) {

        gameBoard.PickRandomNeighbor(centerX, centerY, playerId, out int targetX, out int targetY);
        StartCoroutine(DoSwap(type, slotIdx, playerId, targetX, targetY));
    }

    IEnumerator DoSwap(TileType type, int slotIdx, int playerId, int targetX, int targetY) {
        gameBoard.RandomSwapNeighbor(targetX, targetY, playerId);
        GetPlayer(playerId).usedPowerUpInTurn = true;
        UpdateHUD(type, slotIdx, playerId);
        
        SFXManager.Instance.PlayClip(SFXManager.Instance.swapClip);
        gameState = GameState.Animation;
        yield return StartCoroutine(Display.Instance.SwapColor(gameBoard, targetX, targetY, playerId));
        
        RedrawBoard();
        RedropCoins();
        gameState = GameState.Playing;
    }

    void UpdateHUD(TileType type, int slotIdx, int playerId) {
        Player player = GetPlayer(playerId);

        player.ClearSlot(slotIdx);
        HUD.Instance.RemovePowerUp(playerId, slotIdx);
    }

    void RedrawBoard() {
        Display.Instance.ClearCoins();
        Display.Instance.DrawFullBoard(gameBoard);
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

                    StartCoroutine(DropCoin(x, yOut, y, playerId));
                    CheckPowerUps(x, yOut, playerId, out bool redrawTile);
                    if(redrawTile) Display.Instance.RedrawNormalTile(x, yOut);
                }
            }
        }

        CheckWinConditionBoard();
    }

    void UpdateCurrentPlayer() {
        currentPlayer = (currentPlayer == 1) ? 2 : 1;
        GetPlayer(currentPlayer).usedPowerUpInTurn = false;
        HUD.Instance.UpdateCurrPlayer("Turn player ", currentPlayer);
    }

    Player GetPlayer(int playerId) {
        return playerId == 1 ? player1 : player2;
    }

    void HandleCancelPowerup() {
        gameState = GameState.Playing;
    }

    void EndGame(int winPlayerId) {
        Debug.Log("victory player " + winPlayerId);

        Time.timeScale = 0f; // pause game until it is reset
        gameState = GameState.GameOver;
        HUD.Instance.ShowWinScreen(winPlayerId);
    }

    public void RestartGame() {
        if(isOnlineGame) {
            RequestRestartGameRpc();
        }
        else {
            InitializeGame();
            RedrawBoard();
            HUD.Instance.ResetHUD();
            HUD.Instance.UpdateCurrPlayer("Start player ", currentPlayer);
        }
    }

    // ~~~~~~~~ NETWORK FUNCTIONS ~~~~~~~~~

    public override void OnNetworkSpawn() {
        if (IsServer) {
            InitializeGame();

            foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds) {
                OnClientConnected(clientId);
            }
        }
    }

    void OnClientConnected(ulong clientId) {
        int playerId = clientIdToPlayerId.Count == 0 ? 1 : 2;
        clientIdToPlayerId[clientId] = playerId;

        TileType[] types = gameBoard.GetTileTypes();
        SendInitialGameStateRpc(types, currentPlayer, gameState, RpcTarget.Single(clientId, RpcTargetUse.Temp));
    }

    int GetPlayerIdForClient(ulong clientId) {
        if(!clientIdToPlayerId.ContainsKey(clientId)) return -1;
        
        return clientIdToPlayerId[clientId];
    }

    [Rpc(SendTo.SpecifiedInParams)]
    void SendInitialGameStateRpc(TileType[] types, int initCurrentPlayer, GameState initGameState, RpcParams rpcParams = default) {
        gameBoard = new Board(boardWidth, boardHeight, types);
        player1 = new Player();
        player2 = new Player();
        currentPlayer = initCurrentPlayer;
        gameState = initGameState;
        Time.timeScale = 1f;

        RedrawBoard();
        HUD.Instance.ResetHUD();
        HUD.Instance.UpdateCurrPlayer("Start player ", currentPlayer);
    }

    // server checks if a coin can be placed, if not return, if yes send move to clients to update board state locally
    [Rpc(SendTo.Server)]
    void RequestDropCoinRpc(int xPos, RpcParams rpcParams = default) {
        // get playerid from server
        int playerId = GetPlayerIdForClient(rpcParams.Receive.SenderClientId);

        if (playerId != currentPlayer) return;
        if (!gameBoard.PlaceCoin(xPos, playerId, out int yPos)) return;

        ApplyDropRpc(xPos, yPos, playerId);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void ApplyDropRpc(int xPos, int yPos, int playerId) {
        if(!IsServer) gameBoard.PlaceCoin(xPos, playerId, out int _); // clients mirror server if coin was placed

        StartCoroutine(ApplyDroppedCoin(xPos, yPos, playerId));
    }

    [Rpc(SendTo.Server)]
    void RequestPowerUpRpc(TileType type, int slotIdx, int centerX, int centerY, RpcParams rpcParams = default) {
        int playerId = GetPlayerIdForClient(rpcParams.Receive.SenderClientId);

        if (playerId != currentPlayer) return;
        if (GetPlayer(currentPlayer).usedPowerUpInTurn) return;

        if (type == TileType.SwapNeighbor) {
            if (!gameBoard.PickRandomNeighbor(centerX, centerY, playerId, out int targetX, out int targetY)) return;
            ApplySwapRpc(type, slotIdx, playerId, targetX, targetY);
        }
        else ApplyPowerUpRpc(type, slotIdx, playerId, centerX, centerY);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void ApplyPowerUpRpc(TileType type, int slotIdx, int playerId, int centerX, int centerY) {
        switch (type) {
            case TileType.RotateBoard:
                StartCoroutine(HandleRotation(type, slotIdx, playerId));
                break;
            case TileType.FlipBoard:
                StartCoroutine(HandleFlip(type, slotIdx, playerId));
                break;
            case TileType.BlowUp:
                StartCoroutine(HandleBlowup(type, slotIdx, playerId, centerX, centerY));
                break;
            default:
                break;
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void ApplySwapRpc(TileType type, int slotIdx, int playerId, int targetX, int targetY) {
        StartCoroutine(DoSwap(type, slotIdx, playerId, targetX, targetY));
    }

    [Rpc(SendTo.Server)]
    void RequestRestartGameRpc() {
        // reinitialize on server and send to both players
        InitializeGame();
        TileType[] types = gameBoard.GetTileTypes();

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds) {
            SendInitialGameStateRpc(types, currentPlayer, gameState, RpcTarget.Single(clientId, RpcTargetUse.Temp));
        }
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
    Waiting,
    Animation,
    GameOver
}
