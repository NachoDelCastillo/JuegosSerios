using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowHideObj : MonoBehaviour
{
    protected GameplayManager gameplayManager;
    GameObject obj;

    protected void Awake()
    {
        obj = transform.GetChild(0).gameObject;
    }

    protected void Start()
    {
        gameplayManager = GameplayManager.Instance;
    }

    public void Show()
    { obj.SetActive(true); }

    protected void Hide()
    { obj.SetActive(false); }

    public bool IsObjectEnabled()
    { return obj.activeInHierarchy; }
}
