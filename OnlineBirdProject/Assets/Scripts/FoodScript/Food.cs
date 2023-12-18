using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using DG.Tweening;

public class Food : MonoBehaviour
{

    [SerializeField]
    private EventReference eatSound;
    [SerializeField]
    private int foodCount = 4;

    private void Start()
    {
        Invoke("ConfigureBirds", 10);
    }

    BirdManager localBird;

    void ConfigureBirds()
    {
        BirdManager[] allBirds = FindObjectsByType<BirdManager>(FindObjectsSortMode.None);

        for (int i = 0; i < allBirds.Length; i++)
        {
            if (allBirds[i].IsOwner)
                localBird = allBirds[i];
        }

    }

    bool foodEaten = false;
    bool eatingFood = false;
    float eatSpeed = 5;
    float eatTime = 1;

    private void Update()
    {
        //BirdManager localBird = GameplayManager.Instance.localBird;

        BirdManager[] allBirds = FindObjectsByType<BirdManager>(FindObjectsSortMode.None);

        for (int i = 0; i < allBirds.Length; i++)
        {
            if (allBirds[i].IsOwner)
                localBird = allBirds[i];
        }

        if (!foodEaten && localBird != null && Vector3.Distance(localBird.transform.position, transform.position) < 5)
        {
            foodEaten = true;
            localBird.GetComponent<LifeBar>().eatFood(foodCount);
            AudioManager_PK.GetInstance().PlayOneShoot(eatSound, transform.position);

            //Destroy(this.gameObject);
            eatingFood = true;

            StartCoroutine(EatFood());
        }

        if (eatingFood)
        {
            transform.position = Vector3.Lerp(transform.position, localBird.transform.position, Time.deltaTime * eatSpeed);
        }
    }

    IEnumerator EatFood()
    {
        //Vector3 finalPos = localBird.transform.position + localBird.transform.forward * 2;
        //transform.DOMove(finalPos, eatTime);
        transform.DOScale(Vector3.zero, eatTime);
        transform.DORotate(new Vector3(0, 360, 0), eatTime, RotateMode.FastBeyond360);

        yield return new WaitForSeconds(eatTime);

        eatingFood = false;
        Destroy(this.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        //if (other.gameObject.GetComponent<BirdManager>() && other.GetComponent<BirdManager>().IsOwner)
        //{
        //    other.GetComponent<LifeBar>().eatFood(foodCount);
        //    AudioManager_PK.GetInstance().PlayOneShoot(eatSound, transform.position);
        //}

        //if (Vector3.Distance(localBird.transform.position, transform.position) < 10)
        //{
        //    localBird.GetComponent<LifeBar>().eatFood(foodCount);
        //    AudioManager_PK.GetInstance().PlayOneShoot(eatSound, transform.position);

        //    Destroy(this.gameObject);
        //}


        //if (other.gameObject.GetComponent<BirdManager>()) { Destroy(this.gameObject); }

    }
}
