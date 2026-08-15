using UnityEngine;
using System.Collections.Generic;

public class Player {

    public TileType?[] playerCollectedTiles = new TileType?[3];
    public bool usedPowerUpInTurn {get; set;}
    
    public Player() {
        this.usedPowerUpInTurn = false;
    }

    public bool GivePlayerPowerUp(TileType powerUpType, out int slotIdx) {
        // find earliest empty slot
        for (int i = 0; i < playerCollectedTiles.Length; i++) {
            if (playerCollectedTiles[i] == null) {
                playerCollectedTiles[i] = powerUpType;
                slotIdx = i;
                return true;
            }
        }
        slotIdx = -1;
        return false;
    }

    public void ClearSlot(int slotIdx) {
        playerCollectedTiles[slotIdx] = null;
    }
}