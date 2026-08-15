using UnityEngine;
using System.Collections;

public class Display: MonoBehaviour
{
    public static Display Instance { get; private set; }
    public GameObject boardGO;
    public Vector2 drawOffset;
    public Transform parentBoard;
    public GameObject boardTilePrefab;
    GameObject[,] boardTileGOs;
    GameObject[,] coinGOs;

    public Transform parentCoins;
    public GameObject playerCoinPrefab;
    public float transparency = 50;
    float gravity = 9.81f;

    public GameObject previewCoinPrefab;

    [Header("Powerups Setup")]
    public float boardRotationSpeed;
    public float boardFlipSpeed;
    public GameObject blowUpPrefab;
    public float blowupTime;
    public float swapSpeed;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this.gameObject);
        }
        else {
            Instance = this;
        }
    }

    /// <summary>
    /// Instantiates one tile GameObject per board cell.
    /// Uses the specified preset to set color and sprite per tile.
    /// </summary>
    public void DrawFullBoard(Board board) {
        ClearParent(parentBoard);
        CustomPreset preset = PlayerSettings.Instance.gameLookPreset;

        boardTileGOs = new GameObject[board.width, board.height];

        for(int x = 0; x < board.width; x++) {
            for(int y = 0; y < board.height; y++) {
                GameObject boardTileGO = Instantiate(boardTilePrefab, new Vector3(x - (board.width/2f) + drawOffset.x, y - (board.height/2f) + drawOffset.y, 0), Quaternion.identity, parentBoard);
                boardTileGOs[x, y] = boardTileGO;
                
                Color tileColor;
                Color tileBgColor;
                Sprite bgSprite;
                
                switch (board.GetCellType(x, y)) {
                    case TileType.RotateBoard: 
                        tileColor = preset.rotateBoardTileColor;
                        tileBgColor = preset.rotateBgColor;
                        bgSprite = preset.powerupRotate; 
                        break;
                    case TileType.FlipBoard: 
                        tileColor = preset.flipBoardTileColor;
                        tileBgColor = preset.flipBgColor;
                        bgSprite = preset.powerupFlip; 
                        break;
                    case TileType.BlowUp: 
                        tileColor = preset.blowupTileColor;
                        tileBgColor = preset.blowupBgColor;
                        bgSprite = preset.powerupBlowup; 
                        break;
                    case TileType.SwapNeighbor: 
                        tileColor = preset.swapNeighborTileColor;
                        tileBgColor = preset.swapBgColor;
                        bgSprite = preset.powerupSwap; 
                        break;

                    default:
                        tileColor = preset.normalTileColor;
                        tileBgColor = preset.normalBgColor;
                        bgSprite = null;
                        break;
                };
                
                boardTileGO.GetComponent<TilePrefab>().SetupTile(tileColor, tileBgColor, bgSprite);
            }
        }

        BoxCollider2D boardCollider = boardGO.GetComponent<BoxCollider2D>();
        boardCollider.size = new Vector2(board.width, board.height);
    }

    /// <summary>
    /// Clears coins from the board.
    /// </summary>
    public void ClearCoins(Board board) {
        ClearParent(parentCoins);
        coinGOs = new GameObject[board.width, board.height];
    }

    void ClearParent(Transform parent) {
        for (int i = parent.childCount - 1; i >= 0; i--) Destroy(parent.GetChild(i).gameObject);
    }

    /// <summary>
    /// Redraws a power up tile as a normal one.
    /// </summary>
    public void RedrawNormalTile(int x, int y) {
        boardTileGOs[x, y].GetComponent<TilePrefab>().SetupTile(PlayerSettings.Instance.gameLookPreset.normalTileColor, 
                                                                PlayerSettings.Instance.gameLookPreset.normalBgColor, null);
    }

    /// <summary>
    /// Instantiates a new coin prefab and starts a coroutine to animate it dropping.
    /// </summary>
    /// <param name="initYPos">The starting y position (either at the top of the board or the previous y when redropping)</param>
    /// <param name="finalYPos">The final position (calculated from where the board has the lowest point to drop)</param>
    public IEnumerator DrawCoin(Board board, int xPos, int initYPos, int finalYPos, int playerId) {
        float xPosF = xPos - board.width/2f + drawOffset.x;
        float initYPosF = initYPos - board.height/2f + drawOffset.y;
        float finalYPosF = finalYPos - board.height/2f + drawOffset.y;

        CustomPreset preset = PlayerSettings.Instance.gameLookPreset;

        GameObject coinGO = Instantiate(playerCoinPrefab, new Vector3(xPosF, initYPosF, 0), Quaternion.identity, parentCoins);
        coinGOs[xPos, finalYPos] = coinGO;
        Color playerColor = playerId == 1 ? preset.player1Color : preset.player2Color;
        coinGO.GetComponent<SpriteRenderer>().color = playerColor;

        Sprite playerSprite = playerId == 1 ? preset.player1Sprite : preset.player2Sprite;
        coinGO.GetComponent<SpriteRenderer>().sprite = playerSprite;

        yield return StartCoroutine(DropCoin(coinGO, finalYPosF, board, xPosF, xPos, finalYPos));
    }

    // animates the coin dropping under gravity
    IEnumerator DropCoin(GameObject coinGO, float finalYPosF, Board board, float xPosF, int x, int y) {
        float velocity = 0;
        Vector3 currentPos = coinGO.transform.position;

        while (coinGO.transform.position.y > finalYPosF) {
            velocity += gravity * Time.deltaTime;
            currentPos.y -= velocity * Time.deltaTime;

            if(currentPos.y < finalYPosF) currentPos.y = finalYPosF;

            coinGO.transform.position = currentPos;

            yield return null;
        }
    }
    
    // animates the board rotation over boardRotationSpeed seconds
    public IEnumerator RotateBoard() {
        Quaternion startRot = boardGO.transform.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0, 0, -90f);

        float t = 0;

        while (t < boardRotationSpeed) {
            t += Time.deltaTime;
            boardGO.transform.rotation = Quaternion.Slerp(startRot, endRot, t / boardRotationSpeed);
            yield return null;
        }

        boardGO.transform.rotation = endRot;
    }

    public void ResetRotation() {
        boardGO.transform.rotation = Quaternion.identity;
    }

    // animates the board flip over boardFlipSpeed seconds
    public IEnumerator FlipBoard() {
        Vector3 startScale = boardGO.transform.localScale;
        Vector3 endScale = new Vector3(1, -1 * startScale.y, 1);

        float t = 0;

        while (t < boardFlipSpeed) {
            t += Time.deltaTime;
            boardGO.transform.localScale = Vector3.Lerp(startScale, endScale, t / boardFlipSpeed);
            yield return null;
        }

        boardGO.transform.localScale = endScale;
    }

    public void ResetFlip() {
        boardGO.transform.localScale = new Vector3(1, 1, 1);
    }

    // spawns blowup effects around the center position and waits for x seconds
    public IEnumerator BlowUpTiles(Board board, int centerX, int centerY) {
        // List<GameObject> blowUpPrefabs = new List<GameObject>();

        for(int i = -1; i < 2; i++) {
            for(int j = -1; j < 2; j++) {
                if(centerX + i < 0 || centerX + i >= board.width) continue;
                if(centerY + j < 0 || centerY + j >= board.height) continue;

                if(centerX + i == centerX && centerY + j == centerY) continue;

                float x = centerX + i - (board.width/2f) + drawOffset.x;
                float y = centerY + j - (board.height/2f) + drawOffset.y;
                Instantiate(blowUpPrefab, new Vector3(x, y, 0), Quaternion.identity, parentCoins);
                // blowUpPrefabs.Add(blowupGO);
            }
        }

        yield return new WaitForSeconds(blowupTime);
    }

    // animates the swap transition for swapSpeed seconds; fades between two colors and sprites
    public IEnumerator SwapColor(Board board, int targetX, int targetY, int playerId) {

        float x = targetX - (board.width/2f) + drawOffset.x;
        float y = targetY - (board.height/2f) + drawOffset.y;

        CustomPreset preset = PlayerSettings.Instance.gameLookPreset;
        Color startColor = playerId == 2 ? preset.player1Color : preset.player2Color;
        Color endColor = playerId == 1 ? preset.player1Color : preset.player2Color;
        Sprite startSprite = playerId == 2 ? preset.player1Sprite : preset.player2Sprite;
        Sprite endSprite = playerId == 1 ? preset.player1Sprite : preset.player2Sprite;

        // start obj
        GameObject fromGO = Instantiate(playerCoinPrefab, new Vector3(x, y, 0), Quaternion.identity, parentCoins);
        SpriteRenderer fromSr = fromGO.GetComponent<SpriteRenderer>();
        fromSr.sprite = startSprite;
        fromSr.color = startColor;
        fromSr.sortingOrder = 1;

        // end obj
        GameObject toGO = Instantiate(playerCoinPrefab, new Vector3(x, y, 0), Quaternion.identity, parentCoins);
        SpriteRenderer toSr = toGO.GetComponent<SpriteRenderer>();
        toSr.sprite = endSprite;
        Color endColorTransparent = endColor;
        endColorTransparent.a = 0f;
        toSr.color = endColorTransparent;
        toSr.sortingOrder = 1;

        float t = 0;

        // fade from start to end game object
        while (t < swapSpeed) {
            t += Time.deltaTime;
            
            float lerp = t / swapSpeed;

            Color fromColor = startColor;
            fromColor.a = 1f - lerp;
            fromSr.color = fromColor;

            Color toColor = endColor;
            toColor.a = lerp;
            toSr.color = toColor;

            yield return null;
        }

        Destroy(fromGO);
        Destroy(toGO);
    }

    // used as a visual guide for the player when they are selecting which tiles to blow up or flip
    public void HighlightCoinsAroundCenter(Board board, Vector2 mousePos, int playingId, int playerIdToHighlight) {
        int centerX = Mathf.FloorToInt(mousePos.x + GameManager.Instance.gameBoard.width / 2f);
        int centerY = Mathf.FloorToInt(mousePos.y + GameManager.Instance.gameBoard.height / 2f);

        if(board.GetCellOccupancy(centerX, centerY) != playingId) return;

        for(int i = -1; i < 2; i ++) {
            for(int j = -1; j < 2; j ++) {
                int x = centerX + i;
                int y = centerY + j;

                if(x < 0 || x >= board.width) continue;
                if(y < 0 || y >= board.height) continue;

                if(x == centerX && y == centerY) continue;
                if (coinGOs[x, y] == null) continue;

                if(board.GetCellOccupancy(x, y) == playerIdToHighlight) {
                    SpriteRenderer sr = coinGOs[x, y].GetComponent<SpriteRenderer>();

                    Color color = sr.color;
                    color.a = transparency / 255f;
                    sr.color = color;
                }
            }
        }
    }

    public void DeselectCoinsAroundCenter(Board board, Vector2 mousePos) {
        int centerX = Mathf.FloorToInt(mousePos.x + GameManager.Instance.gameBoard.width / 2f);
        int centerY = Mathf.FloorToInt(mousePos.y + GameManager.Instance.gameBoard.height / 2f);

        for(int i = -1; i < 2; i ++) {
            for(int j = -1; j < 2; j ++) {
                int x = centerX + i;
                int y = centerY + j;

                if(x < 0 || x >= board.width) continue;
                if(y < 0 || y >= board.height) continue;

                if(x == centerX && y == centerY) continue;
                if (coinGOs[x, y] == null) continue;

                SpriteRenderer sr = coinGOs[x, y].GetComponent<SpriteRenderer>();

                Color color = sr.color;
                color.a = 1f;
                sr.color = color;
            }
        }
    }

    public void DeactiveCoinPreview() {
        previewCoinPrefab.SetActive(false);
    }

    // used as a visual guide to show where the current coin would drop on mouse over
    public void ActivateCoinPreview(Vector2 mousePos) {
        float boardX = Mathf.FloorToInt(mousePos.x + GameManager.Instance.gameBoard.width / 2f);

        float xPos = boardX - (GameManager.Instance.gameBoard.width / 2f) + drawOffset.x;
        float yPos = (GameManager.Instance.gameBoard.height / 2f) + drawOffset.y;

        int playerId = GameManager.Instance.GetCurrentPlayer();

        CustomPreset preset = PlayerSettings.Instance.gameLookPreset;
        Color playerColor = playerId == 1 ? preset.player1Color : preset.player2Color;
        playerColor.a = transparency / 255;
        Sprite playerSprite = playerId == 1 ? preset.player1Sprite : preset.player2Sprite;

        previewCoinPrefab.SetActive(true);
        previewCoinPrefab.GetComponent<SpriteRenderer>().color = playerColor;
        previewCoinPrefab.GetComponent<SpriteRenderer>().sprite = playerSprite;
        previewCoinPrefab.transform.position = new Vector3(xPos, yPos, 0);
    }
}