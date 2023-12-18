using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Haz : MonoBehaviour
{
    BirdManager localBird;


    private void Update()
    {
        //BirdManager localBird = GameplayManager.Instance.localBird;

        if (localBird == null)
        {
            BirdManager[] allBirds = FindObjectsByType<BirdManager>(FindObjectsSortMode.None);

            for (int i = 0; i < allBirds.Length; i++)
            {
                if (allBirds[i].IsOwner)
                    localBird = allBirds[i];
            }
        }


        if (localBird != null)
        {
            Vector2 localBirdPosition = new Vector2(localBird.transform.position.x, localBird.transform.position.z);
            Vector2 myPosition = new Vector2(transform.position.x, transform.position.z);

            if (Vector2.Distance(localBirdPosition, myPosition) < 35)
            {
                gameObject.SetActive(false);
                //localBird.GetComponent<LifeBar>().eatFood(foodCount);
                //AudioManager_PK.GetInstance().PlayOneShoot(eatSound, transform.position);
            }
        }

    }
}
