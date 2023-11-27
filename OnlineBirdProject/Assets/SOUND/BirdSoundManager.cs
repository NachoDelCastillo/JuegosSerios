using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdSoundManager : MonoBehaviour
{
    FMOD.Studio.EventInstance instance;
    FMOD.Studio.PARAMETER_ID velocityId;

    // Referencia al evento
    public FMODUnity.EventReference fmodEvent;


    // Referencia al Script de movimiento de este jugador
    PlayerMovement playerMovement;
    // Velocidad actual de este pajaro
    // Se actualiza en cada iteracion al principio del update
    float currentVelocity;
    // Rango usado para calcular el valor normalizado de la velocidad
    float minVelocity = 5;
    float maxVelocity = 43;
    // Velocidad pero el valor va de 0 a 1, usando como referencia los
    // parametros de "minVelocity" y "maxVelocity"
    float birdVelocityNormalized;


    [SerializeField]
    [Range(0f, 1f)]
    float prueba;


    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Start()
    {
        instance = FMODUnity.RuntimeManager.CreateInstance(fmodEvent);

        FMOD.Studio.EventDescription eventDescription;
        instance.getDescription(out eventDescription);
        FMOD.Studio.PARAMETER_DESCRIPTION velocityParameterDescription;
        eventDescription.getParameterDescriptionByName("Velocity", out velocityParameterDescription); ;
        velocityId = velocityParameterDescription.id;

        instance.start();
    }

    private void Update()
    {
        currentVelocity = playerMovement.GetCurrentVelocity();

        // Transformar el valor de la velocidad en un valor generico para el parametro de FMOD
        // Es decir, un valor de 0 a 1, en el que 0 es el minimo y 1 es el maximo
        birdVelocityNormalized = (currentVelocity - minVelocity) / (maxVelocity - minVelocity);
        birdVelocityNormalized = Mathf.Clamp(birdVelocityNormalized, 0, 1);

        Debug.Log("birdVelocityNormalized = " + birdVelocityNormalized);

        // Informar a FMOD del nuevo valor de la velocidad
        instance.setParameterByID(velocityId, birdVelocityNormalized);
    }
}
