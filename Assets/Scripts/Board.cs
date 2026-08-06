using UnityEngine;
using System.Collections.Generic;

public class Board {
    public int width {get; private set;}
    public int height {get; private set;}
    Tile[,] cells;
    System.Random rng = new System.Random();

    public Color boardColor {get;}
    public Color boardBgColor {get;}

    public Board(int boardWidth, int boardHeight, Color boardColor, Color boardBgColor, Dictionary<TileType, float> tileProbabilities) {
        this.width = boardWidth;
        this.height = boardHeight;
        this.boardColor = boardColor;
        this.boardBgColor = boardBgColor;

        this.cells = new Tile[boardWidth, boardHeight];

        PopulateCellsBasedOnProbabilities(tileProbabilities);
    }

    public Tile GetCell(int x, int y) {
        return cells[x, y];
    }

    public TileType GetCellType(int x, int y) {
        return cells[x, y].tileType;
    }

    public int GetCellOccupancy(int x, int y) {
        return cells[x, y].occupiedById;
    }

    public void SetCellOccupancy(int x, int y, int id) {
        cells[x, y].occupiedById = id;
    }

    // give each cell a type based on some probability of it occuring
    void PopulateCellsBasedOnProbabilities(Dictionary<TileType, float> tileProbabilities) {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                TileType chosenType = Roll(tileProbabilities);
                cells[x, y] = new Tile(chosenType);
            }
        }
    }

    TileType Roll(Dictionary<TileType, float> tileProbabilities) {
        float totalP = 0;
        foreach(float p in tileProbabilities.Values) totalP += p;

        double roll = rng.NextDouble() * totalP;

        foreach(KeyValuePair<TileType, float> t in tileProbabilities) {
            roll -= t.Value;
            if(roll <= 0) {
                return t.Key;
            }
        }

        return TileType.Normal; // fallback
    }

    public void UseTile(int xPos, int yPos) {
        cells[xPos, yPos].tileType = TileType.Normal;
    }

    // returns whether coin was placed based on whether grid is full
    public bool PlaceCoin(int xPos, int playerId, out int yPos) {
        yPos = FindAvailableSlot(xPos);

        if(yPos != -1) {
            SetCellOccupancy(xPos, yPos, playerId);

            return true;
        }

        return false;
    }

    int FindAvailableSlot(int xPos) {
        for(int y = 0; y < height; y++) {
            if(GetCellOccupancy(xPos, y) == 0) return y;
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

        while(curX >= 0 && curX < width && curY >= 0 && curY < height && GetCellOccupancy(curX, curY) == playerId) {
            currentTally++;
            curX += dirX;
            curY += dirY;
        }

        return currentTally;
    }

    // rotate cw
    public void RotateBoard() {
        int temp = width;
        width = height;
        height = temp;

        Tile[,] newBoard = new Tile[width, height];

        for(int xOld = 0; xOld < height; xOld++) {
            for(int yOld = 0; yOld < width; yOld++) {
                int yNew = height - 1 - xOld;
                int xNew = yOld;
                newBoard[xNew, yNew] = GetCell(xOld, yOld);
            } 
        }

        cells = newBoard;
    }

    public void FlipBoard() {

        Tile[,] newBoard = new Tile[width, height];

        for(int xOld = 0; xOld < width; xOld++) {
            for(int yOld = 0; yOld < height; yOld++) {
                int xNew = xOld;
                int yNew = height - yOld - 1;
                newBoard[xNew, yNew] = GetCell(xOld, yOld);
            }
        }

        cells = newBoard;
    }

    public void BlowUpCells(int centerX, int centerY) {

        for(int i = -1; i < 2; i++) {
            for(int j = -1; j < 2; j++) {
                if(centerX + i < 0 || centerX + i >= width) continue;
                if(centerY + j < 0 || centerY + j >= height) continue;

                if(centerX + i == centerX && centerY + j == centerY) continue;

                SetCellOccupancy(centerX + i, centerY + j, 0);
            }
        }
    }

    // swap one adjacent neighbor to the player's own type
    public void RandomSwapNeighbor(int centerX, int centerY, int playerId) {
        List<Tile> validNeighbors = new List<Tile>();

        for(int i = -1; i < 2; i++) {
            for(int j = -1; j < 2; j++) {
                int x = centerX + i;
                int y = centerY + j;

                if(x < 0 || x >= width) continue;
                if(y < 0 || y >= height) continue;

                if(x == centerX && y == centerY) continue;

                int playerInCell = GetCellOccupancy(x, y);

                if(playerInCell != 0 && playerInCell != playerId) validNeighbors.Add(GetCell(x, y));
            }
        }

        if(validNeighbors.Count == 0) return;

        int chosenIdx = Random.Range(0, validNeighbors.Count);
        validNeighbors[chosenIdx].occupiedById = playerId;
    }
}