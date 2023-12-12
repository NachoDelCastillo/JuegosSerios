using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowTarget : MonoBehaviour
{

    public PlayerMovement Target;
    public float Offset;
    public Transform OffsetDirection;
    bool specting = false;

    // Update is called once per frame
    void Update()
    {

        if (Target != null && OffsetDirection!=null)
        {
            Vector3 MPos = Target.transform.position;
            if (Target.Rigid != null)
                MPos = Target.Rigid.position;
            if(!specting)transform.position = MPos + (OffsetDirection.up * Offset);
            else transform.position = Target.transform.position + (Vector3.up * 2);
        }
        else
        {
            GameObject[] birds = GameObject.FindGameObjectsWithTag("Player");
            Target = birds[0].GetComponent<PlayerMovement>();
            OffsetDirection = birds[0].transform;
            specting = true;
            Debug.Log("Entra al cambio de target");
        }

    }

    public void SetTarget(PlayerMovement target)
    {

        Debug.Log(target);
        Target = target;
    }

    public void SetOffsetDirection(Transform offsetDirection)
    {
        Debug.Log(offsetDirection);
        OffsetDirection = offsetDirection;
    }


    private void Start()
    {
        BirdManager birdManager = Target.GetComponentInParent<BirdManager>();

        if (!birdManager.IsOwner)
            if (birdManager.DEBUG == false)
                Destroy(gameObject);
    }
}
