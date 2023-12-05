using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    [SerializeField]
    private int foodCount = 4;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<BirdManager>() && other.gameObject.GetComponent<BirdManager>().IsOwner)
        {

            Debug.Log("Se ha comida la comidita");
            Destroy(this.gameObject);
        }

    }
}
