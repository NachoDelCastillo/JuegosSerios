using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnviromentSound : MonoBehaviour
{
    public FMOD.Studio.EventInstance instance;
    public FMOD.Studio.EventInstance instance2;
    // Referencia al evento
    public FMODUnity.EventReference fmodEvent;
    public FMODUnity.EventReference fmodEvent2;

    public int xPos=500,yPos=200,zPos=500;

    // Jugador local con el listener de FMOD
    BirdManager player;

    private void Start()
    {
        instance = FMODUnity.RuntimeManager.CreateInstance(fmodEvent);
        instance2 = FMODUnity.RuntimeManager.CreateInstance(fmodEvent2);
        instance.start();
        instance2.start();
        instance.setParameterByName("Enviroment", 0);
        instance2.setParameterByName("Nature", 0);

        // Informar a los objetos de la escena que este es el pajaro local
        BirdManager[] birdManagers = FindObjectsByType<BirdManager>(FindObjectsSortMode.None);
        for (int i = 0; i < birdManagers.Length; i++)
            if (birdManagers[i].IsOwner)
                player = birdManagers[i];

    }

    private void Update()
    {


        // Si el jugador esta los suficientemente cerca, actualizar valores
        if (player.transform.position.x - transform.position.x < xPos && player.transform.position.y - transform.position.y < yPos &&
            player.transform.position.z - transform.position.z< zPos)
        {
            Debug.Log(Mathf.Clamp(Vector3.Distance(player.transform.position, transform.position), 0, 1));
            instance.setParameterByName("Enviroment",1);
        }
    }
}
