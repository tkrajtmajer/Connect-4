using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class BoardInputHandler: MonoBehaviour {

    public static BoardInputHandler Instance { get; private set; }
    public event Action<Vector2> MouseClicked;

    void Update() {
        UpdateDropPreview();

        if (Mouse.current.leftButton.wasPressedThisFrame) {
            Vector2 screenPos = Mouse.current.position.ReadValue();
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(screenPos);

            Collider2D hit = Physics2D.OverlapPoint(mousePos);
            if (hit != null && hit.gameObject == gameObject) {
                // Debug.Log("Clicked board at " + mousePos.x + ", " + mousePos.y);
                MouseClicked?.Invoke(mousePos);
            }
        }
    }

    void UpdateDropPreview() {

        GameManager gm = GameManager.Instance;

        if (gm.gameState != GameState.Playing) {
            Display.Instance.DeactiveCoinPreview();
            return;
        }

        if (gm.isOnlineGame && gm.GetLocalPlayer() != gm.GetCurrentPlayer()) {
            Display.Instance.DeactiveCoinPreview();
            return;
        }

        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(screenPos);
        
        Collider2D hit = Physics2D.OverlapPoint(mousePos);
        if (hit == null || hit.gameObject != gameObject) {
            Display.Instance.DeactiveCoinPreview();
            return;
        }

        Display.Instance.ActivateCoinPreview(mousePos);
    }

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this.gameObject);
        }
        else {
            Instance = this;
        }
    }
}