using UnityEngine;


public class Board {
    public int width {get;}
    public int height {get;}
    int[,] cells;

    public Board(int boardWidth, int boardHeight) {
        this.width = boardWidth;
        this.height = boardHeight;

        this.cells = new int[boardWidth, boardHeight];
    }

    public int GetCell(int x, int y) {
        return cells[x, y];
    }

    // returns whether coin was placed based on whether grid is full
    public bool PlaceCoin(int xPos, int playerId) {
        int y = FindAvailableSlot(xPos);

        if(y != -1) {
            cells[xPos, y] = playerId;

            return true;
        }

        return false;
    }

    int FindAvailableSlot(int xPos) {
        for(int y = 0; y < height; y++) {
            if(cells[xPos, y] == 0) return y;
        }

        return -1;
    }
}