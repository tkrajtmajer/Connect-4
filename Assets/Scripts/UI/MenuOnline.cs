using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class MenuOnline : MonoBehaviour
{
    public TMP_InputField joinCodeText;
    public TMP_InputField joinCodeEnterField;
    public GameObject playGameButton;

    void Start() {
        if(playGameButton != null) playGameButton.SetActive(false);
    }

    void OnEnable() {
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
    }

    void OnDisable() {
        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
    }

    // generate new join code 
    public async void StartGameHost() {
        string joinCode = await ConnectionManager.Instance.StartGameHost();
        joinCodeText.text = joinCode;
    }

    public async void JoinGameClient() {
        await ConnectionManager.Instance.JoinGameClient(joinCodeEnterField.text);
    }

    void HandleClientConnected(ulong _) {
        if (NetworkManager.Singleton.IsHost && NetworkManager.Singleton.ConnectedClientsList.Count >= 2) {
            if(playGameButton != null) playGameButton.SetActive(true);
        }
    }

    public void EnterGameScene() {
        NetworkManager.Singleton.SceneManager.LoadScene("OnlineMultiplayerTest", LoadSceneMode.Single);
    }
}
