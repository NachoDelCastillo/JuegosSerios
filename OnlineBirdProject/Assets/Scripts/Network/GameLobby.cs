using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

// Se encarga de gestionar la creacion y union a Lobbies usando Relay
public class GameLobby : MonoBehaviour
{
    // Clave con la que se accede al codigo del lobby actual
    const string KEY_RELAY_JOIN_CODE = "RelayJoinCode";

    public static GameLobby Instance { get; private set; }

    // Eventos
    public event EventHandler OnCreateLobbyStarted;
    public event EventHandler OnCreateLobbyFailed;
    public event EventHandler OnJoinStarted;
    public event EventHandler OnQuickJoinFailed;
    public event EventHandler OnJoinFailed;
    public event EventHandler<OnLobbyListChangedEventArgs> OnLobbyListChanged;
    public class OnLobbyListChangedEventArgs : EventArgs
    { public List<Lobby> lobbyList; }

    // Lobby actual
    Lobby joinedLobby;
    // Mantener vivo el lobby (si en 30 segundos no hay actividad se cierra automaticamente)
    float heartBeatTimer;
    float heartBeatTimerMax = 15;

    // Cada cuanto tiempo se actualizan las lobbies
    float listLobbiesTimer;
    float listLobbiesTimerMax = 3;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeUnityAuthentication();
    }

    // Inscribirse en los servicios de Relay anonimamente para poder usarlos
    async void InitializeUnityAuthentication()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            // Crear un perfil falso para que el programa pueda diferenciar entre jugadores
            // creados desde un mismo ordenador ya que por defecto, inicializara el perfil
            // teniendo en cuenta el ordenador desde el que se llama, haciendo imposible el testing
            // en un mismo ordenador ya que se piensa que todos son el mismo jugador
            InitializationOptions options = new InitializationOptions();
            // options.SetProfile(UnityEngine.Random.Range(0, 10000).ToString());

            await UnityServices.InitializeAsync(options);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    private void Update()
    {
        // Mantener vivo el lobby
        HandleHeartBeat();

        // Actualizar los lobbies disponibles
        HandlePeriodicListLobbies();
    }

    // Mantener vivo el lobby
    void HandleHeartBeat()
    {
        if (IsLobbyHost())
        {
            heartBeatTimer -= Time.deltaTime;
            if (heartBeatTimer <= 0f)
            {
                heartBeatTimer = heartBeatTimerMax;

                LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
            }
        }
    }

    // Se encarga de llamar al metodo que recarga la informacion de las lobbies actuales cada X segundos
    // Se deja de llamar cuando el jugador se une a una lobby, ya que no es necesario
    void HandlePeriodicListLobbies()
    {
        if (joinedLobby == null
            && AuthenticationService.Instance.IsSignedIn
            && SceneManager.GetActiveScene().name == SceneLoader.SceneName.LobbyScene.ToString())
        {
            listLobbiesTimer -= Time.deltaTime;
            if (listLobbiesTimer <= 0f)
            {
                listLobbiesTimer = listLobbiesTimerMax;
                ListLobbies();
            }
        }
    }

    // Devuelve true si esta es la maquina que inicio el lobby
    bool IsLobbyHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    // Actualiza la variable de lobbies
    async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Filters = new List<QueryFilter>
            {
                new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
            }
            };
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();

            OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs
            {
                lobbyList = queryResponse.Results
            });

        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    // Una allocation es una reserva de un servidor de juego
    // Este metodo se encarga de reservar una para crear una lobby
    async Task<Allocation> AllocateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(OnlineMultiplayerManager.MAX_PLAYER_AMOUNT - 1);
            return allocation;
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            return default;
        }
    }

    // Devuelve el codigo del lobby Relay
    async Task<string> GetRelayJoinCode(Allocation allocation)
    {
        try
        {
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            return relayJoinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            return default;
        }
    }

    // Se une a un lobby Relay mediante un codigo de lobby Relay
    async Task<JoinAllocation> JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            return joinAllocation;
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            return default;
        }
    }

    // Crea una lobby en la que esta maquina es el host
    public async void CreateLobby(string lobbyName, bool isPrivate)
    {
        // Antes de intentar crear un lobby, como no es un proceso inmediato (await)
        // Llamar a este evento para avisar de que se acaba de empezar a solicitar el lobby
        // Para asi poner en pantalla un mensaje de carga correspondiente
        OnCreateLobbyStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            // No deja 1 jugador como jugadores maximos
            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, OnlineMultiplayerManager.MAX_PLAYER_AMOUNT, new CreateLobbyOptions
            {
                IsPrivate = isPrivate,
            });

            // Crear el allocation y asignarlo al NetworkManager para que este lo use
            Allocation allocation = await AllocateRelay();
            string relayJoinCode = await GetRelayJoinCode(allocation);

            // Guardar la clave de acceso del relay
            await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {KEY_RELAY_JOIN_CODE , new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
                }
            });
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));

            OnlineMultiplayerManager.Instance.StartHost();
            SceneLoader.LoadNetwork(SceneLoader.SceneName.CharacterSelectScene);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            // Si la creacion del lobby fracasa, llamar al evento correspondiente
            OnCreateLobbyFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    // Busca una partida publica, se une y inicializa un cliente
    public async void QuickJoin()
    {
        OnJoinStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            
            // Unirse usando la clave de relay creada
            string relayJoinCode = joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;
            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

            OnlineMultiplayerManager.Instance.StartClient();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            OnQuickJoinFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    // Se une a una lobby usando el Id
    public async void JoinWithId(string lobbyId)
    {
        OnJoinStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);

            // Unirse usando la clave de relay creada
            string relayJoinCode = joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;
            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

            OnlineMultiplayerManager.Instance.StartClient();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            OnJoinFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    // Se une a una lobby usando un codigo
    public async void JoinWithCode(string lobbyCode)
    {
        OnJoinStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);

            // Unirse usando la clave de relay creada
            string relayJoinCode = joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;
            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

            OnlineMultiplayerManager.Instance.StartClient();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            OnJoinFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    // Elimina una Lobby
    public async void DeleteLobby()
    {
        if (joinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
                joinedLobby = null;
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }

    // Elimina la referencia del lobby actual
    // Esto se hace en los clientes cuando el host cierra el lobby
    public void DeleteLobbyReference()
    { joinedLobby = null; }

    // Lo mismo que el metodo "DeleteLobbyReference" pero en todos los clientes
    [ClientRpc]
    public void EraseLobbyReference_ClientRpc()
    { joinedLobby = null; }

    // Notifica al lobby que esta maquina se desconecta
    public async void LeaveLobby()
    {
        if (joinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
                joinedLobby = null;
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }

    // Elimina a un jugador del lobby por su Id
    public async void KickPlayer(string playerId)
    {
        if (IsLobbyHost())
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }

    // Referencia a la lobby actual
    public Lobby GetLobby()
    { return joinedLobby; }
}
