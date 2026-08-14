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

    public Transform parentCoins;
    public GameObject playerCoinPrefab;
    float gravity = 9.81f;

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

    // draw the board according to which player placed their coins where
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

    public void ClearCoins() {
        ClearParent(parentCoins);
    }

    void ClearParent(Transform parent) {
        for (int i = parent.childCount - 1; i >= 0; i--) Destroy(parent.GetChild(i).gameObject);
    }

    public void RedrawNormalTile(int x, int y) {
        boardTileGOs[x, y].GetComponent<TilePrefab>().SetupTile(PlayerSettings.Instance.gameLookPreset.normalTileColor, 
                                                                PlayerSettings.Instance.gameLookPreset.normalBgColor, null);
    }

    public IEnumerator DrawCoin(Board board, int xPos, int initYPos, int finalYPos, int playerId) {
        float xPosF = xPos - board.width/2f + drawOffset.x;
        float initYPosF = initYPos - board.height/2f + drawOffset.y;
        float finalYPosF = finalYPos - board.height/2f + drawOffset.y;

        CustomPreset preset = PlayerSettings.Instance.gameLookPreset;

        GameObject coinGO = Instantiate(playerCoinPrefab, new Vector3(xPosF, initYPosF, 0), Quaternion.identity, parentCoins);
        Color playerColor = playerId == 1 ? preset.player1Color : preset.player2Color;
        coinGO.GetComponent<SpriteRenderer>().color = playerColor;

        Sprite playerSprite = playerId == 1 ? preset.player1Sprite : preset.player2Sprite;
        coinGO.GetComponent<SpriteRenderer>().sprite = playerSprite;

        yield return StartCoroutine(DropCoin(coinGO, finalYPosF, board, xPosF, xPos, finalYPos));
    }

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

    public IEnumerator FlipBoard() {
        Vector3 startScale = boardGO.transform.localScale;
        Vector3 endScale = new Vector3(1, -1 * startScale.y, 1);

        float t = 0;

        while (t < boardRotationSpeed) {
            t += Time.deltaTime;
            boardGO.transform.localScale = Vector3.Lerp(startScale, endScale, t / boardFlipSpeed);
            yield return null;
        }

        boardGO.transform.localScale = endScale;
    }

    public void ResetFlip() {
        boardGO.transform.localScale = new Vector3(1, 1, 1);
    }

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
}