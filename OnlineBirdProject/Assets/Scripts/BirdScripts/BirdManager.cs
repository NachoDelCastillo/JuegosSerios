using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BirdManager : NetworkBehaviour
{
    [SerializeField]
    bool DEBUG;


    GameplayManager gameplayManager;
    InputHandler inputHandler;

    PlayerMovement birdMovement;

    private void Awake()
    {
        // Asignaciones
        inputHandler = GetComponent<InputHandler>();
        birdMovement = GetComponent<PlayerMovement>();
        gameplayManager = GameplayManager.Instance;
    }

    private void Start()
    {
        // Asignar los datos del personaje cliente a esta instancia
        // Tanto como si lo estas controlando como si no
    }

    void Update()
    {
        // if (CanMove())
        if (IsOwnerBool())
        {
            float delta = Time.deltaTime;
            inputHandler.TickInput(delta);
        }
        else
        {
            // Resetear inputs
        }

        if (IsOwnerBool())

            // Handle Movement
            birdMovement.MovementUpdate();
    }

    private void FixedUpdate()
    {
        if (IsOwnerBool())

            // Handle Movement
            birdMovement.MovementFixedUpdate();
    }



    public override void OnNetworkSpawn()
    {
        transform.position = gameplayManager.spawnPositionList
            [OnlineMultiplayerManager.Instance.GetPlayerDataIndexFromClientId(OwnerClientId)].position;

        if (!IsOwnerBool())
        {
            CameraFollow camFollow = GetComponentInChildren<CameraFollow>();
            Destroy(camFollow.gameObject);
        }
    }


    bool IsOwnerBool()
    {
        return IsOwner || DEBUG;
    }

    //bool CanMove()
    //{
    //    return IsOwner && gameplayManager.GetState() == GameplayManager.State.GamePlaying;
    //}

}
