using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField]
    LobbyCreateUI lobbyCreateUI;

    [SerializeField]
    Button mainMenuButton;
    [SerializeField]
    Button createLobbyButton;
    [SerializeField]
    Button quickJoinButton;
    [SerializeField]
    Button joinCodeButton;
    [SerializeField]
    TMP_InputField joinCodeInputField;

    [SerializeField]
    TMP_InputField playerNameInputField;

    [SerializeField]
    Transform lobbyContainer;
    [SerializeField]
    Transform lobbyTemplate;



    private void Awake()
    {
        mainMenuButton.onClick.AddListener(() =>
        {
            GameLobby.Instance.LeaveLobby();

            GameManager.GetInstance().ChangeScene(SceneLoader.SceneName.MainMenuScene);
            // SceneLoader.Load(SceneLoader.SceneName.MainMenuScene);
            FMODUnity.RuntimeManager.PlayOneShot("event:/PressButton");
        });
        createLobbyButton.onClick.AddListener(() =>
        {
            // Mostrar la tabla de creacion de lobby
            lobbyCreateUI.Show();
            FMODUnity.RuntimeManager.PlayOneShot("event:/PressButton");
        });
        quickJoinButton.onClick.AddListener(() =>
        { 
            GameLobby.Instance.QuickJoin();
            FMODUnity.RuntimeManager.PlayOneShot("event:/PressButton");
        });
        joinCodeButton.onClick.AddListener(() =>
        {
            GameLobby.Instance.JoinWithCode(joinCodeInputField.text);
            FMODUnity.RuntimeManager.PlayOneShot("event:/PressButton");
        });

        lobbyTemplate.gameObject.SetActive(false);
    }

    private void Start()
    {
        playerNameInputField.text = OnlineMultiplayerManager.Instance.GetPlayerName();
        playerNameInputField.onValueChanged.AddListener((string newText) =>
        {
            OnlineMultiplayerManager.Instance.SetPlayerName(newText);
        });

        GameLobby.Instance.OnLobbyListChanged += GameLobby_OnLobbyListChanged;
        UpdateLobbyList(new List<Lobby>());
    }

    private void GameLobby_OnLobbyListChanged(object sender, GameLobby.OnLobbyListChangedEventArgs e)
    {
        UpdateLobbyList(e.lobbyList);
    }

    void UpdateLobbyList(List<Lobby> lobbyList)
    {
        foreach (Transform child in lobbyContainer)
        {
            if (child == lobbyTemplate)
                continue;
            Destroy(child.gameObject);
        }

        foreach (Lobby lobby in lobbyList)
        {
            Transform lobbyTransform = Instantiate(lobbyTemplate, lobbyContainer);
            lobbyTransform.gameObject.SetActive(true);
            lobbyTransform.GetComponent<LobbyListSingleUI>().SetLobby(lobby);
        }
    }

    private void OnDestroy()
    {
        GameLobby.Instance.OnLobbyListChanged -= GameLobby_OnLobbyListChanged;
    }
}