using UnityEngine;
using System.Collections.Generic;

public class Board {
    public int width {get;}
    public int height {get;}
    Tile[,] cells;

    public Color boardColor {get;}

    public Board(int boardWidth, int boardHeight, Color boardColor, Dictionary<TileType, float> tileProbabilities) {
        this.width = boardWidth;
        this.height = boardHeight;
        this.boardColor = boardColor;

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

        System.Random rng = new System.Random();

        double roll = rng.NextDouble() * totalP;

        foreach(KeyValuePair<TileType, float> t in tileProbabilities) {
            roll -= t.Value;
            if(roll <= 0) {
                return t.Key;
            }
        }

        return TileType.Normal; // fallback
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
}