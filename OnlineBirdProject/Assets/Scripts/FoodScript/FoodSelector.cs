using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSelector : MonoBehaviour
{
    [SerializeField]
    private int foodNum = 4;

    private void OnEnable()
    {
        transform.GetChild(Random.RandomRange(0,foodNum)).gameObject.SetActive(true);
    }
}
