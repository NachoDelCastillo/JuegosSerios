using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplayTimerUI : MonoBehaviour
{
    GameplayManager gameplayManager;

    [SerializeField]
    Image countDown_Img;

    GameObject obj;

    void Start()
    {
        gameplayManager = GameplayManager.Instance;
        obj = transform.GetChild(0).gameObject;

        gameplayManager.OnStateChanged += StateChanged;

        Hide();
    }

    private void StateChanged(object sender, GameplayManager.State newState)
    {
        if (newState == GameplayManager.State.GamePlaying)
            Show();

        else
            Hide();
    }

    void Show()
    { obj.SetActive(true); }

    void Hide()
    { obj.SetActive(false); }

    void Update()
    {
        if (gameplayManager.GetState() == GameplayManager.State.GamePlaying)
        {
            float rawTimer = gameplayManager.GetGameplayTimer();
            float maxTimer = GameplayManager.maxGameplayTimer;
            countDown_Img.fillAmount = (maxTimer - rawTimer ) / maxTimer;
        }
    }
}
