using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

// Se encarga de gestionar las acciones de Ready de los jugadores
public class CharacterSelect_Online : NetworkBehaviour // CharacterSelectReady
{
    public static CharacterSelect_Online Instance;

    // Almacena que jugadores tienen el ready activado y quienes no
    Dictionary<ulong, bool> playerReadyDictionary;

    // Se llama cada vez que cualquier jugador activa o desactiva el modo ready
    public event EventHandler OnReadyChanged;

    private void Awake()
    {
        if (GameManager.IsLocal())
        {
            Destroy(GetComponent<NetworkObject>());
            Destroy(this);
        }

        Instance = this;

        playerReadyDictionary = new Dictionary<ulong, bool>();
    }

    // Se llama tanto desde un cliente como de un host
    // Notifica al servidor de que el jugador de esta maquina esta presiono Ready
    public void SetPlayerReady()
    { SetPlayerReady_ServerRpc(); }

    // Actualiza la variable playerReadyDictionary con el jugador que ha presiona Ready
    [ServerRpc(RequireOwnership = false)]
    void SetPlayerReady_ServerRpc(ServerRpcParams serverRpcParams = default)
    {
        bool playerReadyNow;
        // Si todavia no se ha registrado este jugador en el servidor, crearlo y ponerlo a true
        if (!playerReadyDictionary.ContainsKey(serverRpcParams.Receive.SenderClientId))
        {
            playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;
            playerReadyNow = true;
        }
        // En el caso en el que ya se haya registrado anteriormente
        else
        {
            // Saber que valor tiene antes del cambio
            bool playerReadyBefore = playerReadyDictionary[serverRpcParams.Receive.SenderClientId];
            // Saber el cambio que se va a hacer
            playerReadyNow = !playerReadyBefore;
            // Asignarlo
            playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = playerReadyNow;
            // Avisar a los clientes del cambio
            SetPlayerReady_ClientRpc(serverRpcParams.Receive.SenderClientId, playerReadyNow);
            // Si no esta actualmente en ready, ni siquiera comprobar si todos estan ready
            if (!playerReadyNow)
                return;
        }

        // Notifica a los clientes de que este jugador esta Ready o no
        SetPlayerReady_ClientRpc(serverRpcParams.Receive.SenderClientId);

        // Comprobar si todos los jugadores estan Ready y cambiar de escena
        CheckAllClientsReadyServerRpc();
    }

    // Comprueba si todos los jugadores tienen presionado el Ready, y en tal caso, cambia la escena
    [ServerRpc(RequireOwnership = false)]
    public void CheckAllClientsReadyServerRpc()
    {
        // Si se ha puesto en ready, comprobar si estan todos los jugadores en ready
        bool allClientsReady = true;
        foreach (ulong clientID in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionary.ContainsKey(clientID) || !playerReadyDictionary[clientID])
            {
                // Este jugador no esta listo
                allClientsReady = false;
                break;
            }
        }

        if (allClientsReady)
        {
            EraseLobbyReference_ClientRpc();
            GameLobby.Instance.DeleteLobby();
            GameLobby.Instance.LeaveLobby(); //
            SceneLoader.LoadNetwork(SceneLoader.SceneName.Gameplay);
        }
    }

    // Borra la referencia al lobby en los clientes, ya que el propio host lo ha eliminado
    [ClientRpc]
    void EraseLobbyReference_ClientRpc()
    { GameLobby.Instance.DeleteLobbyReference(); }

    // Notifica a los clientes de que un jugador ha cambiado su estado de Ready
    [ClientRpc]
    void SetPlayerReady_ClientRpc(ulong clientId, bool playerReady = true)
    {
        playerReadyDictionary[clientId] = playerReady;

        OnReadyChanged?.Invoke(this, new EventArgs());
    }

    // Devuelve true si el jugador con el Id "clientId" esta Ready
    public bool IsPlayerReady(ulong clientId)
    {
        return playerReadyDictionary.ContainsKey(clientId) && playerReadyDictionary[clientId];
    }
}
