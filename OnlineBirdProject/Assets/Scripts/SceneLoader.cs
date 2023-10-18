using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public enum SceneName
    {
        MainMenuScene,
        GameScene, 
        Gameplay, 
        LoadingScene,
        LobbyScene,
        CharacterSelectScene,

        None
    }

    public static void Load(SceneName sceneName)
    {
        SceneManager.LoadScene(sceneName.ToString());
    }

    public static void LoadNetwork(SceneName sceneName)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName.ToString(), LoadSceneMode.Single);
    }
}
