using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HostDisconnectUI : ShowHideObj
{
    [SerializeField] private Button returnToMenuButton;

    new void Start()
    {
        if (GameManager.IsLocal())
        {
            Hide();
            return;
        }

        base.Start();

        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;

        returnToMenuButton.onClick.AddListener(() =>
        {
            SceneLoader.Load(SceneLoader.SceneName.MainMenuScene);
        });

        Hide();
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        if (clientId == NetworkManager.ServerClientId)
        {
            // El Server se esta cerrando
            Show();
        }
    }

    private void OnDestroy()
    {
        // Desuscribir a este metodo del evento, ya que el tiempo de vida de la funcion OnClientDisconnectCallback es mayor
        // que la vida de este objeto (el cual se borrara cuando se cambie de escena), de esta forma no se intenta llamar a 
        // un metodo que ya no existe
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnectCallback;
    }
}
