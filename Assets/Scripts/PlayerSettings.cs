using UnityEngine;

public class PlayerSettings : MonoBehaviour
{
    public static PlayerSettings Instance { get; private set; }

    public CustomPreset gameLookPreset;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this.gameObject);
        }
        else {
            Instance = this;
        } // TODO: also make it persist across scenes, move to first scene
    }
}
