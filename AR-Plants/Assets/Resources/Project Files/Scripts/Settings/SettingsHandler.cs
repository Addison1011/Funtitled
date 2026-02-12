using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class SettingsData //data type for the settings
{
    public bool highContrastToggle = false;
    public bool soundToggle = true;
    public int masterVolume = 100;
    public int soundEffects = 100;
    public int musicVolume = 100;
    public int ambientVolume = 100;
}


public class SettingsHandler : MonoBehaviour
{
    public SettingsData settings;

    private string jsonFilePath = "Assets/Resources/Project Files/Scripts/Settings/Resources/";
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
        UpdateSoundManager();
        
        var root  = GetComponent<UIDocument>().rootVisualElement;

        //add functionality to buttons
        Button cancelButton = root.Q<Button>("cancel-button");
        if (cancelButton != null)
        {
            cancelButton.clicked += () =>
            {
                LoadSettingsFromJson();
                UpdateSoundManager();
                // Reset theme to match loaded settings
                if (ColorThemeManager.Instance != null)
                {
                    ColorThemeManager.Instance.SetHighContrast(settings.highContrastToggle);
                }
                SoundManager.Instance.PlayDefaultButtonSound();
                Destroy(gameObject);
            };
        }

        Button saveButton = root.Q<Button>("save-button");
        if (saveButton != null)
        {
            saveButton.clicked += () =>
            {
                SoundManager.Instance.PlayDefaultButtonSound();
                SaveSettingsToJson();
                Destroy(gameObject);
            };
        }

        //set up listener for high contrast toggle
        Toggle contrastToggleBox = root.Q<Toggle>("high-contrast-button");
        if (contrastToggleBox != null)
        {
            contrastToggleBox.value = settings.highContrastToggle;
            contrastToggleBox.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                settings.highContrastToggle = evt.newValue;
                Debug.Log($"SettingsHandler: high-contrast toggle changed -> {evt.newValue}");
                // Apply theme change to ColorThemeManager
                if (ColorThemeManager.Instance != null)
                {
                    Debug.Log("SettingsHandler: calling ColorThemeManager.SetHighContrast from toggle callback");
                    ColorThemeManager.Instance.SetHighContrast(evt.newValue);
                    ApplyUITheme(root);
                }
                else
                {
                    Debug.LogWarning("SettingsHandler: ColorThemeManager.Instance is null when toggle changed");
                }
            });

            // Ensure ColorThemeManager matches saved settings when the UI opens
            if (ColorThemeManager.Instance != null)
            {
                Debug.Log($"SettingsHandler: setting ColorThemeManager to saved state -> {settings.highContrastToggle}");
                ColorThemeManager.Instance.SetHighContrast(settings.highContrastToggle);
                ApplyUITheme(root);
            }
            else
            {
                Debug.LogWarning("SettingsHandler: ColorThemeManager.Instance is null on settings open");
            }
        }

        

        //set up listener for toggle
        Toggle soundToggleBox = root.Q<Toggle>("sound-toggle");
        if (soundToggleBox != null)
        {
            soundToggleBox.value = settings.soundToggle;
            soundToggleBox.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                settings.soundToggle = evt.newValue;
                UpdateSoundManager();
            });
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
                    UpdateSoundManager();
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
                    UpdateSoundManager();
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
                    UpdateSoundManager();
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
                    UpdateSoundManager();
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

    public void UpdateSoundManager()
    {

        Debug.Log("hello");
        //update sound volumes
        float masterVolumeModifier = settings.masterVolume * 0.01f;

        if(settings.soundToggle == true)
        {
            //background noise
            SoundManager.Instance.menuMusicSource.volume = settings.musicVolume * .01f * masterVolumeModifier;
            SoundManager.Instance.ambientSoundSource.volume = settings.ambientVolume *.01f * masterVolumeModifier;

            //sound effects
            SoundManager.Instance.selectLeafSoundSource.volume = settings.soundEffects * .01f * masterVolumeModifier;
            SoundManager.Instance.selectFlowerSoundSource.volume = settings.soundEffects * .01f * masterVolumeModifier;
            SoundManager.Instance.selectBranchSoundSource.volume = settings.soundEffects * .01f * masterVolumeModifier;
            SoundManager.Instance.interactionSoundSource.volume = settings.soundEffects * .01f * masterVolumeModifier;
        }
        else //sound is off
        {
            //background noise
            SoundManager.Instance.menuMusicSource.volume = 0;
            SoundManager.Instance.ambientSoundSource.volume = 0;

            //sound effects
            SoundManager.Instance.selectLeafSoundSource.volume = 0;
            SoundManager.Instance.selectFlowerSoundSource.volume = 0;
            SoundManager.Instance.selectBranchSoundSource.volume = 0;
            SoundManager.Instance.interactionSoundSource.volume = 0;
        }
    }

    private void ApplyUITheme(VisualElement root)
    {
        if (ColorThemeManager.Instance == null)
            return;

        var uiTheme = ColorThemeManager.Instance.GetUITheme();

        // Apply theme to root and content areas
        VisualElement rootElement = root.Q<VisualElement>("root");
        if (rootElement != null)
            rootElement.style.backgroundColor = uiTheme.backgroundColor;

        VisualElement header = root.Q<VisualElement>("header");
        if (header != null)
            header.style.backgroundColor = uiTheme.headerBackgroundColor;

        VisualElement footer = root.Q<VisualElement>("footer");
        if (footer != null)
            footer.style.backgroundColor = uiTheme.headerBackgroundColor;

        // Apply text color to labels
        var labels = root.Query<Label>().ToList();
        foreach (Label label in labels)
        {
            label.style.color = uiTheme.textColor;
        }

        // Apply text color to toggles
        var toggles = root.Query<Toggle>().ToList();
        foreach (Toggle toggle in toggles)
        {
            toggle.style.color = uiTheme.textColor;
        }

        Debug.Log($"SettingsHandler: applied UI theme -> {(ColorThemeManager.Instance.IsHighContrast() ? "High Contrast" : "Normal")}");
    }

}