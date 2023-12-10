using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Haz : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<BirdManager>().IsOwner)
        {
            gameObject.SetActive(false);
        }
    }
}
