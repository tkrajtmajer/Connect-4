using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class PowerUpItem: MonoBehaviour {

    public Image powerUpCard;
    public Image powerUpSprite;
    TileType powerUpType;
    int belongsToPlayer;
    int slotIdx;

    public event Action<TileType, int, int> TriggerPowerUp;

    public void Setup(int playerId, TileType powerUpType, int slotIdx) {
        this.belongsToPlayer = playerId;
        this.powerUpType = powerUpType;
        this.slotIdx = slotIdx;

        CustomPreset preset = PlayerSettings.Instance.gameLookPreset;

        switch (powerUpType) {
            case TileType.RotateBoard:
                powerUpCard.color = preset.rotateBoardTileColor;
                powerUpSprite.sprite = preset.powerupRotate; 
                break;
            case TileType.FlipBoard:
                powerUpCard.color = preset.flipBoardTileColor;
                powerUpSprite.sprite = preset.powerupFlip; 
                break;
            case TileType.BlowUp:
                powerUpCard.color = preset.blowupTileColor;
                powerUpSprite.sprite = preset.powerupBlowup; 
                break;
            case TileType.SwapNeighbor:
                powerUpCard.color = preset.swapNeighborTileColor;
                powerUpSprite.sprite = preset.powerupSwap; 
                break;

            default:
                break;
        }
    }

    public void Use() {
        TriggerPowerUp?.Invoke(powerUpType, belongsToPlayer, slotIdx);
    }

}