using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using FMODUnity;

public class AudioManager_PK : MonoBehaviour
{
    public Sound[] sounds;

    public static AudioManager_PK instance;
    public static AudioManager_PK GetInstance() { return instance; }

    void Awake()
    {
        // Singleton
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.music;
        }

        // Valores iniciales de los sliders de musica y sonido
        AudioVolume(PlayerPrefs.GetFloat("SoundVolume", 5), false);
        AudioVolume(PlayerPrefs.GetFloat("MusicVolume", 5), true);

        ChangeBackgroundMusic(ParseEnum<SceneLoader.SceneName>(SceneManager.GetActiveScene().name));
    }

    public static T ParseEnum<T>(string value)
    {
        return (T)Enum.Parse(typeof(T), value, true);
    }

    public float GetCurrentSoundVolume()
    { return PlayerPrefs.GetFloat("SoundVolume", 5); }

    public float GetCurrentMusicVolume()
    { return PlayerPrefs.GetFloat("MusicVolume", 5); }

    public void ChangeBackgroundMusic(SceneLoader.SceneName sceneNameEnum) // Cambia la muscia de fondo segun la escena, llamado desde 
    {
        string sceneName = sceneNameEnum.ToString();

        switch (sceneNameEnum)
        {
            case SceneLoader.SceneName.MainMenuScene:
                Stop("Gameplay_Music");
                Play("MainMenu_Music", 1);
                break;
            default:
                Stop("MainMenu_Music");
                Play("Gameplay_Music", 1);
                break;
        }
    }

    public void Play(string name, float pitch)
    {
        if (name == "MainMenu_Music")
        {
            Debug.Log("MainMenu_Music");
            FMODUnity.RuntimeManager.PlayOneShot("event:/MenuMusic");
        }

        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.source.pitch = pitch;
        s.source.Play();
    }

    public void PlayOneShoot(EventReference sound,Vector3 worldPosition)
    {
        RuntimeManager.PlayOneShot(sound, worldPosition);
    }

    public void Stop(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.source.Stop();
    }

    public void AudioVolume(float volume, bool isMusic) // Se llama cada vez que se modifica el sonido o la musica
    {
        // Almacenar valores en PlayerPref
        if (isMusic)  // Volumen Musica
            PlayerPrefs.SetFloat("MusicVolume", volume);

        if (!isMusic) // Volumen Sonido
            PlayerPrefs.SetFloat("SoundVolume", volume);

        // Convertir a un valor sobre 1
        volume = volume / 10;

        AudioSource[] AllaudioSources = GetComponents<AudioSource>();
        
        for (int i = 0; i < AllaudioSources.Length; i++)
        {
            if (sounds[i].music == isMusic)
                AllaudioSources[i].volume = volume * sounds[i].maxVolume; // Cambiar el volumen de todas las musicas/sonidos dependiendo de su "maxVolume"
        }
    }
}
