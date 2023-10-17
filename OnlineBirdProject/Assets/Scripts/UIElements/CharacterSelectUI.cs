using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    [SerializeField]
    Button mainMenuButton;

    [SerializeField]
    Button readyButton;

    [SerializeField]
    TMP_Text lobbyNameText;

    [SerializeField]
    TMP_Text lobbyCodeText;

    private void Awake()
    {
        if (GameManager.IsLocal())
        {
            mainMenuButton.onClick.AddListener(() =>
            {
                GameManager.instance.ChangeScene(SceneLoader.SceneName.MainMenuScene);
            });
        }
        else if (GameManager.IsOnline())
        {
            mainMenuButton.onClick.AddListener(() =>
            {
                if (NetworkManager.Singleton.IsServer)
                    GameLobby.Instance.DeleteLobby();
                else
                {
                    GameLobby.Instance.LeaveLobby();
                }
                NetworkManager.Singleton.Shutdown();
                SceneLoader.Load(SceneLoader.SceneName.MainMenuScene);
            });

            readyButton.onClick.AddListener(() =>
            {
                CharacterSelect_Online.Instance.SetPlayerReady();
            });
        }
    }

    private void Start()
    {
        if (GameManager.IsOnline())
        {
            // Si se viene de terminar una partida
            Lobby lobby = GameLobby.Instance.GetLobby();

            if (lobby != null)
            {
                lobbyNameText.text = "Lobby Name : " + lobby.Name;
                lobbyCodeText.text = "Lobby Code : " + lobby.LobbyCode;
            }
            else
            {
                lobbyNameText.gameObject.SetActive(false);
                lobbyCodeText.gameObject.SetActive(false);
            }
        }
    }
}
