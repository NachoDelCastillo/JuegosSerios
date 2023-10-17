using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    PlayerManager localInstance;

    GameplayManager gameplayManager;
    InputHandler inputHandler;

    Animator anim;
    CameraHandler cameraHandler;
    PlayerLocomotion playerLocomotion;
    PlayerVisual playerVisual;

    public bool isInteracting;

    private void Awake()
    {
        cameraHandler = FindObjectOfType<CameraHandler>();
        inputHandler = GetComponent<InputHandler>();
        anim = GetComponentInChildren<Animator>();
        playerLocomotion = GetComponent<PlayerLocomotion>();
        playerVisual = GetComponentInChildren<PlayerVisual>();
        gameplayManager = GameplayManager.Instance;
    }

    private void Start()
    {
        // Una vez que se ha asignado la variable de clientId, cambiar el color del personaje
        PlayerData playerData = OnlineMultiplayerManager.Instance.GetPlayerDataFromClientId(OwnerClientId);
        playerVisual.SetPlayerMaterial(OnlineMultiplayerManager.Instance.GetPlayerMaterial(playerData.colorId));

        // Suscribirse al evento que marca cuando se cambia de estado en el GameplayManager
        gameplayManager.OnStateChanged += StateChanged;
    }

    // Si se cambia al estado de GameOver, resetear los valores de movimiento
    // para que el personaje no se quede moviendose solo
    private void StateChanged(object sender, GameplayManager.State newState)
    {
        if (newState == GameplayManager.State.GameOver)
        {
            inputHandler.ResetMovementValues();
            playerLocomotion.FreezeRigidbody();
            anim.SetFloat("Vertical", 0);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
            localInstance = this;

        transform.position = gameplayManager.spawnPositionList
            [OnlineMultiplayerManager.Instance.GetPlayerDataIndexFromClientId(OwnerClientId)].position;

        if (IsServer)
            // Este metodo se llamara solo en el servidor y en el cliente que se ha desconectado
            // Pero limitamos aposta para que solo se llame en el servidor
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
    }

    // Se llama solo en el servidor, cuando se desconecta un cliente
    private void NetworkManager_OnClientDisconnectCallback(ulong clientID)
    {
        //if (clientID == OwnerClientId)
    }

    private void Update()
    {
        // Si no es el personaje al cual controla el jugador en esta maquina, no moverse
        if (!CanMove())
            return;

        isInteracting = anim.GetBool("isInteracting");

        float delta = Time.deltaTime;
        inputHandler.TickInput(delta);

        playerLocomotion.HandleMovement(delta);
        playerLocomotion.HandleRotation(delta);
    }

    private void FixedUpdate()
    {
        if (CanMove())
            return;

        float delta = Time.deltaTime;

        if (cameraHandler != null)
        {
            cameraHandler.FollowTarget(delta);
            cameraHandler.HandleCameraRotation(delta, inputHandler.mouseX, inputHandler.mouseY);
        }
    }

    private void LateUpdate()
    {
        if (CanMove())
            return;

        inputHandler.InteractFlag = false;
        inputHandler.sprintFlag = false;
    }

    bool CanMove()
    {
        return IsOwner && gameplayManager.GetState() == GameplayManager.State.GamePlaying;
    }


    public void SetInteraction(bool newState)
    { anim.SetBool("isInteracting", newState); }

    public bool GetInteraction()
    { return isInteracting; }
}
