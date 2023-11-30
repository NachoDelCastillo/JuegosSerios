using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSelector : MonoBehaviour
{
    [SerializeField]
    private int foodNum = 4;

    private void Start()
    {
        Random.RandomRange(0, foodNum);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
