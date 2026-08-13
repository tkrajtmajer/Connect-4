using UnityEngine;

public class Preset: MonoBehaviour {
    public CustomPreset presetData;

    public GameObject selectedBg;
    
    public void UpdateChosenPreset() {
        PlayerSettings.Instance.gameLookPreset = presetData;
        selectedBg.SetActive(true);
    }

    public void Deselect() {
        selectedBg.SetActive(false);
    }
}