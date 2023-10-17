using Mono.CSharp.yydebug;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Xml.Serialization;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OnlineMultiplayerManager : NetworkBehaviour
{
    static public OnlineMultiplayerManager Instance;

    public static int MAX_PLAYER_AMOUNT = 4;
    public const string PLAYERPREFS_PLAYERNAME_MULTIPLAYER = "PlayerNameMultiplayer";

    // Almacena todos los posibles materiales que se puede elegir los jugadores
    [SerializeField] List<Material> playerMaterials = new List<Material>();

    // Eventos
    public EventHandler OnTryingToJoinGame;
    public EventHandler OnFailedToJoinGame;
    public EventHandler OnPlayerDataNetworkListChanged;

    // Lista compartida por todos en la conexion el caul almacena los datos de los jugadores actuales
    // No se puede inicializar una NetworkList fuera de un metodo
    public NetworkList<PlayerData> playerDataNetworkList;
    // Nombre personal de cada jugador en su maquina
    string playerName;

    public static bool singlePlayerMode;

    private void Awake()
    {
        Instance = this;

        // Generar nombre por defecto si es la primera vez que se inicia el juego
        playerName = PlayerPrefs.GetString(PLAYERPREFS_PLAYERNAME_MULTIPLAYER, "CabezaOvo_" + UnityEngine.Random.Range(100, 1000));

        // Inicializar la lista y suscribirse a sus cambios
        playerDataNetworkList = new NetworkList<PlayerData>();
        playerDataNetworkList.OnListChanged += PlayerDataNetworkList_OnListChanged;

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Singleplayer
        // Inicia un host sin clientes el cual no necesita conexion para funcionar
        if (singlePlayerMode)
        {
            // Para que el modo de un singleplayer funcione sin conexion, es necesario asignar 
            // la variable UnityTransport.ProtocolType.UnityTransport a "Unity Transport"
            // Y para que el modo de multijugadore funcione, es necesario asignar 
            // la variable UnityTransport.ProtocolType.UnityTransport a "RelayUnity Transport"
            // Asi que por defecto se asignara como UnityTransport, ya que esta variable se cambiara sola
            // cuando se intente iniciar un servidor online con Relay
            StartHost();
            SceneLoader.LoadNetwork(SceneLoader.SceneName.GameScene);
        }
    }

    // Se llama cuando se añade o elimina un jugador de la lista de jugadores conectados
    private void PlayerDataNetworkList_OnListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        OnPlayerDataNetworkListChanged?.Invoke(this, EventArgs.Empty);
    }

    // Devuelve el nombre del jugador de esta maquina
    public string GetPlayerName()
    { return playerName; }
    // Cambia el nombre del jugador de esta maquina
    public void SetPlayerName(string _playerName)
    {
        playerName = _playerName;
        PlayerPrefs.SetString(PLAYERPREFS_PLAYERNAME_MULTIPLAYER, playerName);
    }

    // Inicia esta maquina como un host
    public void StartHost()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Server_OnClientDisConnectCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.StartHost();
    }

    // Se llama cuando se ha desconectado cualquier jugador (cliente o host)
    // Solo se puede llamar desde el host
    private void NetworkManager_Server_OnClientDisConnectCallback(ulong clientId)
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            PlayerData playerData = playerDataNetworkList[i];
            if (playerData.clientId == clientId)
            {
                // Borrar a ese cliente de la lista de jugadores del servidor
                playerDataNetworkList.RemoveAt(i);

                // No hace falta borrar el personaje que se ha desconectado, ya que 
            }
        }

        // Si un jugador se ha desconectado en la pantalla de Seleccion, avisar al server, para que compruebe de nuevo los readies
        // para ver si se empieza partida
        if (SceneManager.GetActiveScene().name == SceneLoader.SceneName.CharacterSelectScene.ToString())
          FindObjectOfType<CharacterSelect_Online>().CheckAllClientsReadyServerRpc();
    }

    // Se llama cuando se ha conectado cualquier jugador, incluyendo el propio host
    // Solo se puede llamar desde el host
    private void NetworkManager_OnClientConnectedCallback(ulong _clientId)
    {
        playerDataNetworkList.Add(new PlayerData
        {
            clientId = _clientId,
            colorId = GetFirstUnusedColorId(),
        });
        SetPlayerNameServerRpc(GetPlayerName());
        SetPlayerIdServerRpc(AuthenticationService.Instance.PlayerId);
    }
    
    // Se llama cuando se tiene que tomar la decision de aceptar o no a un nuevo jugador en la conexion
    // Si la variable Aprroved acaba en false, la conexion no se realizara por la razon especificada en Reason
    // Solo se puede llamar desde el host
    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest connectionApprovalRequest, NetworkManager.ConnectionApprovalResponse connectionApprovalResponse)
    {
        // Si los jugadores dentro del lobby ya no estan en la sala de seleccion, esque ya estan en partida
        if (SceneManager.GetActiveScene().name != SceneLoader.SceneName.CharacterSelectScene.ToString())
        {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game has already started";
            return;
        }

        // No sobrepasar el numero maximo de jugadores
        if (NetworkManager.Singleton.ConnectedClientsIds.Count >= MAX_PLAYER_AMOUNT)
        {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game is full [" + MAX_PLAYER_AMOUNT + "/" + MAX_PLAYER_AMOUNT + "]";
            return;
        }

        connectionApprovalResponse.Approved = true;
    }

    // Inicia esta maquina como un cliente
    public void StartClient()
    {
        OnTryingToJoinGame?.Invoke(this, EventArgs.Empty);

        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Client_OnClientDisconnectCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Client_OnClientConnectedCallback;

        NetworkManager.Singleton.StartClient();
    }

    // Se llama cuando esta maquina se ha conectado como cliente a un servidor
    // Se llama solo desde el cliente que ha generado el evento
    private void NetworkManager_Client_OnClientConnectedCallback(ulong clientId)
    {
        SetPlayerNameServerRpc(GetPlayerName());
        SetPlayerIdServerRpc(AuthenticationService.Instance.PlayerId);
    }

    // Actualiza el dato de nombre del jugador "playerName" en la lista de jugadores conectados
    [ServerRpc(RequireOwnership = false)]
    void SetPlayerNameServerRpc(string playerName, ServerRpcParams serverRpcParams = default)
    {
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);
        PlayerData playerData = playerDataNetworkList[playerDataIndex];
        playerData.playerName = playerName;
        playerDataNetworkList[playerDataIndex] = playerData;
    }

    // Actualiza el dato del Id en la lista de jugadores conectados
    [ServerRpc(RequireOwnership = false)]
    void SetPlayerIdServerRpc(string playerId, ServerRpcParams serverRpcParams = default)
    {
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);
        PlayerData playerData = playerDataNetworkList[playerDataIndex];
        playerData.playerId = playerId;
        playerDataNetworkList[playerDataIndex] = playerData;
    }

    // Se llama cuando esto es un cliente y este mismo se ha desconectado
    private void NetworkManager_Client_OnClientDisconnectCallback(ulong clientId)
    { OnFailedToJoinGame?.Invoke(this, EventArgs.Empty); }

    // Devuelve true si existe un jugador conectado con el index "playerIndex"
    // Sirve para saber si hay menos de X jugadores
    public bool IsPlayerIndexConnected(int playerIndex)
    { return playerIndex < playerDataNetworkList.Count; }

    // Devuelve el index de un jugador dado su Id
    public int GetPlayerDataIndexFromClientId(ulong clientId)
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].clientId == clientId)
                return i;
        }

        return -1;
    }

    // Devuelve los datos del jugador dado el Id
    public PlayerData GetPlayerDataFromClientId(ulong clientId)
    {
        foreach (PlayerData playerData in playerDataNetworkList)
        {
            if (playerData.clientId == clientId)
                return playerData;
        }

        return default;
    }

    // Devuelve los datos del jugador de esta maquina
    public PlayerData GetPlayerData()
    {
        return GetPlayerDataFromClientId(NetworkManager.Singleton.LocalClientId);
    }

    // Devuelve los datos del jugador dado el Index
    public PlayerData GetPlayerDataFromIndex(int playerIndex)
    { return playerDataNetworkList[playerIndex]; }

    // Devuelve un material dado su Id
    public Material GetPlayerMaterial(int colorId)
    {
        return playerMaterials[colorId];
    }

    // Notifica al servidor de que se cambia el color del jugador de esta maquina
    public void ChangePlayerMaterial(int colorId)
    { ChangePlayerMaterialServerRpc(colorId); }

    // Actualiza (si es posible) el material de un jugador
    [ServerRpc(RequireOwnership = false)]
    void ChangePlayerMaterialServerRpc(int colorId, ServerRpcParams serverRpcParams = default)
    {
        if (!IsMaterialAvailable(colorId))
        {
            // No esta disponible
            return;
        }

        else
        {
            // Esta disponible
            int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);
            PlayerData playerData = playerDataNetworkList[playerDataIndex];
            playerData.colorId = colorId;
            playerDataNetworkList[playerDataIndex] = playerData;
        }
    }

    // Saber si este material esta disponible
    bool IsMaterialAvailable(int colorId)
    {
        foreach (PlayerData playerData in playerDataNetworkList)
            if (playerData.colorId == colorId)
                return false;
        return true;
    }

    // Devuelve el Id del primer color disponible, empezando desde el 0
    private int GetFirstUnusedColorId()
    {
        for (int i = 0; i < playerMaterials.Count; i++)
            if (IsMaterialAvailable(i))
                return i;

        return -1;
    }

    // Echa al cliente de la is especificada, no hace falta encargarse de actualizar el array de jugadores y
    // la parte visual ya que esta hecha con eventos que se llaman automaticamente cuando este cliente se desconecta
    public void KickPlayer(ulong clientId)
    {
        NetworkManager.Singleton.DisconnectClient(clientId);
        // Hay que llamar a este metodo manualmente, ya que con la funcion DisconnectClient no se llama al evento correspondiente
        NetworkManager_Server_OnClientDisConnectCallback(clientId);
    }
}