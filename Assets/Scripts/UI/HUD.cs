using UnityEngine;

public class HUD: MonoBehaviour {

    public static HUD Instance { get; private set; }

    [Header("PowerUps")]
    public GameObject powerUpPrefab;
    public Transform[] player1Slots; // places to spawn each powerup
    public Transform[] player2Slots;
    public GameObject cancelPowerUpPrefab;

    GameObject[] player1SlotItems;
    GameObject[] player2SlotItems;

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
        GameManager.Instance.gameState = GameState.Playing;
    }
}