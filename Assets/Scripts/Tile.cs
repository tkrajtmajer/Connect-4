using UnityEngine;
using System;

public class Tile {
    public TileType tileType;
    public int occupiedById;

    public Tile(TileType tileType) {
        this.tileType = tileType;
        this.occupiedById = 0;
    }
}

[Serializable]
public enum TileType {
    Normal,
    FlipBoard,
    RotateBoard,
    BlowUp,
    SwapNeighbor
}