using UnityEngine;

public class Display: MonoBehaviour
{
    public static Display Instance { get; private set; }
    public Transform parent;
    public GameObject playerCoinPrefab;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this.gameObject);
        }
        else {
            Instance = this;
        }
    }

    // draw the board according to which player placed their coins where
    // TODO: might wanna optimize redrawing the entire board
    public void DrawBoard(Board board, Player player1, Player player2) {
        ClearParent();

        for(int x = 0; x < board.width; x++) {
            for(int y = 0; y < board.height; y++) {
                int cellValue = board.GetCell(x, y);
                if (cellValue == 0) continue;

                GameObject coinGO = Instantiate(playerCoinPrefab, new Vector3Int(x, y, 0), Quaternion.identity, parent);

                if(cellValue == 1) {
                    coinGO.GetComponent<SpriteRenderer>().color = player1.playerColor;
                }
                else if(cellValue == 2) {
                    coinGO.GetComponent<SpriteRenderer>().color = player2.playerColor;
                }
            }
        }

        Debug.Log("board drawn");
    }

    void ClearParent() {
        for (int i = parent.childCount - 1; i >= 0; i--) Destroy(parent.GetChild(i).gameObject);
    }
}