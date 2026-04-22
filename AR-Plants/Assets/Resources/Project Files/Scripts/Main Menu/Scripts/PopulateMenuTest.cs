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

    public float maxSize = 1.3f;
    public float minSize = 0.1f;
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

    private VisualElement mainMenuRoot; // Store root for theme updates

    public GameObject mainMenu;
    //Chat gpt error fix. pulling database from web request for android compatibility
    void Awake()
    {
        SelectedCategory selectedData = GameObject.FindGameObjectWithTag("SelectedCategory").GetComponent<SelectedCategory>();
        plants = DatabaseManager.Instance.GetAllPlants();

        Debug.Log("no categories selected, loading all plants");
        Debug.Log("Plants type null, count: " + plants.Count);

        Debug.Log("SceneCounter:" + GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().sceneCounter);
        if (GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().sceneCounter >= 1)
        {
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

        // update color theme using manager
        if (ColorThemeManager.Instance != null)
        {
            ColorThemeManager.Instance.SetHighContrast(settings.highContrastToggle);
        }
        else
        {
            Debug.LogWarning("PopulateMenuTest: ColorThemeManager instance not found when applying theme.");
        }

        //copied from UpdateSoundManager() in SettingsHandler.cs
        float masterVolumeModifier = settings.masterVolume * 0.01f;

        if (settings.soundToggle == true)
        {
            //background noise
            SoundManager.Instance.menuMusicSource.volume = settings.musicVolume * .01f * masterVolumeModifier;
            SoundManager.Instance.ambientSoundSource.volume = settings.ambientVolume * .01f * masterVolumeModifier;

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
        mainMenuRoot = mainMenu.GetComponent<UIDocument>().rootVisualElement;
        var content = mainMenuRoot.Q<VisualElement>(contentHandelName);

        // Apply theme to main menu
        if (ColorThemeManager.Instance != null)
        {
            ColorThemeManager.Instance.ApplyThemeToUIDocument(mainMenuRoot);
            ApplyMainMenuBackgroundOverride();
            // Subscribe to theme changes
            ColorThemeManager.Instance.SubscribeToThemeChange(OnThemeChanged);
        }

        //add functionality to settings button
        Button settingsButton = mainMenuRoot.Q<Button>("settings-button");
        if (settingsButton != null)
        {
            settingsButton.clicked += () =>
            {
                SoundManager.Instance.PlayDefaultButtonSound();
                GameObject settingsMenu = Instantiate(settingsPrefab);
            };
        }

        //back button
        Button backButton = mainMenuRoot.Q<Button>("back-button");

        if (backButton != null)
        {
            backButton.clicked += OnBackButtonClicked;
        }

        // populate plant cards based on database
        for (int i = 0; i < plants.Count; i++)
        {
            PlantInfo currentPlant = plants[i];

            // Create a new button from the template
            VisualElement newPlantCardInstance = plantCardTemplate.CloneTree();
            Label plantNameLabel = newPlantCardInstance.Q<Label>("PlantName");

            Label scientificNameLabel = newPlantCardInstance.Q<Label>("ScientificName");

            Button button = newPlantCardInstance.Q<Button>("PlantDescriptionButton");

            // Plant card text should always be white (displayed over image background)
            plantNameLabel.style.color = new Color(1f, 1f, 1f, 1f); // White
            scientificNameLabel.style.color = new Color(1f, 1f, 1f, 1f); // White

            // Also set info labels to white (like "Species:")
            var infoLabels = newPlantCardInstance.Query<Label>(className: "info-label").ToList();
            foreach (Label label in infoLabels)
            {
                label.style.color = new Color(1f, 1f, 1f, 1f); // White
            }

            scientificNameLabel.text = currentPlant.scientificName;
            plantNameLabel.text = currentPlant.plantName;

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

    private void OnThemeChanged(ColorThemeManager.ColorTheme newTheme)
    {
        // Reapply theme when it changes
        if (mainMenuRoot != null && ColorThemeManager.Instance != null)
        {
            ColorThemeManager.Instance.ApplyThemeToUIDocument(mainMenuRoot);
            ApplyMainMenuBackgroundOverride();
            ApplyMainMenuTextOverrides();
        }
    }

    private void ApplyMainMenuBackgroundOverride()
    {
        if (mainMenuRoot == null || ColorThemeManager.Instance == null)
        {
            return;
        }

        var uiTheme = ColorThemeManager.Instance.GetUITheme();
        Color mainMenuBackground = ColorThemeManager.Instance.IsHighContrast() ? Color.black : uiTheme.backgroundColor;

        mainMenuRoot.style.backgroundColor = mainMenuBackground;

        VisualElement rootElement = mainMenuRoot.Q<VisualElement>("root");
        if (rootElement != null)
        {
            rootElement.style.backgroundColor = mainMenuBackground;
        }

        VisualElement contentElement = mainMenuRoot.Q<VisualElement>(contentHandelName) ?? mainMenuRoot.Q<VisualElement>("content");
        if (contentElement != null)
        {
            contentElement.style.backgroundColor = mainMenuBackground;

            var viewport = contentElement.Q<VisualElement>(className: "unity-scroll-view__content-viewport");
            if (viewport != null)
            {
                viewport.style.backgroundColor = mainMenuBackground;
            }

            var container = contentElement.Q<VisualElement>(className: "unity-scroll-view__content-container");
            if (container != null)
            {
                container.style.backgroundColor = mainMenuBackground;
            }
        }
    }

    private void ApplyMainMenuTextOverrides()
    {
        if (mainMenuRoot == null)
        {
            return;
        }

        // Plant card labels should stay white in both themes because cards are image-backed.
        var plantCardLabels = mainMenuRoot.Query<Label>(className: "card-title").ToList();
        foreach (Label label in plantCardLabels)
        {
            label.style.color = new Color(1f, 1f, 1f, 1f);
        }

        var scientificLabels = mainMenuRoot.Query<Label>().Where(l => l.name == "ScientificName").ToList();
        foreach (Label label in scientificLabels)
        {
            label.style.color = new Color(1f, 1f, 1f, 1f);
        }

        var infoLabels = mainMenuRoot.Query<Label>(className: "info-label").ToList();
        foreach (Label label in infoLabels)
        {
            label.style.color = new Color(1f, 1f, 1f, 1f);
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

                // Keep card text white in normal and high-contrast themes.
                plantNameLabel.style.color = new Color(1f, 1f, 1f, 1f);
                scientificNameLabel.style.color = new Color(1f, 1f, 1f, 1f);

                var infoLabels = newPlantCardInstance.Query<Label>(className: "info-label").ToList();
                foreach (Label label in infoLabels)
                {
                    label.style.color = new Color(1f, 1f, 1f, 1f);
                }


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

        ApplyMainMenuBackgroundOverride();
        ApplyMainMenuTextOverrides();
    }

    private void OnDestroy()
    {
        // Unsubscribe from theme changes to prevent memory leaks
        if (ColorThemeManager.Instance != null)
        {
            ColorThemeManager.Instance.UnsubscribeFromThemeChange(OnThemeChanged);
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
