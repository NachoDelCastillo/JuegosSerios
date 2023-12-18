using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class LifeBar : MonoBehaviour
{
    private Image barImage;
    private Image barImage2;
    [SerializeField]
    private float lifeAmountDecreasing = 0.4f; //0.000000000001f;

    BirdManager birdManager;


    private void Start()
    {
        barImage = GameObject.Find("Lifes").GetComponent<Image>();
        barImage2 = GameObject.Find("Lifes2").GetComponent<Image>();

        birdManager = GetComponent<BirdManager>();

        if (birdManager.IsOwner)
            barImage2.transform.parent.gameObject.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        barImage.fillAmount -= lifeAmountDecreasing * Time.deltaTime;
        barImage2.fillAmount -= lifeAmountDecreasing * Time.deltaTime;

        if (barImage.fillAmount <= 0 && barImage2.fillAmount <= 0 && transform.GetComponent<BirdManager>().IsOwner) {
            GameObject cameraFollow = GameObject.Find("CameraFollow");
            Debug.Log(cameraFollow);
            GameObject[] birds = GameObject.FindGameObjectsWithTag("Player");
            int i = 0;
            Debug.Log(birds.Length);
            while (birds[i] == this || !birds[i].activeInHierarchy)
                i++;

            Debug.Log(birds[i]);
            cameraFollow.GetComponent<CameraFollow>().target = birds[i].transform;
            cameraFollow.GetComponent<CameraFollowTarget>().SetTarget(birds[i].GetComponent<PlayerMovement>());
            cameraFollow.GetComponent<CameraFollowTarget>().SetOffsetDirection(birds[i].transform);

            //Destroy(this.gameObject);

            GameplayManager.Instance.birdDestroyed(birdManager);
        }

        if (birdManager.IsOwner)
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                barImage.fillAmount -= 20;
                barImage2.fillAmount -= 20;
            }

            if (Input.GetKeyDown(KeyCode.Y))
            {
                barImage.fillAmount += 20;
                barImage2.fillAmount += 20;
            }
        }
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
