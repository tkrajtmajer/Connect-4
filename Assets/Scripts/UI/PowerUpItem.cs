using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class PowerUpItem: MonoBehaviour {

    public TMP_Text powerUpName; 
    TileType powerUpType;
    int belongsToPlayer;
    int slotIdx;
    public bool isCancellable;

    public event Action<TileType, int, int> TriggerPowerUp;

    public void Setup(int playerId, TileType powerUpType, int slotIdx) {
        this.belongsToPlayer = playerId;
        this.powerUpType = powerUpType;
        this.slotIdx = slotIdx;

        if(powerUpType == TileType.BlowUp || powerUpType == TileType.SwapNeighbor) isCancellable = true;
        else isCancellable = false;

        powerUpName.text = powerUpType.ToString();
    }

    public void Use() {
        TriggerPowerUp?.Invoke(powerUpType, belongsToPlayer, slotIdx);
    }

}