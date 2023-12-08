using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AllMenuManager_PK : MonoBehaviour
{
    [Header("SETUP")]
    [SerializeField] Transform cameraObj;

    [SerializeField] float cameraSpeed;

    float cameraDistanceX = 25;
    float cameraDistanceY = 15;


    // Referencias a todos los menus
    MainMenu_PK mainMenu;
    SettingsMenu_PK settingsMenu;
    LevelselectorMenu_PK levelSelectorMenu;

    float initialZ;
    float initialY;

    private void Awake()
    {
        mainMenu = FindObjectOfType<MainMenu_PK>();
        settingsMenu = FindObjectOfType<SettingsMenu_PK>();
        levelSelectorMenu = FindObjectOfType<LevelselectorMenu_PK>();

        mainMenu.enabled = true;
        settingsMenu.enabled = false;
        levelSelectorMenu.enabled = false;

        initialZ = cameraObj.transform.position.z;
        initialY = cameraObj.transform.position.y;
    }


    private void Update()
    {
        //Debug.Log("Mathf.Abs(cameraObj.position.z) = " + Mathf.Abs(cameraObj.position.z));
        //Debug.Log("initialZ + cameraDistanceX = " + initialZ + cameraDistanceX);

        if (Input.anyKeyDown && ( Mathf.Abs(cameraObj.position.z) >= initialZ + cameraDistanceX || Mathf.Abs(cameraObj.position.z) < 181 )) 
        {
            // Sound
            AudioManager_PK.GetInstance().Play("ButtonPress", 1);

            StartCoroutine(EnableMenu(mainMenu, true, cameraSpeed));
            cameraObj.DOMoveZ(initialZ, cameraSpeed);
        }
    }

    // Activa/Desactiva el script indicado dentro de Xsegundos
    // Esto se usa para activar los menus una vez la camara ha llegado
    IEnumerator EnableMenu(MonoBehaviour menuScript, bool value, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        menuScript.enabled = value;
    }

    #region buttonPress

    #region mainMenu Buttons

    public void PressPlay()
    {
        mainMenu.enabled = false;
        StartCoroutine(EnableMenu(levelSelectorMenu, true, cameraSpeed / 2));

        cameraObj.DOMoveY(initialY + cameraDistanceY, cameraSpeed);
    }

    public void PressSettings()
    {
        mainMenu.enabled = false;
        StartCoroutine(EnableMenu(settingsMenu, true, cameraSpeed/2));

        cameraObj.DOMoveY(initialY - cameraDistanceY, cameraSpeed);
    }

    public void PressControls()
    {
        mainMenu.enabled = false;

        cameraObj.DOMoveZ(initialZ + cameraDistanceX, cameraSpeed);
    }

    public void PressCredits()
    {
        mainMenu.enabled = false;

        cameraObj.DOMoveZ(initialZ - cameraDistanceX, cameraSpeed);
    }

    #endregion

    #region Settings Buttons

    public void BackButton()
    {
        settingsMenu.enabled = false;
        levelSelectorMenu.enabled = false;
        StartCoroutine(EnableMenu(mainMenu, true, cameraSpeed/2));

        cameraObj.DOMoveY(initialY, cameraSpeed);
    }

    #endregion

    #endregion
}
