using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerLocomotion : MonoBehaviour
{
    Animator animator;
    CameraHandler cameraHandler;
    PlayerManager playerManager;
    Transform cameraObject;
    InputHandler inputHandler;

    Vector3 moveDirection;
    // Valor entre 0 y 1 que indica con que intensidad se esta moviendo el personaje
    // Siendo 0 no moverse, y 1 siendo andando a velocidad de andar maxima
    float moveIIntensity;

    [HideInInspector]
    public Transform myTransform;

    public new Rigidbody rigidbody;
    public GameObject normalCamera;

    [Header("Movement Stats")]
    [SerializeField]
    float movementSpeed = 5;
    [SerializeField]
    float sprintSpeed = 10;
    [SerializeField]
    float rotationSpeed = 10;

    public CapsuleCollider characterCollider;
    public CapsuleCollider characterCollisionBlockerCollider;

    // Se ira hacia este punto si la variable del playermanager de moving esta a true
    Transform interactionTarget;

    public bool movingIntoPosition;
    public bool rotatingIntoPosition;

    private void Awake()
    {
        cameraHandler = FindObjectOfType<CameraHandler>();
    }

    private void Start()
    {
        playerManager = GetComponent<PlayerManager>();
        rigidbody = GetComponent<Rigidbody>();
        inputHandler = GetComponent<InputHandler>();
        cameraObject = Camera.main.transform;
        myTransform = transform;
        animator = GetComponentInChildren<Animator>();

        //Physics.IgnoreCollision(characterCollider, characterCollisionBlockerCollider, true);
    }

    #region Movement

    Vector3 normalVector;
    Vector3 targetPosition;

    public void HandleRotation(float delta)
    {
        Vector3 targetDir = Vector3.zero;

        // Si se esta interactuando, no rotar
        if (playerManager.isInteracting && 
            !GetMovingIntoPosition() && 
            !GetRotatingIntoPosition())
        {
            return;
        }

        // Rotar hacia el target
        if (movingIntoPosition)
        {
            targetDir = interactionTarget.position - transform.position;
            targetDir.Normalize();
            targetDir.y = 0;
        }
        else if (rotatingIntoPosition)
        {
            targetDir = interactionTarget.forward;
            targetDir.Normalize();
            targetDir.y = 0;

            // Comprobar si ya se ha alineado con el forward del target
            Debug.Log("ANGLE TO TARGET FORWARD = " + Vector3.Angle(transform.forward, interactionTarget.forward));

            float angle = Vector3.Angle(transform.forward, interactionTarget.forward);

            if (angle < 10)
            {
                //interactionManager.PlayCurrentInteractionEvent();
                SetRotatingIntoPosition(false);
            }
        }

        // Si el jugador esta moviendo al personaje
        else
        {
            float moveOverride = inputHandler.moveAmount;

            targetDir = cameraObject.forward * inputHandler.vertical;
            targetDir += cameraObject.right * inputHandler.horizontal;

            targetDir.Normalize();
            targetDir.y = 0;

            if (targetDir == Vector3.zero)
                targetDir = myTransform.forward;
        }

        Quaternion tr = Quaternion.LookRotation(targetDir);
        Quaternion targetRotation = Quaternion.Slerp(myTransform.rotation, tr, rotationSpeed * delta);

        myTransform.rotation = targetRotation;
    }

    public void HandleMovement(float delta)
    {
        // Si se esta interactuando, no moverse
        if (playerManager.isInteracting && !movingIntoPosition)
        {
            rigidbody.velocity = Vector3.zero;
            animator.SetFloat("Vertical", Mathf.Lerp(animator.GetFloat("Vertical"), 0, Time.deltaTime * 8));
            return;
        }

        // Moverse hacia el target
        else if (GetMovingIntoPosition())
        {
            moveDirection = interactionTarget.position - transform.position;
            moveDirection.Normalize();
            moveDirection.y = 0;

            moveIIntensity = .5f;
            moveDirection *= movementSpeed * .5f;

            // Comprobar cuando se esta lo suficientemente cerca de la posicion de interaccion
            // para realizar la accion que sea
            float distanceToTarget = Vector3.Distance(interactionTarget.position, transform.position);
            if (distanceToTarget < .3f)
            {
                // Dejar de moverse
                SetMovingIntoPosition(false);
                // Empezar a rotar hacia el frente del target
                SetRotatingIntoPosition(true);
            }
        }

        // Moverse normal o quedarse quieto si esta en una interaccion
        else
        {
            moveDirection = cameraObject.forward * inputHandler.vertical;
            moveDirection += cameraObject.right * inputHandler.horizontal;
            moveDirection.Normalize(); // TODO:
            moveDirection.y = 0;

            float speed = movementSpeed;

            if (inputHandler.sprintFlag && inputHandler.moveAmount > .5f)
            {
                speed = sprintSpeed;
                moveDirection *= speed;
            }
            else
            {
                if (inputHandler.moveAmount < 0.5)
                    moveDirection *= (movementSpeed * inputHandler.moveAmount);
                else
                    moveDirection *= (speed * inputHandler.moveAmount);
            }

            if (playerManager.isInteracting)
                moveDirection = Vector3.zero;

            if (inputHandler.sprintFlag)
                moveIIntensity = 2;
            else
                moveIIntensity = inputHandler.moveAmount;
        }

        Vector3 projectedVelocity = Vector3.ProjectOnPlane(moveDirection, normalVector);
        rigidbody.velocity = projectedVelocity;

        animator.SetFloat("Vertical", Mathf.Lerp(animator.GetFloat("Vertical"), moveIIntensity, Time.deltaTime * 8));
    }

    #endregion


    public void SetTarget(Transform newTarget)
    { interactionTarget = newTarget; }

    public void SetMovingIntoPosition(bool newState)
    { movingIntoPosition = newState; }

    public bool GetMovingIntoPosition()
    { return movingIntoPosition; }

    public void SetRotatingIntoPosition(bool newState)
    { rotatingIntoPosition = newState; }

    public bool GetRotatingIntoPosition()
    { return rotatingIntoPosition; }


    public void PlayTargetAnimation(string targetAnim, bool isInteracting, bool canRotate = false)
    {
        animator.SetBool("isInteracting", isInteracting);
        animator.CrossFade(targetAnim, .2f);
    }

    public void FreezeRigidbody()
    {
        rigidbody.constraints = RigidbodyConstraints.FreezePosition;
    }
}
