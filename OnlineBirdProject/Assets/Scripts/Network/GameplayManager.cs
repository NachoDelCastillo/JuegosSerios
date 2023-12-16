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
using DG.Tweening;

public class GameplayManager : NetworkBehaviour
{
    List<BirdManager> allBirds;

    float secondsToAnimation = 5;

    [SerializeField]
    private TMP_Text birdHasDied;

    [SerializeField]
    private TMP_Text countDownText;

    [SerializeField]
    private TMP_Text birdsLeftText;

    [SerializeField]
    private TMP_Text levelTimerText;
    bool showLevelTimerText = false;

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
    public const float maxGameplayTimer = 15;
    // Tiempo desde que termina la partida hasta que se vuelve al lobby, mostrando resultados de la partida
    NetworkVariable<float> gameover_Timer = new NetworkVariable<float>(maxGameoverTimer);
    public const float maxGameoverTimer = 5;

    private List<GameObject> clients = new List<GameObject>();


    private void Awake()
    {
        Instance = this;
        allBirds = new List<BirdManager>();
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
        currentLevel = 1;
        Invoke("SetupValues", .1f);
    }

    void SetupValues()
    {
        birdCamera = FindObjectOfType<CameraFollowTarget>();
        birdCamera.gameObject.SetActive(false);
        waitingForOtherPlayersText.transform.parent.gameObject.SetActive(true);
    }


    float endingAnimationTime = 5;

    [ClientRpc]
    void EndLevelClientRpc(int newLevel)
    {
        if (IsServer)
        {
            if (newLevel == 3)
                gameplay_Timer.Value = maxGameplayTimer + endingAnimationTime + 1000;
            else
                gameplay_Timer.Value = maxGameplayTimer + endingAnimationTime;
        }

        showLevelTimerText = false;

        StartCoroutine(EndingLevelAnimation(newLevel));
    }

    IEnumerator EndingLevelAnimation(int newLevel)
    {
        BirdManager[] allPlayerManagers = FindObjectsByType<BirdManager>(FindObjectsSortMode.None);

        bool playerStillAlive = false;

        for (int i = 0; i < allPlayerManagers.Length; i++)
            if (allPlayerManagers[i].IsOwner)
            {
                playerStillAlive = true;
                break;
            }

        ageText.DOFade(1, 0.01f);
        if (playerStillAlive)
            ageText.text = "YOU SURVIVED";
        else
            ageText.text = "YOU ARE DEAD";

        ageText.DOFade(1, 1);

        yield return new WaitForSeconds(endingAnimationTime / 4);

        ageText.DOFade(0, 1);

        yield return new WaitForSeconds(endingAnimationTime / 4);

        StartLevelClientRpc(newLevel);
    }

    [ClientRpc]
    void StartLevelClientRpc(int newLevel)
    {
        if (IsServer)
            gameplay_Timer.Value = maxGameplayTimer;

        if (newLevel == 1)
            EnvironmentChanger.Instance.SetFirstLevel();
        else if (newLevel == 2)
            EnvironmentChanger.Instance.SetSecondLevel();
        else if (newLevel == 3)
            EnvironmentChanger.Instance.SetThirdLevel();


        birdCamera.gameObject.SetActive(false);
        currentLevel = newLevel;
        StartCoroutine(StartingLevelAnimation());
    }


    public void YouAreLastBird()
    {
        StartCoroutine(YouAreLastBirdEnumerator());
    }

