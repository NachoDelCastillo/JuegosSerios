using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodManager : MonoBehaviour
{

    public void FirstLevelFood()
    {
        //Los 5 primeros hijos son los paths del primer nivel
        int randomPath = Random.RandomRange(0, 4);
        transform.GetChild(randomPath).gameObject.SetActive(true);
    }

    public void SecondLevelFood()
    {

        //Los 5 segundos hijos son los paths del segundo nivel
        int randomPath = Random.RandomRange(5, 9);
        transform.GetChild(randomPath).gameObject.SetActive(true);
    }

    public void ThirdLevelFood()
    {
        //Los 5 terceros hijos son los paths del tercer nivel
        int randomPath = Random.RandomRange(10, 14);
        transform.GetChild(randomPath).gameObject.SetActive(true);
    }
}
