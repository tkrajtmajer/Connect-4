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
            DontDestroyOnLoad(this.gameObject);
        } 
    }
}
