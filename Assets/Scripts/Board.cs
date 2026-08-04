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
    public bool PlaceCoin(int xPos, int playerId, out int yPos) {
        yPos = FindAvailableSlot(xPos);

        if(yPos != -1) {
            cells[xPos, yPos] = playerId;

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

    // check board in 4 directions around currently placed tile
    public bool CheckWin(int xPos, int yPos, int playerId) {
        int[][] directions = new int[][] {new int[] {1, 0}, new int[] {0, 1}, new int[] {1, 1}, new int[] {1, -1}};

        foreach (int[] dir in directions) {
            int currentTally = 1;
            currentTally += CheckDirection(xPos, yPos, dir[0], dir[1], playerId);
            currentTally += CheckDirection(xPos, yPos, -dir[0], -dir[1], playerId);

            if (currentTally >= 4) return true;
        }

        return false;
    }

    int CheckDirection(int xPos, int yPos, int dirX, int dirY, int playerId) {
        int currentTally = 0;
        int curX = xPos + dirX;
        int curY = yPos + dirY;

        while(curX >= 0 && curX < width && curY >= 0 && curY < height && cells[curX, curY] == playerId) {
            currentTally++;
            curX += dirX;
            curY += dirY;
        }

        return currentTally;
    }
}