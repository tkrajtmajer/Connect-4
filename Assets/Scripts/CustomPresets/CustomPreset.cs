using UnityEngine;

[CreateAssetMenu(fileName = "CustomPreset", menuName = "Scriptable Objects/CustomPreset")]
public class CustomPreset : ScriptableObject
{
    [Header("Players")]
    public Sprite player1Sprite;
    public Color player1Color;
    public Sprite player2Sprite;
    public Color player2Color;

    [Header("Board")]
    // public Color boardBgColor;
    public Color normalTileColor;
    public Color normalBgColor;
    public Color rotateBoardTileColor;
    public Color rotateBgColor;
    public Color flipBoardTileColor;
    public Color flipBgColor;
    public Color blowupTileColor;
    public Color blowupBgColor;
    public Color swapNeighborTileColor;
    public Color swapBgColor;

    [Header("PowerUps")]
    public Sprite powerupRotate;
    public Sprite powerupFlip;
    public Sprite powerupBlowup;
    public Sprite powerupSwap;
}
