using Mono.CSharp;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCreateUI : ShowHideObj
{
    [SerializeField]
    TMP_InputField lobbyNameInputField;
    [SerializeField]
    TMP_Text maxPlayersText;
    [SerializeField]
    Slider maxPlayerSlider;
    [SerializeField]
    Button createPrivateButton;
    [SerializeField]
    Button createPublicButton;
    [SerializeField]
    Button closeButton;

    private new void Awake()
    {
        base.Awake();

        maxPlayerSlider.onValueChanged.AddListener(MaxPlayerSliderChanged);

        createPublicButton.onClick.AddListener(() =>
        {
            GameLobby.Instance.CreateLobby(lobbyNameInputField.text, false);
        });

        createPrivateButton.onClick.AddListener(() =>
        {
            GameLobby.Instance.CreateLobby(lobbyNameInputField.text, true);
        });

        closeButton.onClick.AddListener(() =>
        {
            Hide();
        });

        Hide();
    }

    public void MaxPlayerSliderChanged(float value)
    {
        maxPlayersText.text = "MAX PLAYERS  " + value.ToString();
        OnlineMultiplayerManager.MAX_PLAYER_AMOUNT = (int) value;
    }
}
