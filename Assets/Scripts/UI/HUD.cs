using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class HUD: MonoBehaviour {

    public static HUD Instance { get; private set; }

    public TMP_Text currentPlayerText;
    public AudioClip clickButtonAudio;

    [Header("PowerUps")]
    public GameObject powerUpPrefab;
    public Transform[] player1Slots; // places to spawn each powerup
    public Transform[] player2Slots;
    public GameObject cancelPowerUpPrefab;

    GameObject[] player1SlotItems;
    GameObject[] player2SlotItems;

    // public event Action CancelledPowerup;

    [Header("Game Over")]
    public GameObject gameOverScreen;
    public TMP_Text gameOverText;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this.gameObject);
        }
        else {
            Instance = this;
        }

        player1SlotItems = new GameObject[player1Slots.Length];
        player2SlotItems = new GameObject[player2Slots.Length];

        cancelPowerUpPrefab.SetActive(false);
        gameOverScreen.SetActive(false);
    }

    public void AddPowerUp(int playerId, int slotIdx, TileType powerUpType) {
        Transform[] slots = playerId == 1 ? player1Slots : player2Slots;
        GameObject[] slotItems = playerId == 1 ? player1SlotItems : player2SlotItems;

        GameObject itemGO = Instantiate(powerUpPrefab, slots[slotIdx]);
        slotItems[slotIdx] = itemGO;

        PowerUpItem powerUpItem = itemGO.GetComponent<PowerUpItem>();
        powerUpItem.Setup(playerId, powerUpType, slotIdx);

        powerUpItem.TriggerPowerUp += GameManager.Instance.HandlePowerUp;
    }

    public void RemovePowerUp(int playerId, int slotIdx) {
        GameObject[] slotItems = playerId == 1 ? player1SlotItems : player2SlotItems;

        if (slotItems[slotIdx] != null) {
            Destroy(slotItems[slotIdx]);
            slotItems[slotIdx] = null;
        }
    }

    public void EnableCancellingPowerUp() {
        cancelPowerUpPrefab.SetActive(true);
    }

    public void HideCancelPowerUp() {
        cancelPowerUpPrefab.SetActive(false);
        // GameManager.Instance.gameState = GameState.Playing;
        // CancelledPowerup?.Invoke();
        GameManager.Instance.HandleCancelPowerup();
    }

    public void UpdateCurrPlayer(string text, int currPlayer) {
        currentPlayerText.text = text + currPlayer;
    }

    public void ShowWinScreen(int winner) {
        gameOverScreen.SetActive(true);
        gameOverText.text = "Winner player " + winner;
    }

    public void RematchGame() {
        GameManager.Instance.RestartGame();
    }

    public void ResetHUD() {
        gameOverScreen.SetActive(false);

        for (int i = 1; i < 3; i++) {
            for (int j = 0; j < 3; j++) {
                RemovePowerUp(i, j);
            }
        }
    }

    public void QuitToMenu() {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public async void QuitToMenuOnline() {
        Time.timeScale = 1f;
        await ConnectionManager.Instance.LeaveSession();
        // NetworkManager.Singleton.SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        SceneManager.LoadScene("MainMenu");
    }
}