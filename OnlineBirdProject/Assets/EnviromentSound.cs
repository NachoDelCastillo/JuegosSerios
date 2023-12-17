using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnviromentSound : MonoBehaviour
{
    public FMOD.Studio.EventInstance instance;
    // Referencia al evento
    public FMODUnity.EventReference fmodEvent;
    // Start is called before the first frame update
    void Start()
    {
        instance = FMODUnity.RuntimeManager.CreateInstance(fmodEvent);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<BirdManager>() && other.GetComponent<BirdManager>().IsOwner)
        {
            instance.start();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<BirdManager>() && other.GetComponent<BirdManager>().IsOwner)
        {
            instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
    }
}
