using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterfallSound : MonoBehaviour
{
    StudioEventEmitter eventEmitter;
    FMOD.Studio.PARAMETER_ID altitudeId;


    // Jugador local con el listener de FMOD
    BirdSoundManager player;

    [Header("Min/Max altitude")]
    [SerializeField]
    Transform maxAltitude;
    [SerializeField]
    Transform minAltitude;

    float minY;
    float maxY;

    float currentPlayerY;

    public void Initialize(BirdSoundManager player)
    {
        this.player = player;

        eventEmitter = GetComponent<StudioEventEmitter>();
        // Acceder al ID del parametro de Altitude
        FMOD.Studio.EventDescription eventDescription;
        eventEmitter.EventInstance.getDescription(out eventDescription);
        FMOD.Studio.PARAMETER_DESCRIPTION altitudeParameterDescription;
        eventDescription.getParameterDescriptionByName("Altitude", out altitudeParameterDescription); ;
        altitudeId = altitudeParameterDescription.id;


        minY = minAltitude.position.y;
        maxY = maxAltitude.position.y;
    }

    private void Update()
    {
        if (player == null)
            return;

        // Si el jugador esta los suficientemente cerca, actualizar valores
        if (Vector3.Distance(player.transform.position, transform.position) < 500)
        {
            // Mueve este objeto a la misma altura que el jugador, respetando unos limites
            MoveToPlayerAltitude();

            // Calcular parametro de altitud y aplicarlo al emitter
            ApplyAltitudeToEvent();
        }
    }

    [SerializeField]
    [Range(0, 1)]
    float prueba;

    void ApplyAltitudeToEvent()
    {
        // Normalizar el valor entre 0 y 1 para que sea generico para FMOD
        // y de esta forma poder tener cascadas de diferentes tamaños
        float altitudeParameter = (currentPlayerY - minY) / (maxY - minY);

        Debug.Log("altitudeParameter = " + altitudeParameter);
        eventEmitter.EventInstance.setParameterByID(altitudeId, altitudeParameter);
        //eventEmitter.EventInstance.setParameterByName("Altitude", altitudeParameter);
    }

    void MoveToPlayerAltitude()
    {
        currentPlayerY = player.transform.position.y;

        // Si el jugador esta a nivel de la cascada
        // Mover el emmitter para que este siempre a la misma altura y se mantenga el efecto 3D
        if (currentPlayerY > minAltitude.position.y
            && currentPlayerY < maxAltitude.position.y)
            transform.position = new Vector3(transform.position.x, currentPlayerY, transform.position.z);

        Debug.Log("currentPlayerY = " + currentPlayerY);
        Debug.Log("minAltitude.position.y = " + minAltitude.position.y);
        Debug.Log("maxAltitude.position.y = " + maxAltitude.position.y);

    }


}
