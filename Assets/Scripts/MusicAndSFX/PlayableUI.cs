using UnityEngine;
using UnityEngine.UI;

public class PlayableUI : MonoBehaviour {

    void Start () {
		Button btn = this.GetComponent<Button>();
		btn.onClick.AddListener(PlayAudioClip);
	}

    public void PlayAudioClip() {
        SFXManager.Instance.PlayClip(SFXManager.Instance.buttonClickClip);
    }
}