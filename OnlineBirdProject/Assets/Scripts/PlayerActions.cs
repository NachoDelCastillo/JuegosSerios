using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerActions : MonoBehaviour
{
    [SerializeField]
    Transform cone;

    [SerializeField]
    Transform[] bodies;

    [SerializeField]
    CapsuleCollider playerCollider;

    Animator animator;

    PlayerLocomotion playerLocomotion;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        playerLocomotion = GetComponent<PlayerLocomotion>();

        Physics.IgnoreCollision(playerCollider, cone.GetComponent<MeshCollider>());
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
            TurnIntoCone();
    }

    public void Dance()
    {
        playerLocomotion.PlayTargetAnimation("Dance", true);
    }

    public void TurnIntoCone()
    {
        cone.SetParent(transform);
        cone.GetComponent<MeshCollider>().enabled = true;
        cone.AddComponent<Rigidbody>();

        //playerCollider.enabled = false;


        animator.SetBool("isInteracting", true);

        for (int i = 0; i < bodies.Length; i++)
            bodies[i].gameObject.SetActive(false);
    }
}
