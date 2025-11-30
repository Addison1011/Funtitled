using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

using UnityEngine.SceneManagement;
using System.Diagnostics;
using UnityEngine.Audio;

public enum SoundSettings
{
    AmbientSoundOnly,
    MusicSoundOnly,
}
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    public GameManager gameManager;

    [SerializeField]
    private AudioSource menuMusicSource;

    [SerializeField]
    private AudioSource ambientSoundSource;

    [SerializeField]
    private AudioSource selectLeafSoundSource;

    [SerializeField]
    private AudioSource selectFlowerSoundSource;

    [SerializeField]
    private AudioSource selectBranchSoundSource;

    [SerializeField]
    private AudioSource interactionSoundSource;
    [SerializeField]
    private AudioClip[] backgroundSoundClips;
    [SerializeField]
    private AudioClip[] interactionSoundClips;

    public SoundSettings soundSettings;


    void Start()
    {
        gameManager = GameManager.Instance;
        // interactionSoundSource = GameObject.FindGameObjectWithTag("InteractionSound").GetComponent<AudioSource>();
        //menuMusicSource = GameObject.FindGameObjectWithTag("MenuMusicSound").GetComponent<AudioSource>();
        //audioRandomizer = GetComponent<AudioResource>();

        if (soundSettings == SoundSettings.AmbientSoundOnly)
        {
            PlayAmbientSounds();
        }
        else if (soundSettings == SoundSettings.MusicSoundOnly)
        {
            PlayMusic();
        }
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


    public void ToggleAtmosphericSoundSetting()
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
    }

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