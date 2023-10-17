using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CountDownToStartUI : MonoBehaviour
{
    GameplayManager gameplayManager;

    [SerializeField]
    TMP_Text countDownToStart_Text;

    GameObject obj;

    void Start()
    {
        gameplayManager = GameplayManager.Instance;
        obj = transform.GetChild(0).gameObject;

        gameplayManager.OnStateChanged += StateChanged;
    }

    private void StateChanged(object sender, GameplayManager.State newState)
    {
        if (newState == GameplayManager.State.CountdownToStart)
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
        if (gameplayManager.GetState() == GameplayManager.State.CountdownToStart)
        {
            float rawTimer = gameplayManager.GetCountDownToStartTimer();

            if (rawTimer > 0)
            {
                int cleanTimer = Mathf.CeilToInt(rawTimer);
                countDownToStart_Text.text = cleanTimer.ToString();
            }
        }
    }
}
