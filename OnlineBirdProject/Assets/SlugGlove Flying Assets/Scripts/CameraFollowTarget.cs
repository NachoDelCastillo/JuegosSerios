using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowTarget : MonoBehaviour
{

    public PlayerMovement Target;
    public float Offset;
    public Transform OffsetDirection;

    // Update is called once per frame
    void Update()
    {
        Vector3 MPoss = Vector3.up;
        transform.position = Target.transform.position + (MPoss * 2);
        return;

        if (Target != null && OffsetDirection!=null)
        {
            Vector3 MPos = Vector3.up;

            if (Target.Rigid != null)
                MPos = Target.Rigid.position;
            transform.position = Target.transform.position + (MPos * 2);
        }
        else
        {
            GameObject[] birds = GameObject.FindGameObjectsWithTag("Player");
            Target = birds[0].GetComponent<PlayerMovement>();
            OffsetDirection = birds[0].transform;
        }

    }

    public void SetTarget(PlayerMovement target)
    {
        return;
        Debug.Log("Se llama");
        Debug.Log(target);
        Target = target;
    }

    public void SetOffsetDirection(Transform offsetDirection)
    {
        return;
        Debug.Log("Se llama2");
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
