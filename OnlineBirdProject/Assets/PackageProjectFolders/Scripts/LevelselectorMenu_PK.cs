using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelselectorMenu_PK : MenuManager_PK
{
    AllMenuManager_PK allMenuManager;

    protected override void ExtraAwake()
    {
        allMenuManager = GetComponentInParent<AllMenuManager_PK>();
    }

    protected override void buttonPressed(int index)
    {
        base.buttonPressed(index);

        if (index == 0)
            PressLocalMultiplayer();
        else if (index == 1)
            PressOnlineMultiplayer();

        else if (index == nButtons-1) 
            allMenuManager.BackButton();
    }

    public void PressLocalMultiplayer()
    {
        GameManager.SetPlayMode(GameManager.PlayMode.local);
        GameManager.GetInstance().ChangeScene(SceneLoader.SceneName.CharacterSelectScene);
    }

    public void PressOnlineMultiplayer()
    {
        GameManager.SetPlayMode(GameManager.PlayMode.online);
        GameManager.GetInstance().ChangeScene(SceneLoader.SceneName.LobbyScene);
    }
}
