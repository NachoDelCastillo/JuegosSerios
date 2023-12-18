using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class Food : MonoBehaviour
{

    [SerializeField]
    private EventReference eatSound;
    [SerializeField]
    private int foodCount = 4;

    private void Start()
    {
        Invoke("ConfigureBirds", 4);
    }

    BirdManager[] allBirds;

    BirdManager localBird;

    void ConfigureBirds()
    {
        allBirds = GameplayManager.Instance.allBirds.ToArray();

        for (int i = 0; i < allBirds.Length; i++)
        {
            if (allBirds[i].IsOwner)
                localBird = allBirds[i];
        }
    }

    private void Update()
    {
        if (Vector3.Distance(localBird.transform.position, transform.position) < 10)
        {
            localBird.GetComponent<LifeBar>().eatFood(foodCount);
            AudioManager_PK.GetInstance().PlayOneShoot(eatSound, transform.position);

            Destroy(this.gameObject);
        }
    }

    IEnumerator EatFood()
    {

        yield return new WaitForSeconds(1);
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
