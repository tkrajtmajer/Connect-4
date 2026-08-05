using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class PowerUpItem: MonoBehaviour {

    public TMP_Text powerUpName; 
    TileType powerUpType;
    int belongsToPlayer;
    int slotIdx;

    public event Action<TileType, int, int> TriggerPowerUp;

    public void Setup(int playerId, TileType powerUpType, int slotIdx) {
        this.belongsToPlayer = playerId;
        this.powerUpType = powerUpType;
        this.slotIdx = slotIdx;

        powerUpName.text = powerUpType.ToString();
    }

    public void Use() {
        TriggerPowerUp?.Invoke(powerUpType, belongsToPlayer, slotIdx);
    }

}