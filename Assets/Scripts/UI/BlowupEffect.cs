using UnityEngine;

public class BlowupEffect : MonoBehaviour {
    public AnimationClip blowupAnimation;

    void Start() {
        Destroy(gameObject, blowupAnimation.length);
    }
}