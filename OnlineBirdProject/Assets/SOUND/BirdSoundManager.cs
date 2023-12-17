using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD;

public class BirdSoundManager : MonoBehaviour
{
    public FMOD.Studio.EventInstance instance;
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


        instance.setParameterByName("Obstruction", 1);
        instance.start();

        // Comprobar si este es el pajaro local
        if (GetComponent<BirdManager>().IsOwnerBool())
        {
            // Informar a los objetos de la escena que este es el pajaro local
            WaterfallSound[] waterfallSounds = FindObjectsByType<WaterfallSound>(FindObjectsSortMode.None);

            for (int i = 0; i < waterfallSounds.Length; i++)
                waterfallSounds[i].Initialize(this);
        }
        else
        {
            // Si no es el pajaro local
            GetComponent<FMODUnity.StudioListener>().enabled = false;
            this.enabled = false;

            instance.start();
        }
    }

    private void Update()
    {
        currentVelocity = playerMovement.GetCurrentVelocity();

        // Transformar el valor de la velocidad en un valor generico para el parametro de FMOD
        // Es decir, un valor de 0 a 1, en el que 0 es el minimo y 1 es el maximo
        birdVelocityNormalized = (currentVelocity - minVelocity) / (maxVelocity - minVelocity);
        birdVelocityNormalized = Mathf.Clamp(birdVelocityNormalized, 0, 1);


        float finalValue = Mathf.Max(birdVelocityNormalized, NearestBirdDistance());

        // Informar a FMOD del nuevo valor de la velocidad
        instance.setParameterByID(velocityId, finalValue);
        instance.setParameterByName("Velocity", finalValue);
    }

    float maxDistance = 30;
    float minDistance = 2;

    float NearestBirdDistance()
    {
       
        List<BirdManager> lb = GameplayManager.Instance.allBirds;

        float currentMinDistance = Mathf.Infinity;

        for (int i = 0; i < lb.Count; i++)
        {
            BirdManager thisLB = lb[i];
            if (thisLB != null && !thisLB.IsOwner)
            {
                float distance = Vector3.Distance(transform.position, thisLB.transform.position);
                if (distance < currentMinDistance)
                    currentMinDistance = distance;
            }
        }

        float finalValue;

        finalValue = (currentMinDistance - minDistance) / (maxDistance - minDistance);
        finalValue = Mathf.Clamp(finalValue, 0, 1);
        finalValue = 1 - finalValue;

        UnityEngine.Debug.Log("finalValue = " + finalValue);

        return finalValue;
    }
}
