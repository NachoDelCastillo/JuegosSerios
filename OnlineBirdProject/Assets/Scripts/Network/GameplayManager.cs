using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using static UnityEngine.CullingGroup;

public class GameplayManager : NetworkBehaviour
{
    float secondsToAnimation = 5;


    static public GameplayManager Instance;

    [SerializeField]
    private Transform playerPrefab;
    [SerializeField]
    private Transform foodManagerPrefab;

    private int numLevels = 3;
    private int currentLevel = 0;
    String[] ageStrings = { "1320", "2023", "2083" };

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

    private void Start()
    {
        if (DEBUG_ANIMATION)
            StartCoroutine(StartingLevelAnimation());
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

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Transform playerTransform = Instantiate(playerPrefab);
            clients.Add(playerTransform.gameObject);
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
           // playerTransform.gameObject.SetActive(false);
            playerTransform.gameObject.transform.GetChild(2).tag = "CameraFollow";
        }

        createFoodClientRpc();

        initializeClientsClientRpc();
    }

    [ClientRpc]
    void initializeClientsClientRpc()
    {
        Invoke("DeactivateMainCamera", .1f);
    }

    void DeactivateMainCamera()
    {
        birdCamera = FindObjectOfType<CameraFollowTarget>();
        birdCamera.gameObject.SetActive(false);

        waitingForOtherPlayersText.transform.parent.gameObject.SetActive(true);
    }

    //[ServerRpc]
    //void StartAnimationServerRpc()
    //{
    //    StartCoroutine(StartingLevelAnimation());
    //    StartAnimationClientRpc();
    //}
    [ClientRpc]
    void StartAnimationClientRpc()
    {
        StartCoroutine(StartingLevelAnimation());
    }


    // ANIMATION MANAGER
    bool DEBUG_ANIMATION = false;
    bool animationPlayed = false;

    [SerializeField]
    TMP_Text ageText;

    [SerializeField]
    TMP_Text waitingForOtherPlayersText;


    [SerializeField]
    CinemachineVirtualCamera[] cameras;

    IEnumerator StartingLevelAnimation()
    {
        Debug.Log("START LEVEL ANIMATION");

        waitingForOtherPlayersText.transform.parent.gameObject.SetActive(false);

        for (int i = 0; i < cameras.Length; i++)
            cameras[i].gameObject.SetActive(false);

        cameras[0].gameObject.SetActive(true);

        yield return new WaitForSeconds(1);

        cameras[0].gameObject.SetActive(false);
        cameras[1].gameObject.SetActive(true);

        yield return new WaitForSeconds(1);

        // Renderizar Texto
        StartCoroutine(RenderText());

        yield return new WaitForSeconds(2);

        birdCamera.gameObject.SetActive(true);

        Debug.Log("END LEVEL ANIMATION");
    }

    IEnumerator RenderText()
    {
        int typographicTime = 2;

        String s = ageStrings[currentLevel];
        float timeperLetter = (float)typographicTime / (float)s.Length;
        String currentText = "";
        foreach (char letter in s)
        {
            currentText += letter;
            ageText.text = currentText;
            yield return new WaitForSeconds(timeperLetter);
        }
    }

    CameraFollowTarget birdCamera;




    [ClientRpc]
    void createFoodClientRpc()
    {
        //Debug.Log("En el server se ejecuta");
        Transform p = Instantiate(foodManagerPrefab);
        p.GetComponent<NetworkObject>().Spawn();
    }

    // TimeLine acabada
    //void OnTimelineFinished(PlayableDirector director)
    //{
    //    GameObject[] clientsArray = clients.ToArray();

    //    foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
    //    {
    //        clientsArray[clientId].SetActive(true);
    //    }

    //    GameObject.FindGameObjectWithTag("CameraFollow").SetActive(true);
    //}

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

        int timerNumber = 5 - Mathf.RoundToInt(maxGameplayTimer - gameplay_Timer.Value);// - secondsToAnimation);
        waitingForOtherPlayersText.text = 
            "Waiting for other Players (" + timerNumber + ")";


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

                Debug.Log("gameplay_Timer.Value = " + gameplay_Timer.Value);

                if (!animationPlayed && gameplay_Timer.Value < maxGameplayTimer - secondsToAnimation)
                {
                    animationPlayed = true;

                    if (DEBUG_ANIMATION)
                        StartCoroutine(StartingLevelAnimation());
                    else
                        // El servidor activa la animacion, la cual tambien avisa al resto de maquinas para que empiecen 
                        // la misma animacion en su propia maquina local
                        StartAnimationClientRpc();
                }

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