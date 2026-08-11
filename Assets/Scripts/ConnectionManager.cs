using UnityEngine;
using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;

public class ConnectionManager : MonoBehaviour {

    public static ConnectionManager Instance { get; private set; }

    ISession currentSession;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this.gameObject);
        }
        else {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    async void Start() {
	    try {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log($"Sign in anonymously succeeded! PlayerID: {AuthenticationService.Instance.PlayerId}");
	    }
	    catch (Exception e) {
	        Debug.LogException(e);
	    }
    }

    // start new connection as host, returns the join code
    public async Task<string> StartGameHost() {
        var options = new SessionOptions{MaxPlayers = 2}.WithRelayNetwork().WithNetworkOptions(new NetworkOptions{RelayProtocol = RelayProtocol.WSS});

        var session = await MultiplayerService.Instance.CreateSessionAsync(options);
        Debug.Log($"Session {session.Id} created! Join code: {session.Code}");

        return session.Code;
    }

    public async Task<bool> JoinGameClient(string joinCode) {
        try {
            var options = new JoinSessionOptions{}.WithNetworkOptions(new NetworkOptions{RelayProtocol = RelayProtocol.WSS});

            currentSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(joinCode, options);
            Debug.Log("Created session " + currentSession);
            return true;
        }
        catch (Exception e) {
	        Debug.LogException(e);
            return false;
	    }
    }
}