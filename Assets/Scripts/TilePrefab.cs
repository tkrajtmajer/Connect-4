using UnityEngine;

public class TilePrefab : MonoBehaviour
{
    
    public SpriteRenderer tileSR;
    public SpriteRenderer tileBgSR;
    public SpriteRenderer powerupSR;

    public void SetupTile(Color color, Color colorBg, Sprite bgSprite) {
        tileSR.color = color;
        tileBgSR.color = colorBg;
        powerupSR.sprite = bgSprite;
    }
}
