using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectingUI : ShowHideObj
{
    private new void Start()
    {
        base.Start();

        OnlineMultiplayerManager.Instance.OnTryingToJoinGame += GameMultiplayer_OnTryingToJoinGame;
        OnlineMultiplayerManager.Instance.OnFailedToJoinGame += GameMultiplayer_OnFailedToJoinGame;

        Hide();
    }

    private void GameMultiplayer_OnFailedToJoinGame(object sender, EventArgs e)
    {
        Hide();
    }

    private void GameMultiplayer_OnTryingToJoinGame(object sender, EventArgs e)
    {
        Show();
    }

    private void OnDestroy()
    {
        OnlineMultiplayerManager.Instance.OnTryingToJoinGame -= GameMultiplayer_OnTryingToJoinGame;
        OnlineMultiplayerManager.Instance.OnFailedToJoinGame -= GameMultiplayer_OnFailedToJoinGame;
    }
}