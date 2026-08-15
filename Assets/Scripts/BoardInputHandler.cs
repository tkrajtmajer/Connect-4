using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class BoardInputHandler: MonoBehaviour {

    public static BoardInputHandler Instance { get; private set; }
    public event Action<Vector2> MouseClicked;

    Vector2 previousMousePos;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this.gameObject);
        }
        else {
            Instance = this;
        }
    }

    void Update() {
        UpdateDropPreview();
        UpdateWaitingPreview();

        bool mouseClicked = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool touchClicked = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame;

        if (!mouseClicked && !touchClicked) return;

        // Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector2 screenPos = mouseClicked ? Mouse.current.position.ReadValue() : Touchscreen.current.primaryTouch.position.ReadValue();        
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(screenPos);

        Collider2D hit = Physics2D.OverlapPoint(mousePos);
        if (hit != null && hit.gameObject == gameObject) {
            // Debug.Log("Clicked board at " + mousePos.x + ", " + mousePos.y);
            MouseClicked?.Invoke(mousePos);
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

    void UpdateWaitingPreview() {

        GameManager gm = GameManager.Instance;

        if(gm.gameState != GameState.Waiting) return;

        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(screenPos);
        
        Collider2D hit = Physics2D.OverlapPoint(mousePos);
        if (hit == null || hit.gameObject != gameObject) {
            Display.Instance.DeactiveCoinPreview();
            return;
        }

        Display.Instance.DeselectCoinsAroundCenter(gm.gameBoard, previousMousePos);
        previousMousePos = mousePos;

        int playerId = gm.GetCurrentPlayer();
        int otherPlayerId = playerId == 1 ? 2 : 1;

        if(gm.pendingPowerUpType == TileType.BlowUp) {
            Display.Instance.HighlightCoinsAroundCenter(gm.gameBoard, mousePos, playerId, playerId);
            Display.Instance.HighlightCoinsAroundCenter(gm.gameBoard, mousePos, playerId, otherPlayerId);
        }

        else if(gm.pendingPowerUpType == TileType.SwapNeighbor) {
            Display.Instance.HighlightCoinsAroundCenter(gm.gameBoard, mousePos, playerId, otherPlayerId);
        }
    }
}