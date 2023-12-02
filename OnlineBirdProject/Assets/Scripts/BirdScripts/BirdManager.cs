using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BirdManager : NetworkBehaviour
{
    [SerializeField]
    public bool DEBUG;


    GameplayManager gameplayManager;
    InputHandler inputHandler;

    PlayerMovement birdMovement;

    PlayerVisual playerVisual;

    TrailRenderer[] trailRenderers;

    [SerializeField]
    Transform cameraFollow;

    private void Awake()
    {
        // Asignaciones
        inputHandler = GetComponent<InputHandler>();
        birdMovement = GetComponent<PlayerMovement>();
        gameplayManager = GameplayManager.Instance;
        playerVisual = GetComponentInChildren<PlayerVisual>();
        trailRenderers = GetComponentsInChildren<TrailRenderer>();
    }

    private void Start()
    {
        // Asignar los datos del personaje cliente a esta instancia
        // Tanto como si lo estas controlando como si no

        // Una vez que se ha asignado la variable de clientId, cambiar el color del personaje
        PlayerData playerData = OnlineMultiplayerManager.Instance.GetPlayerDataFromClientId(OwnerClientId);
        playerVisual.SetPlayerMaterial(OnlineMultiplayerManager.Instance.GetPlayerMaterial(playerData.colorId));


        Material neonMaterial = OnlineMultiplayerManager.Instance.GetNeonMaterial(playerData.colorId);
        for (int i = 0; i < trailRenderers.Length; i++)
            trailRenderers[i].material = neonMaterial;
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

        float velocity = birdMovement.GetCurrentVelocity() / 44;

        if (velocity < .3f)
            velocity = 0;

        velocity -= .3f;
        velocity /= .7f;

        for (int i = 0; i < trailRenderers.Length; i++)
            trailRenderers[i].startWidth = velocity * maxTrailWidth;
    }

    float maxTrailWidth = 0.033f; //0.033f;


    public override void OnNetworkSpawn()
    {
        transform.position = gameplayManager.spawnPositionList
            [OnlineMultiplayerManager.Instance.GetPlayerDataIndexFromClientId(OwnerClientId)].position;

        //if (!IsOwnerBool())
        //{
        //    CameraFollow camFollow = GetComponentInChildren<CameraFollow>();
        //    Destroy(camFollow.gameObject);
        //}

        //CameraFollowTarget camFollow = GetComponentInChildren<CameraFollowTarget>();

        //if (IsOwnerBool())
        //{
        //    // Destruir el resto de CameraFollow
        //    CameraFollowTarget[] cameraFollows = FindObjectsOfType<CameraFollowTarget>();

        //    for (int i = 0; i < cameraFollows.Length; i++)
        //        if (cameraFollows[i] != null && cameraFollows[i].Target != gameObject)
        //            Destroy(cameraFollows[i].gameObject);
        //}
    }


    public bool IsOwnerBool()
    {
        return IsOwner || DEBUG;
    }

    //bool CanMove()
    //{
    //    return IsOwner && gameplayManager.GetState() == GameplayManager.State.GamePlaying;
    //}

}
