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

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this.gameObject);
        }
        else {
            Instance = this;
        }
    }

    // draw the board according to which player placed their coins where
    public void DrawFullBoard(Board board, Player player1, Player player2) {
        ClearParent(parentBoard);
        ClearParent(parentCoins);

        boardTileGOs = new GameObject[board.width, board.height];

        for(int x = 0; x < board.width; x++) {
            for(int y = 0; y < board.height; y++) {
                GameObject boardTileGO = Instantiate(boardTilePrefab, new Vector3(x - (board.width/2f) + drawOffset.x, y - (board.height/2f) + drawOffset.y, 0), Quaternion.identity, parentBoard);
                boardTileGOs[x, y] = boardTileGO;
                
                Color tileColor = board.GetCellType(x, y) switch {
                    TileType.RotateBoard => GameManager.Instance.colorRotate,
                    TileType.FlipBoard => GameManager.Instance.colorFlip,
                    TileType.BlowUp => GameManager.Instance.colorBlowup,
                    TileType.SwapNeighbor => GameManager.Instance.colorSwap,
                    _ => board.boardColor
                };
                
                boardTileGO.GetComponent<SpriteRenderer>().color = tileColor;
                boardTileGO.GetComponentsInChildren<SpriteRenderer>()[1].color = board.boardBgColor;

                // int cellValue = board.GetCellOccupancy(x, y);
                // if (cellValue == 0) continue;

                // GameObject coinGO = Instantiate(playerCoinPrefab, new Vector3Int(x, y, 0), Quaternion.identity, parentCoins);

                // if(cellValue == 1) {
                //     coinGO.GetComponent<SpriteRenderer>().color = player1.playerColor;
                // }
                // else if(cellValue == 2) {
                //     coinGO.GetComponent<SpriteRenderer>().color = player2.playerColor;
                // }
            }
        }

        BoxCollider2D boardCollider = boardGO.GetComponent<BoxCollider2D>();
        boardCollider.size = new Vector2(board.width, board.height);
        // boardCollider.offset = new Vector2(board.width / 2f, board.height / 2f);
    }

    void ClearParent(Transform parent) {
        for (int i = parent.childCount - 1; i >= 0; i--) Destroy(parent.GetChild(i).gameObject);
    }

    public void RedrawNormalTile(Board board, int x, int y) {
        boardTileGOs[x, y].GetComponent<SpriteRenderer>().color = board.boardColor;
    }

    public void DrawCoin(Board board, int xPos, int initYPos, int finalYPos, Player player, bool redrawTile) {
        float xPosF = xPos - board.width/2f + drawOffset.x;
        float initYPosF = initYPos - board.height/2f + drawOffset.y;
        float finalYPosF = finalYPos - board.height/2f + drawOffset.y;

        GameObject coinGO = Instantiate(playerCoinPrefab, new Vector3(xPosF, initYPosF, 0), Quaternion.identity, parentCoins);
        coinGO.GetComponent<SpriteRenderer>().color = player.playerColor;

        StartCoroutine(DropCoin(coinGO, finalYPosF, board, xPosF, redrawTile, xPos, finalYPos));
    }

    IEnumerator DropCoin(GameObject coinGO, float finalYPosF, Board board, float xPosF, bool redrawTile, int x, int y) {
        float velocity = 0;
        Vector3 currentPos = coinGO.transform.position;

        while (coinGO.transform.position.y > finalYPosF) {
            velocity += gravity * Time.deltaTime;
            currentPos.y -= velocity * Time.deltaTime;

            if(currentPos.y < finalYPosF) currentPos.y = finalYPosF;

            coinGO.transform.position = currentPos;

            yield return null;
        }

        if(redrawTile) RedrawNormalTile(board, x, y);
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
}