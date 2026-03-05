//real populate menu, gets all the plants from database and displays them

using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using SQLite4Unity3d;
using System.IO;
using System.Linq;
using System;


public class PlantInfo
{
    [PrimaryKey, AutoIncrement]
    public int plantID { get; set; }
    public string plantName { get; set; }
    public string scientificName { get; set; }
    public string plantDesc { get; set; }
    public string stem { get; set; }
    public string leaf { get; set; }
    public string flower { get; set; }
    public int typeID { get; set; }

    public float maxSize = 2;
    public float minSize = .3f;
}

// public class SettingsData //data type for the settings
// {
//     public bool highContrastToggle = false;
//     public bool soundToggle = true;
//     public int masterVolume = 100;
//     public int soundEffects = 100;
//     public int musicVolume = 100;
//     public int ambientVolume = 100;
// }

public class PopulateMenuTest : MonoBehaviour
{

    [SerializeField] private GameObject plantButtonDataHolder;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private SQLiteConnection _connection;
    private List<PlantInfo> plants;

    [SerializeField] private VisualTreeAsset plantCardTemplate;
    [SerializeField] private GameObject mainMenuPrefab;
    [SerializeField] private GameObject settingsPrefab;
    //[SerializeField] private string buttonHandelName;
    [SerializeField] private string contentHandelName;
    public GameObject mainMenu;
    //Chat gpt error fix. pulling database from web request for android compatibility
    void Awake()
    {
        string dbName = "PlantInfoDB.db";
        string persistentPath = Path.Combine(Application.persistentDataPath, dbName);

        // First-run copy from StreamingAssets -> persistentDataPath
        if (!File.Exists(persistentPath))
        {
#if UNITY_ANDROID
            // StreamingAssets on Android must be read via UnityWebRequest
            string srcPath = Path.Combine(Application.streamingAssetsPath, dbName);
            // srcPath will be like "jar:file:///.../assets/PlantInfoDB.db"
            var req = UnityEngine.Networking.UnityWebRequest.Get(srcPath);
            var op = req.SendWebRequest();
            while (!op.isDone) { }  // simple blocking copy during Awake()
            if (req.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to copy DB from StreamingAssets: " + req.error);
            }
            else
            {
                File.WriteAllBytes(persistentPath, req.downloadHandler.data);
            }
#else
            // Desktop/editor/iOS etc.
            string srcPath = Path.Combine(Application.streamingAssetsPath, dbName);
            File.Copy(srcPath, persistentPath, overwrite: true);
#endif
        }

        // Open the DB from a real filesystem location
        _connection = new SQLite4Unity3d.SQLiteConnection(
            persistentPath,
            SQLite4Unity3d.SQLiteOpenFlags.ReadWrite | SQLite4Unity3d.SQLiteOpenFlags.Create
        );

        //plants = _connection.Table<PlantInfo>().ToList();
        //filtering for the plants in the selected category
        SelectedCategory selectedData = GameObject.FindGameObjectWithTag("SelectedCategory").GetComponent<SelectedCategory>();
        plants = _connection.Table<PlantInfo>().ToList();
        Debug.Log("no categories selected, loading all plants");
        Debug.Log("Plants type null, count: " + plants.Count);


        // goes back to previous plant description if going back to main menu from AR scene
        Debug.Log("SceneCounter:" + GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().sceneCounter);
        if (GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().sceneCounter >= 1)
        {
            //Destroy(GameObject.FindGameObjectWithTag("PlantDescription"));
            Instantiate(Resources.Load<GameObject>("PlantDescription")).GetComponent<UIDocument>().panelSettings.sortingOrder = 2;
        }
    }

    public void Start()
    {
        // Set sorting order on start to ensure correct menu is on top
        GameObject.Find("categMenu(Clone)").GetComponent<UIDocument>().panelSettings.sortingOrder = 1;
        mainMenu.GetComponent<UIDocument>().panelSettings.sortingOrder = 0;

        if (GameManager.Instance.sceneCounter >= 1)
        {
            SoundManager.Instance.StopAmbientSounds();
            if (SoundManager.Instance.musicSoundEnabled)
            {
                SoundManager.Instance.PlayMusic();
            }
        }
        else
        {
            if (SoundManager.Instance.musicSoundEnabled)
            {
                SoundManager.Instance.PlayMusic();
            }
        }

        //instantiate settings for app
        //string jsonFilePath = "Assets/Resources/Project Files/Scripts/Settings/Resources/";
        string persistentPath = Path.Combine(Application.persistentDataPath, "settings.json");
        SettingsData settings;
        string json = File.ReadAllText(persistentPath);
        settings = JsonUtility.FromJson<SettingsData>(json);

        //update color theme
        if (settings.highContrastToggle)
        {
            //TODO: set to high contrast
        }
        else
        {
            //TODO: set to normal theme
        }

        //copied from UpdateSoundManager() in SettingsHandler.cs
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
        //end code copied from UpdateSoundManager()
    }

