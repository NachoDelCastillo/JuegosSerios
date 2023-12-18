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
        //BirdManager[] allBirds = FindObjectsByType<BirdManager>(FindObjectsSortMode.None);

        //for (int i = 0; i < allBirds.Length; i++)
        //{
        //    if (allBirds[i].IsOwner)
        //        localBird = allBirds[i];
        //}

    }

    bool foodEaten = false;

    private void Update()
    {
        BirdManager localBird = GameplayManager.Instance.localBird;

        if (!foodEaten && localBird != null && Vector3.Distance(localBird.transform.position, transform.position) < 5)
        {
            foodEaten = true;
            localBird.GetComponent<LifeBar>().eatFood(foodCount);
            AudioManager_PK.GetInstance().PlayOneShoot(eatSound, transform.position);

            Destroy(this.gameObject);

            //StartCoroutine(EatFood());
        }
    }

    //IEnumerator EatFood()
    //{
    //    Vector3 finalPos = localBird.transform.position + localBird.transform.forward * 2;
    //    transform.DOMove(finalPos, .5f);

    //    yield return new WaitForSeconds(1);
    //    Destroy(this.gameObject);
    //}

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
