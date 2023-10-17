using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListSingleUI : MonoBehaviour
{
    [SerializeField] 
    TMP_Text lobbyNameText;

    [SerializeField] 
    TMP_Text numberPlayers;

    Lobby lobby;


    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            GameLobby.Instance.JoinWithId(lobby.Id);
        });
    }

    // Actualiza los valores tanto internos como externos (para que el jugador lo vea)
    // de los datos de la lobby por referencia
    public void SetLobby(Lobby _lobby)
    {
        lobby = _lobby;
        lobbyNameText.text = lobby.Name;

        numberPlayers.text = lobby.Players.Count + " / " + lobby.MaxPlayers;
    }
}