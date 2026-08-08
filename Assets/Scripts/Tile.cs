using UnityEngine;
using System;

public class Tile {
    public TileType tileType;
    public int occupiedById;
    public Sprite tileBg; // TODO: add tile bgs

    public Tile(TileType tileType) {
        this.tileType = tileType;
        this.occupiedById = 0;
        // this.tileBg = tileBg;
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