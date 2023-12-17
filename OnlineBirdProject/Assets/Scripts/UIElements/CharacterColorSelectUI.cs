using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterColorSelectUI : MonoBehaviour
{
    [SerializeField]
    int colorId;
    [SerializeField]
    Image image;
    [SerializeField]
    GameObject selectedGameObject;

    private void Start()
    {
        if (GameManager.IsLocal())
        {
            gameObject.SetActive(false);
            return;
        }

        GetComponent<Button>().onClick.AddListener(() =>
        {
            OnlineMultiplayerManager.Instance.ChangePlayerMaterial(colorId);
            FMODUnity.RuntimeManager.PlayOneShot("event:/PressButton");
        });

        OnlineMultiplayerManager.Instance.OnPlayerDataNetworkListChanged += GameMultiplayer_OnPlayerDataNetworkListChanged;

        UpdateIsSelected();
    }

    // Cada vez que se conecte o desconecte un jugador, se llamara este metodo
    private void GameMultiplayer_OnPlayerDataNetworkListChanged(object sender, EventArgs e)
    {
        UpdateIsSelected();
    }

    void UpdateIsSelected()
    {
        if (GameManager.IsOnline())
        {
            if (OnlineMultiplayerManager.Instance.GetPlayerData().colorId == colorId)
                selectedGameObject.SetActive(true);
            else
                selectedGameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (GameManager.IsOnline())
            OnlineMultiplayerManager.Instance.OnPlayerDataNetworkListChanged -= GameMultiplayer_OnPlayerDataNetworkListChanged;
    }
}
