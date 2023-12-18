using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodManager : MonoBehaviour
{

    static public FoodManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    private void Start()
    {
    }

    GameObject currentPath;

    public void FirstLevelFood()
    {
        Debug.Log("FOOD LOADED Lvl 1");

        if (currentPath != null)
            currentPath.SetActive(false);

        //Los 5 primeros hijos son los paths del primer nivel
        int randomPath = 0; // Random.RandomRange(0, 4);

        currentPath = transform.GetChild(randomPath).gameObject;
        currentPath.SetActive(true);
    }

    public void SecondLevelFood()
    {
        Debug.Log("FOOD LOADED Lvl 2");
        if (currentPath != null)
            currentPath.SetActive(false);

        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).gameObject.SetActive(false);

        //Los 5 segundos hijos son los paths del segundo nivel
        int randomPath = Random.Range(5, 9);
        //transform.GetChild(randomPath).gameObject.SetActive(true);

        randomPath = 5;
        currentPath = transform.GetChild(randomPath).gameObject;
        currentPath.SetActive(true);
    }

    public void ThirdLevelFood()
    {
        Debug.Log("FOOD LOADED Lvl 3");
        if (currentPath != null)
            currentPath.SetActive(false);

        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).gameObject.SetActive(false);

        //Los 5 terceros hijos son los paths del tercer nivel
        int randomPath = Random.Range(10, 14);
        //transform.GetChild(randomPath).gameObject.SetActive(true);
        randomPath = 10;
        currentPath = transform.GetChild(randomPath).gameObject;
        currentPath.SetActive(true);
    }
}
