using UnityEngine;
using System.Collections;

public class Display: MonoBehaviour
{
    public static Display Instance { get; private set; }
    public Transform parentBoard;
    public GameObject boardTilePrefab;

    public Transform parentCoins;
    public GameObject playerCoinPrefab;
    float gravity = 9.81f;

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

        for(int x = 0; x < board.width; x++) {
            for(int y = 0; y < board.height; y++) {
                GameObject boardTileGO = Instantiate(boardTilePrefab, new Vector3Int(x, y, 0), Quaternion.identity, parentBoard);
                boardTileGO.GetComponent<SpriteRenderer>().color = board.boardColor;

                int cellValue = board.GetCell(x, y);
                if (cellValue == 0) continue;

                GameObject coinGO = Instantiate(playerCoinPrefab, new Vector3Int(x, y, 0), Quaternion.identity, parentCoins);

                if(cellValue == 1) {
                    coinGO.GetComponent<SpriteRenderer>().color = player1.playerColor;
                }
                else if(cellValue == 2) {
                    coinGO.GetComponent<SpriteRenderer>().color = player2.playerColor;
                }
            }
        }

        // Debug.Log("board drawn");
    }

    void ClearParent(Transform parent) {
        for (int i = parent.childCount - 1; i >= 0; i--) Destroy(parent.GetChild(i).gameObject);
    }

    public void DrawTile(Board board, int xPos, int initYPos, int finalYPos, Player player) {
        GameObject coinGO = Instantiate(playerCoinPrefab, new Vector3Int(xPos, initYPos, 0), Quaternion.identity, parentCoins);
        coinGO.GetComponent<SpriteRenderer>().color = player.playerColor;

        StartCoroutine(DropCoin(coinGO, finalYPos));
    }

    IEnumerator DropCoin(GameObject coinGO, int finalYPos) {
        float velocity = 0;
        Vector3 currentPos = coinGO.transform.position;

        while (coinGO.transform.position.y > finalYPos) {
            velocity += gravity * Time.deltaTime;
            currentPos.y -= velocity * Time.deltaTime;

            if(currentPos.y < finalYPos) currentPos.y = finalYPos;

            coinGO.transform.position = currentPos;

            yield return null;
        }
    }
}