    void OnEnable()
    {
        // Instantiate UI and data holder prefabs and keep references
        mainMenu = Instantiate(mainMenuPrefab);
        plantButtonDataHolder = Instantiate(Resources.Load<GameObject>("PlantButtonDataHolder"));

        // Get the root and content container
        var root = mainMenu.GetComponent<UIDocument>().rootVisualElement;
        var content = root.Q<VisualElement>(contentHandelName);

        //add functionality to settings button
        Button settingsButton = root.Q<Button>("settings-button");
        if (settingsButton != null)
        {
            settingsButton.clicked += () =>
            {
                SoundManager.Instance.PlayDefaultButtonSound();
                GameObject settingsMenu = Instantiate(settingsPrefab);
            };
        }

        //back button
        Button backButton = root.Q<Button>("back-button");

        if (backButton != null)
        {
            backButton.clicked += OnBackButtonClicked;
        }
    }


    // added function to clear current plant buttons and repopulate with plants of the selected category
    public void ClearAndPopulateMenuWithCategoryPlants(PlantTypes category)
    {
        plantButtonDataHolder = Instantiate(Resources.Load<GameObject>("PlantButtonDataHolder"));
        var root = mainMenu.GetComponent<UIDocument>().rootVisualElement;
        var content = root.Q<VisualElement>(contentHandelName);
        content.Clear();
        Destroy(GameObject.FindGameObjectWithTag("DataHolder"));
        for (int i = 0; i < plants.Count; i++)
        {
            if (plants[i].typeID != category.typeID)
            {
                continue;
            }
            else
            {
                PlantInfo currentPlant = plants[i];

                // Create a new button from the template
                VisualElement newPlantCardInstance = plantCardTemplate.CloneTree();
                Label plantNameLabel = newPlantCardInstance.Q<Label>("PlantName");

                Label scientificNameLabel = newPlantCardInstance.Q<Label>("ScientificName");

                Button button = newPlantCardInstance.Q<Button>("PlantDescriptionButton");


                scientificNameLabel.text = currentPlant.scientificName;
                plantNameLabel.text = currentPlant.plantName;

                //Because Azeezat likes comments: 
                // Setting the background image of the plant card according to name
                VisualElement cardElement = newPlantCardInstance.Q<VisualElement>("plant-card");
                String backgroundImagePath = $"Project Files/Scripts/PlantDescriptionComponent/Images/{currentPlant.plantName}";
                Texture2D backgroundTexture = Resources.Load<Texture2D>(backgroundImagePath);
                if (backgroundTexture != null)
                {
                    cardElement.style.backgroundImage = new StyleBackground(backgroundTexture);
                }
                else
                {
                    //default image is the golden cactus, may want to change this in the future
                    Debug.LogWarning($"Background image not found at path: {backgroundImagePath}");

                }

                // Add the button to the content container
                content.Add(newPlantCardInstance);

                // Creates a new GameObject holding its own LoadDatabaseInfo script pertaining to the specific plant in the itteration
                // This allows each button to have its own data loader instance
                GameObject dataObj = new GameObject($"PlantData_{currentPlant.plantName}");
                dataObj.transform.SetParent(plantButtonDataHolder.transform, false);

                LoadDatabaseInfo dataLoader = dataObj.AddComponent<LoadDatabaseInfo>();
                dataLoader.plantInfo = currentPlant;

                //Wire button to its corresponding data loader instance
                button.clicked += dataLoader.OnClick;
            }
        }
    }

    public void ClearMainMenuPlants()
    {
        var root = mainMenu.GetComponent<UIDocument>().rootVisualElement;
        var content = root.Q<VisualElement>(contentHandelName);
        content.Clear();
        Destroy(GameObject.FindGameObjectWithTag("DataHolder"));
    }

    private void OnBackButtonClicked()
    {
        GameObject categories = GameObject.Find("categMenu(Clone)");
        GameObject populator = GameObject.FindGameObjectWithTag("MainMenuPopulator");
        GameObject.FindGameObjectWithTag("MainMenu").GetComponent<UIDocument>().panelSettings.sortingOrder = 0;
        categories.GetComponent<UIDocument>().panelSettings.sortingOrder = 1;
        ClearMainMenuPlants();

        Debug.Log("Back button was clicked. Back to categories page");
        GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().sceneCounter += 1;
        SoundManager.Instance.PlayDefaultButtonSound();
    }
}
