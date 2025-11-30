using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class SettingsData //data type for the settings
{
    public bool soundToggle = true;
    public int masterVolume = 100;
    public int soundEffects = 100;
    public int musicVolume = 100;
    public int ambientVolume = 100;
}


public class SettingsHandler : MonoBehaviour
{
    public SettingsData settings;

    [SerializeField] private string jsonFilePath = "Project Files/Scripts/Settings/settings";
    private string persistentPath;

    void Awake()
    {
        //load the settings when page is opened
        persistentPath = Path.Combine(Application.persistentDataPath, "settings.json");
        if(!File.Exists(persistentPath))
        {
            TextAsset ta = Resources.Load<TextAsset>(jsonFilePath);
            if(ta != null)
            {
                File.WriteAllText(persistentPath, ta.text);
            }
            else
            {
                File.WriteAllText(persistentPath, JsonUtility.ToJson(new SettingsData(), true));
            }
        }
        LoadSettingsFromJson();
    }

    void OnEnable()
    {
        var root  = GetComponent<UIDocument>().rootVisualElement;

        //add functionality to buttons
        Button cancelButton = root.Q<Button>("cancel-button");
        if (cancelButton != null)
        {
            cancelButton.clicked += () =>
            {
                Destroy(gameObject);
            };
        }

        Button saveButton = root.Q<Button>("save-button");
        if (saveButton != null)
        {
            saveButton.clicked += () =>
            {
                SaveSettingsToJson();
                Destroy(gameObject);
            };
        }

        //set up listeners for sliders
        SliderInt masterVolumeSlider = root.Q<SliderInt>("master-volume-bar");
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = settings.masterVolume;
            masterVolumeSlider.RegisterCallback<ChangeEvent<int>>(evt =>
            {
                settings.masterVolume = evt.newValue;
            
                Label numberLabel = masterVolumeSlider.Q<Label>();
                if (numberLabel != null)
                {
                    numberLabel.text = evt.newValue.ToString();
                }
            });
        }

        SliderInt soundEffectsSlider = root.Q<SliderInt>("sound-effects-bar");
        if (soundEffectsSlider != null)
        {
            soundEffectsSlider.value = settings.soundEffects;
            soundEffectsSlider.RegisterCallback<ChangeEvent<int>>(evt =>
            {
                settings.soundEffects = evt.newValue;
            
                Label numberLabel = soundEffectsSlider.Q<Label>();
                if (numberLabel != null)
                {
                    numberLabel.text = evt.newValue.ToString();
                }
            });
        }

        SliderInt musicVolumeSlider = root.Q<SliderInt>("music-volume-bar");
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = settings.musicVolume;
            musicVolumeSlider.RegisterCallback<ChangeEvent<int>>(evt =>
            {
                settings.musicVolume = evt.newValue;
            
                Label numberLabel = musicVolumeSlider.Q<Label>();
                if (numberLabel != null)
                {
                    numberLabel.text = evt.newValue.ToString();
                }
            });
        }

        SliderInt ambientVolumeSlider = root.Q<SliderInt>("ambient-volume-bar");
        if (ambientVolumeSlider != null)
        {
            ambientVolumeSlider.value = settings.ambientVolume;
            ambientVolumeSlider.RegisterCallback<ChangeEvent<int>>(evt =>
            {
                settings.ambientVolume = evt.newValue;
            
                Label numberLabel = ambientVolumeSlider.Q<Label>();
                if (numberLabel != null)
                {
                    numberLabel.text = evt.newValue.ToString();
                }
            });
        }
    }

    //load settings variable with data from json
    public void LoadSettingsFromJson()
    {
        string json = File.ReadAllText(persistentPath);
        settings = JsonUtility.FromJson<SettingsData>(json);
    }

    //save settings to json
    public void SaveSettingsToJson()
    {
        string json = JsonUtility.ToJson(settings);
        File.WriteAllText(persistentPath, json);
    }

}