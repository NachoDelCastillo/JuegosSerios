using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using static UnityEngine.CullingGroup;

public class GameplayManager : NetworkBehaviour
{
    static public GameplayManager Instance;

    [SerializeField]
    private Transform playerPrefab;
    [SerializeField]
    private Transform foodManagerPrefab;

    private int numLevels = 3;
    private int currentLevel = 1;

    public GameObject deforestacion;
    public PlayableDirector director;

    public enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver
    }
    NetworkVariable<State> state = new NetworkVariable<State>(State.WaitingToStart);

    [SerializeField]
    public List<Transform> spawnPositionList;

    //bool isLocalPlayerReady;

    public EventHandler OnLocalReadyPlayerChanged;
    public EventHandler<State> OnStateChanged;


    // Tiempo antes de que empiece el countdown
    NetworkVariable<float> waitingToStart_Timer = new NetworkVariable<float>(.5f);
    // Cuenta atras que se puede ver en pantalla antes de empezar la partida
    NetworkVariable<float> countDownToStart_Timer = new NetworkVariable<float>(3f);
    // Tiempo que dura la partida
    NetworkVariable<float> gameplay_Timer = new NetworkVariable<float>(maxGameplayTimer);
    public const float maxGameplayTimer = 300;
    // Tiempo desde que termina la partida hasta que se vuelve al lobby, mostrando resultados de la partida
    NetworkVariable<float> gameover_Timer = new NetworkVariable<float>(maxGameoverTimer);
    public const float maxGameoverTimer = 3;

    private List<GameObject> clients = new List<GameObject>();


    private void Awake()
    {
        Instance = this;
    }

    // Estado actual
    public State GetState()
    { return state.Value; }

    // Cuando se spawnean los objetos Network
    public override void OnNetworkSpawn()
    {
        state.OnValueChanged += State_OnValueChanged;

        if (IsServer)
        {
            // Este evento se llama cuando todos los clientes han cargado una escena nueva
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }

    // Cuando se termina de cargar la escena para todos los clientes y host
    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        // Si no es la escena de juego, no instanciar ningun jugador
        if (SceneManager.GetActiveScene().name != SceneLoader.SceneName.Gameplay.ToString())
            return;


        director.stopped += OnTimelineFinished;

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Transform playerTransform = Instantiate(playerPrefab);
            clients.Add(playerTransform.gameObject);
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
           // playerTransform.gameObject.SetActive(false);
            playerTransform.gameObject.transform.GetChild(2).tag = "CameraFollow";
        }

        createFoodClientRpc();
    }

    [ClientRpc]
    void createFoodClientRpc()
    {
        Debug.Log("En el server se ejecuta");
        Transform p = Instantiate(foodManagerPrefab);
        p.GetComponent<NetworkObject>().Spawn();
    }

    // TimeLine acabada
    void OnTimelineFinished(PlayableDirector director)
    {
        GameObject[] clientsArray = clients.ToArray();

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            clientsArray[clientId].SetActive(true);
        }

        GameObject.FindGameObjectWithTag("CameraFollow").SetActive(true);
    }

    // Se llama cada vez que cambia el estado
    private void State_OnValueChanged(State previousValue, State newValue)
    {
        if (OnStateChanged != null)
            OnStateChanged.Invoke(this, newValue);
    }

    // Timers
    public float GetCountDownToStartTimer()
    { return countDownToStart_Timer.Value; }

    public float GetGameplayTimer()
    { return gameplay_Timer.Value; }

    public float GetGameOverTimer()
    { return gameover_Timer.Value; }


    private void Update()
    {
        // Codigo provisional
        // Si es tanto el cliente como el host, poder volver al menu desconectandose de la partida actual
        if (Input.GetKeyDown(KeyCode.M))
        {
            // Desconectarse
            NetworkManager.Singleton.Shutdown();
            // Cambiar solo en esta maquina la escena a la del menu
            SceneLoader.Load(SceneLoader.SceneName.MainMenuScene);
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            changeLevel();
        }

        // Transicion de estados por tiempo
        if (!IsServer)
            return;

        switch (state.Value)
        {
            case State.WaitingToStart:
                waitingToStart_Timer.Value -= Time.deltaTime;
                if (waitingToStart_Timer.Value < 0)
                {
                    state.Value = State.CountdownToStart;
                }
                break;

            case State.CountdownToStart:
                countDownToStart_Timer.Value -= Time.deltaTime;
                if (countDownToStart_Timer.Value < 0)
                {
                    state.Value = State.GamePlaying;
                }
                break;

            case State.GamePlaying:
                gameplay_Timer.Value -= Time.deltaTime;
                if (gameplay_Timer.Value < 0)
                {
                    state.Value = State.GameOver;
                }
                break;

            case State.GameOver:
                gameover_Timer.Value -= Time.deltaTime;
                if (gameover_Timer.Value < 0)
                {
                    SceneLoader.LoadNetwork(SceneLoader.SceneName.CharacterSelectScene);
                }
                break;
        }
    }

    private void changeLevel()
    {

        if (currentLevel < numLevels)
        {
            currentLevel++;
            deforestacion.SetActive(true);
            GameObject.FindGameObjectWithTag("CameraFollow").SetActive(false);
            GameObject[] clientsArray = clients.ToArray();
            foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                clientsArray[clientId].transform.position = playerPrefab.transform.position;
                clientsArray[clientId].transform.rotation = playerPrefab.transform.rotation;
                clientsArray[clientId].SetActive(false);
            }
            director.time = 0f;
            director.Play();
        }
    }

    // Conocer el estado actual desde otros Scripts
    public bool IsCountdownActive()
    { return state.Value == State.CountdownToStart; }

    public bool IsWaitingToStart()
    { return state.Value == State.WaitingToStart; }
}