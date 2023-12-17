using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class Food : MonoBehaviour
{

    [SerializeField]
    private EventReference eatSound;
    [SerializeField]
    private int foodCount = 4;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<BirdManager>() && other.GetComponent<BirdManager>().IsOwner)
        {
            other.GetComponent<LifeBar>().eatFood(foodCount);
            AudioManager_PK.GetInstance().PlayOneShoot(eatSound, transform.position);
        }

        if (other.gameObject.GetComponent<BirdManager>()) { Destroy(this.gameObject); }

    }
}
