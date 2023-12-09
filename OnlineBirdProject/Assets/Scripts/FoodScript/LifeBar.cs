using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LifeBar : MonoBehaviour
{
    private Image barImage;
    private Image barImage2;
    [SerializeField]
    private float lifeAmountDecreasing = 0.000000000001f;


    private void Start()
    {
        barImage = GameObject.Find("Lifes").GetComponent<Image>();
        barImage2 = GameObject.Find("Lifes2").GetComponent<Image>();
    }
    // Update is called once per frame
    void Update()
    {
        barImage.fillAmount -= lifeAmountDecreasing*Time.deltaTime;
        barImage2.fillAmount -= lifeAmountDecreasing*Time.deltaTime;
    }


    public void eatFood(int foodAmount)
    {
        //Si la suma de la comida es mayor que el m�ximo, se pone el m�ximo.
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
