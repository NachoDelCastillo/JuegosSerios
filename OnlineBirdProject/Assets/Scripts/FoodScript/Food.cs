using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    [SerializeField]
    private int foodCount = 4;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<BirdManager>() && other.GetComponent<BirdManager>().IsOwner)
        {
            other.GetComponent<LifeBar>().eatFood(foodCount);
        }

        if (other.gameObject.GetComponent<BirdManager>()) { Destroy(this.gameObject); }

    }
}
