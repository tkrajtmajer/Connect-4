using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
    public UnityEngine.UI.Slider volumeSlider;

    public void SetVolume(float volume) {
        Debug.Log("volume " + volume);
        volumeSlider.value = volume;
        // MusicManager.Instance.audioSource.volume = volume;
    }
}