    IEnumerator YouAreLastBirdEnumerator()
    {
        ageText.text = "You are the last survivor";
        ageText.DOFade(1, 1);

        yield return new WaitForSeconds(1);

        ageText.DOFade(0, 1);
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

        ageText.DOFade(0, .001f);
        birdsLeftText.DOFade(0, .001f);
        countDownText.text = "";

        if (currentLevel != 3)
            showLevelTimerText = false;
        levelTimerText.text = "";

        waitingForOtherPlayersText.transform.parent.gameObject.SetActive(false);

        for (int i = 0; i < cameras.Length; i++)
            cameras[i].gameObject.SetActive(false);

        cameras[0].gameObject.SetActive(true);

        yield return new WaitForSeconds(1);

        cameras[0].gameObject.SetActive(false);
        cameras[1].gameObject.SetActive(true);

        yield return new WaitForSeconds(1);

        ageText.text = ageStrings[currentLevel - 1];
        ageText.DOFade(1, 1);

        birdsLeftText.text = " " + allBirds.Count + " birds left ";
        birdsLeftText.DOFade(1, 1);
        // Renderizar Texto
        //StartCoroutine(RenderText());

        yield return new WaitForSeconds(1);

        ageText.DOFade(0, 1);
        birdsLeftText.DOFade(0, 1);

        yield return new WaitForSeconds(1);

        birdCamera.gameObject.SetActive(true);

        countDownText.DOFade(1, .5f);
        countDownText.text = "3";
        yield return new WaitForSeconds(1);
        countDownText.text = "2";
        yield return new WaitForSeconds(1);
        countDownText.text = "1";
        yield return new WaitForSeconds(1);
        countDownText.text = "SURVIVE";
        countDownText.DOFade(0, 1);

        showLevelTimerText = true;

        // Devolver controles al pajaroOwner

        for (int i = 0; i < cameras.Length; i++)
            cameras[i].gameObject.SetActive(false);

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


    [ClientRpc]
    void EndGameClientRpc(int newLevel)
    {
        if (IsServer)
            gameplay_Timer.Value = maxGameplayTimer;

        if (newLevel == 1)
            EnvironmentChanger.Instance.SetFirstLevel();
        else if (newLevel == 2)
            EnvironmentChanger.Instance.SetSecondLevel();
        else if (newLevel == 3)
            EnvironmentChanger.Instance.SetThirdLevel();


        birdCamera.gameObject.SetActive(false);
        currentLevel = newLevel;
        StartCoroutine(StartingLevelAnimation());
    }


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

        if (!showLevelTimerText)
            levelTimerText.text = "";
        else
            levelTimerText.text = Mathf.CeilToInt(gameplay_Timer.Value).ToString();

        // Transicion de estados por tiempo
        if (!IsServer)
            return;

        int timerNumber = 5 - Mathf.RoundToInt(maxGameplayTimer - gameplay_Timer.Value);// - secondsToAnimation);
        waitingForOtherPlayersText.text =
            "Waiting for other Players (" + timerNumber + ")";


        // Si se esta en el tercer nivel, comprobar cuando se mueren todos los pajaros
        if (currentLevel == 3)
        {
            // Si el numero de pajaros vivos llega a 0
            if (allBirds.Count == 0)
            {
                // Animacion de final de juego en la que nadie sobrevive

            }
        }

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

                //Debug.Log("gameplay_Timer.Value = " + gameplay_Timer.Value);

                if (!animationPlayed && gameplay_Timer.Value < maxGameplayTimer - secondsToAnimation)
                {
                    animationPlayed = true;

                    if (DEBUG_ANIMATION)
                        StartCoroutine(StartingLevelAnimation());
                    else
                        // El servidor activa la animacion, la cual tambien avisa al resto de maquinas para que empiecen 
                        // la misma animacion en su propia maquina local
                        //StartAnimationClientRpc();
                        StartLevelClientRpc(1);
                }

                if (gameplay_Timer.Value < 0)
                {
                    if (currentLevel == 1 || currentLevel == 2)
                    {
                        currentLevel++;
                        EndLevelClientRpc(currentLevel);
                    }
                    else
                    {
                        // Esto no deberia pasar nunca, todos los pajaros mueren antes de que llegue aqui
                    }
                }
                //state.Value = State.GameOver;

                break;

            case State.GameOver:
                gameover_Timer.Value -= Time.deltaTime;

                Debug.Log("GAMEOVER");
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


    public void addBird(BirdManager birdManager)
    {
        allBirds.Add(birdManager);
    }

    public void birdDestroyed(BirdManager birdManager)
    {
        allBirds.Remove(birdManager);

        Debug.Log("BIRD ReMOVED");

        if (allBirds.Count == 1)
        {
            if (allBirds[0].IsOwner)
                YouAreLastBird();
        }

        BirdDestroyedUI();
    }

    void BirdDestroyedUI()
    {
        birdHasDied.text = "A bird has died \n " + allBirds.Count + " birds alive";
        birdHasDied.DOFade(1, 0.1f);

        Invoke("BirdDestroyedUIDissappear", 1);
    }

    void BirdDestroyedUIDissappear()
    {
        birdHasDied.DOFade(0, 1);
    }
}