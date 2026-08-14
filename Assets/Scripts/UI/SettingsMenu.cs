using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
    public UnityEngine.UI.Slider volumeMusicSlider;
    public UnityEngine.UI.Slider volumeSFXSlider;

    public void SetVolumeMusic(float volume) {
        // Debug.Log("volume " + volume);
        volumeMusicSlider.value = volume;
        MusicManager.Instance.audioSource.volume = volume;
    }

    public void SetVolumeSFX(float volume) {
        // Debug.Log("volume " + volume);
        volumeSFXSlider.value = volume;
        SFXManager.Instance.audioSource.volume = volume;
    }
}
