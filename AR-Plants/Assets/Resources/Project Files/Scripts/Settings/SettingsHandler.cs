using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SettingsData //data type for the settings
{
    public bool soundToggle = true;
    public int soundEffects = 100;
    public int masterVolume = 100;
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