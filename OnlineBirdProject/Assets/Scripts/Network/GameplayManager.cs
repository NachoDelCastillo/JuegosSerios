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
    public bool birdsCanMove = false;

    public List<BirdManager> allBirds;

    float secondsToAnimation = 5;


    [SerializeField]
    private GameObject healthBar;

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
    public const float maxGameplayTimer = 75; // 20 tiempo optimo
    // Tiempo desde que termina la partida hasta que se vuelve al lobby, mostrando resultados de la partida
    NetworkVariable<float> gameover_Timer = new NetworkVariable<float>(maxGameoverTimer);
    public const float maxGameoverTimer = 5;

    private List<GameObject> clients = new List<GameObject>();

    NetworkVariable<int> birdsAlive = new NetworkVariable<int>(0);

    public BirdManager localBird;

    private void Awake()
    {
        Instance = this;
        allBirds = new List<BirdManager>();
    }

    private void Start()
    {
        if (DEBUG_ANIMATION)
            StartCoroutine(StartingLevelAnimation());

        birdHasDied.DOFade(0, 0.01f);
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
            //playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
            playerTransform.GetComponent<NetworkObject>().SpawnWithOwnership(clientId, true);
            // playerTransform.gameObject.SetActive(false);
            playerTransform.gameObject.transform.GetChild(2).tag = "CameraFollow";

            birdsAlive.Value++;
        }

        //createFoodClientRpc();

        initializeClientsClientRpc();
    }

    [ClientRpc]
    void initializeClientsClientRpc()
    {
        currentLevel = 1;
        Invoke("SetupValues", .1f);

        //healthBar.gameObject.SetActive(false);
    }

    BirdManager localBirdManager;

    [ClientRpc]
    void SetStaticFalseClientRpc()
    {
        localBirdManager.GetComponent<PlayerMovement>().SetStatic(false);
    }

    [ClientRpc]
    void BirdsCanMoveClientRpc(bool newValue)
    {
        birdsCanMove = newValue;
    }

    [ClientRpc]
    void BirdsToSpawnPointClientRpc()
    {
        //if (localBirdManager != null)
        //{
        //    Vector3 spawnPoint = spawnPositionList
        //        [OnlineMultiplayerManager.Instance.GetPlayerDataIndexFromClientId(localBirdManager.OwnerClientId)].position;
        //    localBirdManager.GetComponent<PlayerMovement>().TeleportThis(spawnPoint);
        //    localBirdManager.GetComponent<PlayerMovement>().SetStatic(true);
        //}

        //else
        {
            for (int i = 0; i < allBirds.Count; i++)
            {
                BirdManager bm = allBirds[i];
                if (bm != null)
                    if (bm.IsOwner)
                    {
                        Vector3 spawnPoint = spawnPositionList
                            [OnlineMultiplayerManager.Instance.GetPlayerDataIndexFromClientId(bm.OwnerClientId)].position;

                        localBirdManager = bm;

                        localBirdManager.GetComponent<PlayerMovement>().TeleportThis(spawnPoint);
                        //localBirdManager.GetComponent<PlayerMovement>().SetStatic(true);
                    }
            }
        }
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

            if (IsServer)
                BirdsCanMoveClientRpc(false);
        }

        showLevelTimerText = false;

        StartCoroutine(EndingLevelAnimation(newLevel));


        BirdManager[] allBirds = FindObjectsByType<BirdManager>(FindObjectsSortMode.None);
        for (int i = 0; i < allBirds.Length; i++)
            if (allBirds[i].IsOwner)
                localBird = allBirds[i];
        localBird.GetComponent<LifeBar>().eatFood(9999);
    }

    IEnumerator EndingLevelAnimation(int newLevel)
    {
        healthBar.gameObject.SetActive(false);

        FoodManager.Instance.ResetPaths();

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
        healthBar.gameObject.SetActive(false);


        BirdManager[] allBirds = FindObjectsByType<BirdManager>(FindObjectsSortMode.None);
        for (int i = 0; i < allBirds.Length; i++)
            if (allBirds[i].IsOwner)
                localBird = allBirds[i];
        localBird.GetComponent<LifeBar>().eatFood(9999);

        if (IsServer)
        {
            gameplay_Timer.Value = maxGameplayTimer;
            BirdsToSpawnPointClientRpc();
            BirdsCanMoveClientRpc(false);
        }

        if (newLevel == 1)
            EnvironmentChanger.Instance.SetFirstLevel();
        else if (newLevel == 2)
            EnvironmentChanger.Instance.SetSecondLevel();
        else if (newLevel == 3)
            EnvironmentChanger.Instance.SetThirdLevel();

        //EnvironmentChanger.Instance.SetFirstLevel();


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

        showLevelTimerText = false;

        levelTimerText.text = "";

        waitingForOtherPlayersText.transform.parent.gameObject.SetActive(false);

        for (int i = 0; i < cameras.Length; i++)
            cameras[i].gameObject.SetActive(false);



        //////////////////////////////////////////////////


        cameras[0].gameObject.SetActive(true);

        yield return new WaitForSeconds(1);

        cameras[0].gameObject.SetActive(false);
        cameras[1].gameObject.SetActive(true);

        yield return new WaitForSeconds(5);

        ageText.text = ageStrings[currentLevel - 1];
        ageText.DOFade(1, 1);

        birdsLeftText.text = " " + birdsAlive.Value + " birds left ";
        birdsLeftText.DOFade(1, 1);
        // Renderizar Texto
        //StartCoroutine(RenderText());

        yield return new WaitForSeconds(4);

        ageText.DOFade(0, 1);
        birdsLeftText.DOFade(0, 1);

        yield return new WaitForSeconds(3);

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

        if (currentLevel != 3)
            showLevelTimerText = true;


        //yield return new WaitForSeconds(1);
        //birdCamera.gameObject.SetActive(true);

        //////////////////////////////////////////////////




        // Devolver controles al pajaroOwner

        for (int i = 0; i < cameras.Length; i++)
            cameras[i].gameObject.SetActive(false);

        if (IsServer)
            BirdsCanMoveClientRpc(true);


        if (currentLevel == 1)
            FoodManager.Instance.FirstLevelFood();
        else if (currentLevel == 2)
            FoodManager.Instance.SecondLevelFood();
        else if (currentLevel == 3)
            FoodManager.Instance.ThirdLevelFood();

        healthBar.gameObject.SetActive(true);

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




    //[ClientRpc]
    //void createFoodClientRpc()
    //{
    //    //Debug.Log("En el server se ejecuta");
    //    Transform p = Instantiate(foodManagerPrefab);
    //    p.GetComponent<NetworkObject>().Spawn();
    //}

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
    void EndGameClientRpc()
    {
        Debug.Log("END GAME");

        //if (IsServer)
        //    gameplay_Timer.Value = maxGameplayTimer;

        //if (newLevel == 1)
        //    EnvironmentChanger.Instance.SetFirstLevel();
        //else if (newLevel == 2)
        //    EnvironmentChanger.Instance.SetSecondLevel();
        //else if (newLevel == 3)
        //    EnvironmentChanger.Instance.SetThirdLevel();


        for (int i = 0; i < cameras.Length; i++)
            cameras[i].gameObject.SetActive(false);

        birdCamera.gameObject.SetActive(false);
        //currentLevel = newLevel;
        StartCoroutine(EndGameAnimation());
    }

    IEnumerator EndGameAnimation()
    {
        Debug.Log("END GAME ENUMERATOR");

        ageText.text = "Nobody wins";
        ageText.DOFade(1, 1);

        yield return new WaitForSeconds(1);

        cameras[0].gameObject.SetActive(true);

        yield return new WaitForSeconds(1);

        cameras[0].gameObject.SetActive(false);
        cameras[1].gameObject.SetActive(true);

        yield return new WaitForSeconds(1);

        birdsLeftText.text = "Returning to Lobby";
        birdsLeftText.DOFade(1, 1);

        yield return new WaitForSeconds(1);

        ageText.DOFade(0, 1);
        birdsLeftText.DOFade(0, 1);

        yield return new WaitForSeconds(1.5f);

        SceneLoader.LoadNetwork(SceneLoader.SceneName.CharacterSelectScene);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
            SceneManager.LoadScene(0);

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

        //Debug.Log("birdsAlive.Value = " + birdsAlive.Value);

        int timerNumber = 5 - Mathf.RoundToInt(maxGameplayTimer - gameplay_Timer.Value);// - secondsToAnimation);
        waitingForOtherPlayersText.text =
            "Waiting for other Players (" + timerNumber + ")";

        if (Input.GetKeyDown(KeyCode.Z))
            BirdsToSpawnPointClientRpc();

        //if (Input.GetKeyDown(KeyCode.C))
        //    SetStaticFalseClientRpc();

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
                        StartLevelClientRpc(1); // 1
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

                // Si estas en el ultimo nivel, esperar hasta que todos mueran
                if (!gameEnded && currentLevel == 3)
                {
                    //// Si todos los pajaros estan muertos
                    //if (allBirds.Count == 0)
                    //{
                    //    // Terminar la partida
                    //    EndGameClientRpc();
                    //    gameEnded = true;
                    //    //state.Value = State.GameOver;
                    //}

                    if (birdsAlive.Value <= 0)
                    {
                        // Terminar la partida
                        EndGameClientRpc();
                        gameEnded = true;
                    }
                }
                break;

            case State.GameOver:
                gameover_Timer.Value -= Time.deltaTime;

                //Debug.Log("GAMEOVER");
                //if (gameover_Timer.Value < 0)
                //{
                //    SceneLoader.LoadNetwork(SceneLoader.SceneName.CharacterSelectScene);
                //}
                break;
        }
    }

    bool gameEnded = false;

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

        if (birdManager.IsOwner)
            localBird = birdManager;
    }

    public void birdDestroyed(BirdManager birdManager)
    {
        int deletedIndex = 0;

        for (int i = 0; i < allBirds.Count; i++)
        {
            if (allBirds[i] == birdManager)
                deletedIndex = i;
        }

        BirdDestroyedServerRpc(deletedIndex);
        //BirdDestroyedClientRpc(deletedIndex);

        // Quitarlo de la lista local
        //allBirds.Remove(birdManager);
    }

    [ServerRpc(RequireOwnership = false)]
    void BirdDestroyedServerRpc(int index)
    {
        GameObject g = allBirds[index].gameObject;

        if (g == null)
            return;

        Debug.Log("BirdDestroyedClientRpc = " + index + " , obj = " + g);
        Destroy(g);

        int value = birdsAlive.Value;
        value--;
        birdsAlive.Value = value;

        // Destroy(allBirds[index].gameObject.GetComponent<NetworkBehaviour>().d);

        //allBirds.RemoveAt(index);

        //if (allBirds.Count == 1)
        //    if (allBirds[0].IsOwner)
        //        YouAreLastBird();

        BirdDestroyedUI();
    }

    [ClientRpc]
    void BirdDestroyedClientRpc(int index)
    {
        Debug.Log("BirdDestroyedClientRpc = " + index);

        Destroy(allBirds[index].gameObject);

        allBirds.RemoveAt(index);

        //if (allBirds.Count == 1)
        //    if (allBirds[0].IsOwner)
        //        YouAreLastBird();

        BirdManager[] birdManager = FindObjectsByType<BirdManager>(FindObjectsSortMode.None);
        bool thisBirdAlive = false;
        for (int i = 0; i < birdManager.Length; i++)
        {
            if (birdManager[i].IsOwner == true)
            {
                thisBirdAlive = true;
                break;
            }
        }

        if (thisBirdAlive && birdsAlive.Value == 1)
            YouAreLastBird();

        BirdDestroyedUI();
    }

    void BirdDestroyedUI()
    {
        int value = birdsAlive.Value;
        if (value < 0)
            value = 0;

        birdHasDied.text = "Only " + value + " birds alive";
        birdHasDied.DOFade(1, 0.1f);

        Invoke("BirdDestroyedUIDissappear", 1);
    }

    void BirdDestroyedUIDissappear()
    {
        birdHasDied.DOFade(0, 1);
    }
}