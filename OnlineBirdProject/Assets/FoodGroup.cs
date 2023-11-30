using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodGroup : MonoBehaviour
{
    private void OnEnable()
    {
        transform.GetChild(0).gameObject.SetActive(true);
    }
}
