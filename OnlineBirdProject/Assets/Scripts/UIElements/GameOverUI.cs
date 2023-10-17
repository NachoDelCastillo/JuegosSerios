using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameOverUI : ShowHideObj
{
    [SerializeField]
    TMP_Text backToLobbyText;

    new void Start()
    {
        base.Start();
        gameplayManager.OnStateChanged += StateChanged;
    }

    private void StateChanged(object sender, GameplayManager.State newState)
    {
        if (newState == GameplayManager.State.GameOver)
            Show();
        else
            Hide();
    }

    private void Update()
    {
        if (IsObjectEnabled())
        {
            int cleanTimer = Mathf.CeilToInt(GameplayManager.Instance.GetGameOverTimer());

            backToLobbyText.text = "Returning to Lobby in " +
                cleanTimer + " seconds";
        }
    }

}
