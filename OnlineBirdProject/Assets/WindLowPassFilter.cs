using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindLowPassFilter : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entra");
        if (other.GetComponent<BirdManager>())
        {

            other.GetComponent<BirdSoundManager>().instance.setParameterByName("Obstruction",1);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<BirdManager>())
        {
            Debug.Log("Sale");
            other.GetComponent<BirdSoundManager>().instance.setParameterByName("Obstruction", 0);
        }
    }
}
