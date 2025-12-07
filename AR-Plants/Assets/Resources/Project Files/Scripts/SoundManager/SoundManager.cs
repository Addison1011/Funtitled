using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

using UnityEngine.SceneManagement;
using System.Diagnostics;
using UnityEngine.Audio;


public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    public GameManager gameManager;



    public bool ambientSoundEnabled = true;
    public bool musicSoundEnabled = true;

    public AudioSource menuMusicSource;

    public AudioSource ambientSoundSource;

    public AudioSource selectLeafSoundSource;

    public AudioSource selectFlowerSoundSource;

    public AudioSource selectBranchSoundSource;

    public AudioSource interactionSoundSource;

    public AudioClip[] backgroundSoundClips;

    public AudioClip[] interactionSoundClips;



    void Start()
    {
        gameManager = GameManager.Instance;

    }

    void Update()
    {

    }





    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }


    /*public void ToggleAtmosphericSoundSetting()
    {
        if (soundSettings == SoundSettings.AmbientSoundOnly)
        {
            StopAmbientSounds();
            PlayMusic();
        }
        else if (soundSettings == SoundSettings.MusicSoundOnly)
        {
            StopMusic();
            PlayAmbientSounds();
        }
    }*/

    public void PlayMusic()
    {

        //AudioClip[] musicClips = backgroundSoundClips.Where(clip => clip.name.Contains("Music")).ToArray();
        menuMusicSource.Play();

    }

    public void StopMusic()
    {
        menuMusicSource.Stop();
    }

    public void PlayAmbientSounds()
    {
        ambientSoundSource.Play();
    }

    public void StopAmbientSounds()
    {
        ambientSoundSource.Stop();
    }

    public void PlaySelectPlantPartSound()
    {
        interactionSoundSource.clip = GetInteractionSoundClip("SelectPlantPartSound");
        interactionSoundSource.Play();
    }

    public void PlaySelectLeafSound()
    {
        selectLeafSoundSource.Play();
    }

    public void PlaySelectFlowerSound()
    {
        selectFlowerSoundSource.Play();
    }

    public void PlaySelectBranchSound()
    {
        selectBranchSoundSource.Play();
    }


    private AudioClip GetBackgroundSoundClip(string clipName)
    {
        return backgroundSoundClips.FirstOrDefault(clip => clip.name == clipName);
    }

    private AudioClip GetInteractionSoundClip(string clipName)
    {
        return interactionSoundClips.FirstOrDefault(clip => clip.name == clipName);
    }

    // **** Ambient Sounds **** //
    public void PlayBackgroundMusic()
    {

    }



    // **** Interaction Sounds **** //
    public void PlayPlantPlacementSound()
    {
        interactionSoundSource.clip = GetInteractionSoundClip("PlantPlacementSound");
        interactionSoundSource.Play();
    }

    public void PlayBackButtonSound()
    {
        interactionSoundSource.clip = GetInteractionSoundClip("BackButtonSound");
        interactionSoundSource.Play();
    }


    public void PlayEnterARButtonSound()
    {
        interactionSoundSource.clip = GetInteractionSoundClip("EnterARButtonSound");
        interactionSoundSource.Play();
    }

    public void PlayRefreshARSceneSound()
    {
        interactionSoundSource.clip = GetInteractionSoundClip("RefreshARSceneSound");
        interactionSoundSource.Play();
    }

    public void PlayDefaultButtonSound()
    {
        interactionSoundSource.clip = GetInteractionSoundClip("DefaultButtonSound");
        interactionSoundSource.Play();
    }
}