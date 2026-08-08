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
    public Color boardBgColor;
    public Color normalTileColor;
    public Color rotateBoardTileColor;
    public Color flipBoardTileColor;
    public Color blowupTileColor;
    public Color swapNeighborTileColor;
}
