using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMessageUI : ShowHideObj
{
    [SerializeField]
    TMP_Text messageText;
    [SerializeField]
    Button closeButton;

    new void Start()
    {
        base.Start();

        closeButton.onClick.AddListener(Hide);

        OnlineMultiplayerManager.Instance.OnFailedToJoinGame += GameMultiplayer_OnFailedToJoinGame;

        // Mensajes de carga y fail
        // Creacion de lobby
        GameLobby.Instance.OnCreateLobbyStarted += GameLobby_OnCreateLobbyStarted;
        GameLobby.Instance.OnCreateLobbyFailed += GameLobby_OnCreatedLobbyFailed;
        // Unirse a una lobby
        GameLobby.Instance.OnJoinStarted += GameLobby_OnJoinStarted;
        GameLobby.Instance.OnJoinFailed += GameLobby_OnJoinFailed;
        GameLobby.Instance.OnQuickJoinFailed += GameLobby_OnQuickJoinFailed;

        Hide();
    }


    // Unirse a una lobby
    private void GameLobby_OnJoinStarted(object sender, EventArgs e)
    { ShowMessage("Joining Lobby..."); }

    private void GameLobby_OnJoinFailed(object sender, EventArgs e)
    { ShowMessage("Failed to join Lobby"); }

    private void GameLobby_OnQuickJoinFailed(object sender, EventArgs e)
    { ShowMessage("Could not find a Lobby to Quick Join"); }

    // Crear una lobby
    private void GameLobby_OnCreateLobbyStarted(object sender, EventArgs e)
    { ShowMessage("Creating Lobby..."); }

    private void GameLobby_OnCreatedLobbyFailed(object sender, EventArgs e)
    { ShowMessage("Failed to create Lobby"); }

    private void GameMultiplayer_OnFailedToJoinGame(object sender, EventArgs e)
    {
        if (NetworkManager.Singleton.DisconnectReason == "")
            ShowMessage("FailedToConnect");
        else
            ShowMessage(NetworkManager.Singleton.DisconnectReason);
    }

    void ShowMessage(string message)
    {
        Show();
        messageText.text = message;
    }

    private void OnDestroy()
    {
        OnlineMultiplayerManager.Instance.OnFailedToJoinGame -= GameMultiplayer_OnFailedToJoinGame;

        GameLobby.Instance.OnCreateLobbyStarted -= GameLobby_OnCreateLobbyStarted;
        GameLobby.Instance.OnCreateLobbyFailed -= GameLobby_OnCreatedLobbyFailed;
        GameLobby.Instance.OnJoinStarted -= GameLobby_OnJoinStarted;
        GameLobby.Instance.OnJoinFailed -= GameLobby_OnJoinFailed;
        GameLobby.Instance.OnQuickJoinFailed -= GameLobby_OnQuickJoinFailed;
    }
}
