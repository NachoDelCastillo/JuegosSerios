using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class CharacterSelect_Local : MonoBehaviour
{
    [SerializeField] Transform playerContainer;

    // Referencias de colores
    
    [Serializable]
    public struct PlayerColor
    { public Color light, dark; }

    [SerializeField] public PlayerColor[] playerColors = new PlayerColor[4];


    // Local multiplayer variables

    PlayerInputManager playerInputManager;
    List<PlayerInput> allPlayers;

    private void Awake()
    {
        playerInputManager = GetComponent<PlayerInputManager>();

        allPlayers = new List<PlayerInput>();
    }

    public void PlayerJoined(PlayerInput newplayer)
    {
        Debug.Log("PlayerJoined");

        // Añadir un jugador extra

        //// Asignaciones al script del jugador
        //PlayerCard newPlayerCard = newplayer.GetComponent<PlayerCard>();
        //newPlayerCard.characterSelectionManager = this;
        //newPlayerCard.playerIndex = playerInputManager.playerCount - 1;

        //newPlayerCard.transform.SetParent(playerContainer);

        //// SpawnAnimation
        //newplayer.transform.localScale = Vector3.zero;
        //newplayer.transform.DOScale(1, 2).SetEase(Ease.OutElastic);

        //allPlayers.Add(newplayer);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {

            //// Almacenar los datos de los personajes en orden
            //CharacterData[] charactersData = new CharacterData[4];
            //for (int i = 0; i < allPlayers.Count; i++)
            //    charactersData[i] = allPlayers[i].GetComponent<PlayerCard>().characterSelected;

            //// Almacenar los dispositivos que se usan en orden
            //InputDevice[] devices = new InputDevice[4];
            //for (int i = 0; i < allPlayers.Count; i++)
            //    devices[i] = allPlayers[i].GetDevice<InputDevice>();

            //GameManager.GetInstance().AllPlayersSelected(charactersData, devices);
        }
    }
}
