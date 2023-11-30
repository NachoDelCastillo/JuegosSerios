using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSelector : MonoBehaviour
{
    [SerializeField]
    private int foodNum = 4;

    private void OnEnable()
    {
        Debug.Log("Me llamo");
        transform.GetChild(1).gameObject.SetActive(true);
    }
}
