using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectTemplate : MonoBehaviour
{
    [SerializeField] int playerIndex;

    [SerializeField] GameObject readyGameObject;
    [SerializeField] PlayerVisual characterVisual;

    [SerializeField] Button kickButton;
    [SerializeField] TMP_Text playerNameText;

    GameObject objVisual;

    private void Awake()
    {
        objVisual = transform.GetChild(0).gameObject;
    }

    private void Start()
    {
        readyGameObject.SetActive(false);

        if (GameManager.IsLocal())
        {

        }

        else if (GameManager.IsOnline())
        {
            OnlineMultiplayerManager.Instance.OnPlayerDataNetworkListChanged += GameMultiplayer_OnPlayerDataNetworkListChanged;
            CharacterSelect_Online.Instance.OnReadyChanged += CharacterSelectReady_OnReadyChanged;

            // Para asegurarnos de que este boton solo aparece desde la pantalla del host
            if (NetworkManager.Singleton.IsServer)
            {
                kickButton.gameObject.SetActive(true);
                kickButton.onClick.AddListener(() =>
                {
                    PlayerData playerData = OnlineMultiplayerManager.Instance.GetPlayerDataFromIndex(playerIndex);
                    GameLobby.Instance.KickPlayer(playerData.playerId.ToString());
                    OnlineMultiplayerManager.Instance.KickPlayer(playerData.clientId);
                });
            }
            else
                kickButton.gameObject.SetActive(false);

            UpdatePlayer();
        }
    }

    private void CharacterSelectReady_OnReadyChanged(object sender, EventArgs e)
    { UpdatePlayer(); }
    private void GameMultiplayer_OnPlayerDataNetworkListChanged(object sender, EventArgs e)
    { UpdatePlayer(); }

    void UpdatePlayer()
    {
        if (GameManager.IsLocal())
        {

        }

        else if (GameManager.IsOnline())
        {
            if (OnlineMultiplayerManager.Instance.IsPlayerIndexConnected(playerIndex))
            {
                ShowCharacter();

                PlayerData playerData = OnlineMultiplayerManager.Instance.GetPlayerDataFromIndex(playerIndex);
                readyGameObject.SetActive(CharacterSelect_Online.Instance.IsPlayerReady(playerData.clientId));
                playerNameText.text = playerData.playerName.ToString();
                characterVisual.SetPlayerMaterial(OnlineMultiplayerManager.Instance.GetPlayerMaterial(playerData.colorId));
                characterVisual.SetNeonColorById(playerData.colorId);
            }

            else
            {
                HideCharacter();
                readyGameObject.SetActive(false);
            }
        }
    }

    void ShowCharacter()
    {
        objVisual.SetActive(true);
    }

    void HideCharacter()
    {
        objVisual.SetActive(false);
    }

    private void OnDestroy()
    {
        if (GameManager.IsOnline())
            OnlineMultiplayerManager.Instance.OnPlayerDataNetworkListChanged -= GameMultiplayer_OnPlayerDataNetworkListChanged;
    }
}
