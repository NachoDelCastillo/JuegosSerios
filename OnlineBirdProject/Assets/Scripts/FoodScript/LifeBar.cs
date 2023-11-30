using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LifeBar : MonoBehaviour
{
    public Image barImage;
    public Image barImage2;
    [SerializeField]
    private float lifeAmountDecreasing = 3f;

    // Update is called once per frame
    void Update()
    {
        barImage.fillAmount -= lifeAmountDecreasing*Time.deltaTime;
        barImage2.fillAmount -= lifeAmountDecreasing*Time.deltaTime;
    }


    public void eatFood(int foodAmount)
    {
        //Si la suma de la comida es mayor que el máximo, se pone el máximo.
        if (barImage.fillAmount + foodAmount > 100)
        {
            barImage.fillAmount = 100;
            barImage2.fillAmount = 100;
        }

        else
        {
            barImage.fillAmount += foodAmount;
            barImage2.fillAmount += foodAmount;

        }
    }
}
