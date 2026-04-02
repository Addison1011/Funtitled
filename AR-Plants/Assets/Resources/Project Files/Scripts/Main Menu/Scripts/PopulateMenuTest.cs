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

    public float maxSize = 1000f;
    public float minSize = 0f;
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
    public GameObject MainMenuPrefab => mainMenuPrefab;
    [SerializeField] private GameObject settingsPrefab;
    //[SerializeField] private string buttonHandelName;
    [SerializeField] private string contentHandelName;

    private VisualElement mainMenuRoot; // Store root for theme updates
    private Button backButton;

    public GameObject mainMenu;

    public void SetMainMenuAndRefresh(GameObject newMainMenu)
    {
        mainMenu = newMainMenu;
        if (mainMenu != null)
        {
            var uiDoc = mainMenu.GetComponent<UIDocument>();
            if (uiDoc != null)
            {
                mainMenuRoot = uiDoc.rootVisualElement;
                backButton = null;
                Debug.Log("PopulateMenuTest: SetMainMenuAndRefresh called - mainMenuRoot updated and backButton reset.");
                SetupBackButton();
            }
            else
            {
                Debug.LogWarning("PopulateMenuTest: newMainMenu has no UIDocument component.");
            }
        }
        else
        {
            Debug.LogWarning("PopulateMenuTest: SetMainMenuAndRefresh called with null mainMenu.");
        }
    }
    //Chat gpt error fix. pulling database from web request for android compatibility
    void Awake()
    {
        var selectedCategoryObj = GameObject.FindGameObjectWithTag("SelectedCategory");
        if (selectedCategoryObj != null)
        {
            SelectedCategory selectedData = selectedCategoryObj.GetComponent<SelectedCategory>();
        }
        
        plants = DatabaseManager.Instance?.GetAllPlants();

        Debug.Log("no categories selected, loading all plants");
        Debug.Log("Plants type null, count: " + (plants != null ? plants.Count : 0));

        var gameManagerObj = GameObject.FindGameObjectWithTag("GameManager");
        if (gameManagerObj != null)
        {
            var gameManager = gameManagerObj.GetComponent<GameManager>();
            Debug.Log("SceneCounter:" + (gameManager != null ? gameManager.sceneCounter : 0));
            
            if (gameManager != null && gameManager.sceneCounter >= 1)
            {
                Instantiate(Resources.Load<GameObject>("PlantDescription")).GetComponent<UIDocument>().panelSettings.sortingOrder = 2;
            }
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
        // Destroy old plantButtonDataHolder if it exists
        if (plantButtonDataHolder != null)
        {
            #if UNITY_EDITOR
            DestroyImmediate(plantButtonDataHolder);
            #else
            Destroy(plantButtonDataHolder);
            #endif
            plantButtonDataHolder = null;
        }

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
            // Subscribe to theme changes
            ColorThemeManager.Instance.SubscribeToThemeChange(OnThemeChanged);
        }

        //add functionality to settings button
        Button settingsButton = mainMenuRoot.Q<Button>("settings-button");
        if (settingsButton != null)
        {
            settingsButton.clicked += () =>
            {
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlayDefaultButtonSound();
                }
                if (settingsPrefab != null)
                {
                    GameObject settingsMenu = Instantiate(settingsPrefab);
                }
                else
                {
                    Debug.LogError("PopulateMenuTest: settingsPrefab is null, cannot instantiate settings menu.");
                }
            };
        }

        SetupBackButton();

        if (plants == null || plants.Count == 0)
        {
            Debug.LogWarning("PopulateMenuTest: plants list is null or empty in OnEnable.");
            return;
        }

        // populate plant cards based on database
        for (int i = 0; i < plants.Count; i++)
        {
            PlantInfo currentPlant = plants[i];

            // Create a new button from the template
            if (plantCardTemplate == null)
            {
                Debug.LogError("PopulateMenuTest: plantCardTemplate is null, cannot create plant cards.");
                break;
            }

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

            // Plant card labels should always stay white (displayed over image background)
            var plantCardLabels = mainMenuRoot.Query<Label>(className: "card-title").ToList();
            foreach (Label label in plantCardLabels)
            {
                label.style.color = new Color(1f, 1f, 1f, 1f); // White
            }

            // Also set scientific name labels to white
            var scientificLabels = mainMenuRoot.Query<Label>().Where(l => l.name == "ScientificName").ToList();
            foreach (Label label in scientificLabels)
            {
                label.style.color = new Color(1f, 1f, 1f, 1f); // White
            }

            // Also set info labels to white (like "Species:")
            var infoLabels = mainMenuRoot.Query<Label>(className: "info-label").ToList();
            foreach (Label label in infoLabels)
            {
                label.style.color = new Color(1f, 1f, 1f, 1f); // White
            }
        }
    }


    private bool EnsureMainMenuReady()
    {
        if (mainMenu == null)
        {
            var foundMainMenu = GameObject.FindGameObjectWithTag("MainMenu");
            if (foundMainMenu != null)
            {
                mainMenu = foundMainMenu;
            }
            else if (mainMenuPrefab != null)
            {
                mainMenu = Instantiate(mainMenuPrefab);
            }
            else
            {
                Debug.LogWarning("PopulateMenuTest: mainMenu and mainMenuPrefab are both null.");
            }
        }

        if (mainMenu == null)
        {
            Debug.LogError("PopulateMenuTest: mainMenu is not available.");
            return false;
        }

        return true;
    }

    // added function to clear current plant buttons and repopulate with plants of the selected category
    public void ClearAndPopulateMenuWithCategoryPlants(PlantTypes category)
    {
        if (!EnsureMainMenuReady())
        {
            return;
        }

        // Destroy old plantButtonDataHolder before creating a new one
        if (plantButtonDataHolder != null)
        {
            #if UNITY_EDITOR
            DestroyImmediate(plantButtonDataHolder);
            #else
            Destroy(plantButtonDataHolder);
            #endif
            plantButtonDataHolder = null;
        }

        plantButtonDataHolder = Instantiate(Resources.Load<GameObject>("PlantButtonDataHolder"));
        var root = mainMenu.GetComponent<UIDocument>().rootVisualElement;
        mainMenuRoot = root;

        // Apply theme to the newly instantiated mainMenu
        if (ColorThemeManager.Instance != null)
        {
            ColorThemeManager.Instance.ApplyThemeToUIDocument(root);
            Debug.Log("PopulateMenuTest: Applied theme to mainMenu after category selection.");
        }

        var content = root.Q<VisualElement>(contentHandelName);

        if (content == null)
        {
            content = root.Q<VisualElement>("content");
        }

        if (content == null)
        {
            Debug.LogError($"PopulateMenuTest: content container '{contentHandelName}' not found.");
            var allChildren = root.Query<VisualElement>().ToList();
            Debug.Log($"PopulateMenuTest: root has {allChildren.Count} child elements available.");
            return;
        }

        if (plants == null)
        {
            Debug.LogWarning("PopulateMenuTest: plants list was null in ClearAndPopulateMenuWithCategoryPlants; reloading from database.");
            plants = DatabaseManager.Instance?.GetAllPlants();
        }

        if (plants == null)
        {
            Debug.LogError("PopulateMenuTest: plants list is still null; cannot populate category plants.");
            return;
        }

        var selectedCount = plants.Count(p => p.typeID == category.typeID);
        Debug.Log($"PopulateMenuTest: Selected category '{category.typeName}' (ID {category.typeID}), total plants={plants.Count}, matching={selectedCount}");

        content.Clear();
        var dataHolderToDestroy = GameObject.FindGameObjectWithTag("DataHolder");
        if (dataHolderToDestroy != null)
        {
            #if UNITY_EDITOR
            DestroyImmediate(dataHolderToDestroy);
            #else
            Destroy(dataHolderToDestroy);
            #endif
        }
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
                Debug.Log($"PopulateMenuTest: added plant card '{currentPlant.plantName}' ({currentPlant.scientificName}) to content.");

                // Creates a new GameObject holding its own LoadDatabaseInfo script pertaining to the specific plant in the itteration
                // This allows each button to have its own data loader instance
                if (plantButtonDataHolder != null)
                {
                    GameObject dataObj = new GameObject($"PlantData_{currentPlant.plantName}");
                    dataObj.transform.SetParent(plantButtonDataHolder.transform, false);

                    LoadDatabaseInfo dataLoader = dataObj.AddComponent<LoadDatabaseInfo>();
                    dataLoader.plantInfo = currentPlant;

                    //Wire button to its corresponding data loader instance
                    button.clicked += dataLoader.OnClick;
                }
                else
                {
                    Debug.LogWarning($"PopulateMenuTest: plantButtonDataHolder is null, cannot add data loader for {currentPlant.plantName}");
                }
            }
        }

        // Re-wire back button after mainMenu has been set up
        SetupBackButton();
        Debug.Log("PopulateMenuTest: Back button re-wired after category selection.");
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
        if (mainMenu == null)
        {
            Debug.LogWarning("PopulateMenuTest: ClearMainMenuPlants called but mainMenu is null.");
            return;
        }

        var uiDoc = mainMenu.GetComponent<UIDocument>();
        if (uiDoc == null)
        {
            Debug.LogWarning("PopulateMenuTest: ClearMainMenuPlants - mainMenu has no UIDocument.");
            return;
        }

        var root = uiDoc.rootVisualElement;
        if (root == null)
        {
            Debug.LogWarning("PopulateMenuTest: ClearMainMenuPlants - rootVisualElement is null.");
            return;
        }

        var content = root.Q<VisualElement>(contentHandelName);
        if (content != null)
        {
            content.Clear();
        }

        var dataHolder = GameObject.FindGameObjectWithTag("DataHolder");
        if (dataHolder != null)
        {
            #if UNITY_EDITOR
            DestroyImmediate(dataHolder);
            #else
            Destroy(dataHolder);
            #endif
        }

        // Destroy plantButtonDataHolder
        if (plantButtonDataHolder != null)
        {
            #if UNITY_EDITOR
            DestroyImmediate(plantButtonDataHolder);
            #else
            Destroy(plantButtonDataHolder);
            #endif
            plantButtonDataHolder = null;
        }
    }

    private void SetupBackButton()
    {
        if (mainMenuRoot == null)
        {
            Debug.LogWarning("PopulateMenuTest: SetupBackButton called but mainMenuRoot is null.");
            return;
        }

        backButton = mainMenuRoot.Q<Button>("back-button");
        if (backButton == null)
        {
            // fallback path in case the root is different
            var mainMenuDoc = GameObject.FindGameObjectWithTag("MainMenu")?.GetComponent<UIDocument>();
            if (mainMenuDoc != null)
            {
                backButton = mainMenuDoc.rootVisualElement.Q<Button>("back-button");
            }
        }

        if (backButton != null)
        {
            backButton.clicked -= OnBackButtonClicked;
            backButton.clicked += OnBackButtonClicked;
            backButton.RegisterCallback<PointerUpEvent>(evt =>
            {
                Debug.Log("PopulateMenuTest: back-button pointer-up event fired.");
                OnBackButtonClicked();
            });

            backButton.pickingMode = PickingMode.Position;
            backButton.focusable = true;
            backButton.style.display = DisplayStyle.Flex;

            Debug.Log("PopulateMenuTest: back-button found and SetupBackButton attached.");
        }
        else
        {
            Debug.LogWarning("PopulateMenuTest: back-button not found in mainMenuRoot during SetupBackButton.");
        }
    }

    private void OnBackButtonClicked()
    {
        Debug.Log("PopulateMenuTest: OnBackButtonClicked triggered.");

        if (!EnsureMainMenuReady())
        {
            Debug.LogError("PopulateMenuTest: mainMenu not ready during back navigation.");
            return;
        }

        // find categories menu object in case it is available
        var categoriesMenuObj = GameObject.Find("categMenu(Clone)");
        if (categoriesMenuObj == null)
        {
            categoriesMenuObj = categories.CurrentCategoriesMenu;
        }

        Debug.Log($"PopulateMenuTest: categories menu lookup result = {(categoriesMenuObj != null ? categoriesMenuObj.name : "null")}");
        Debug.Log($"PopulateMenuTest: current categories static reference = {(categories.CurrentCategoriesMenu != null ? categories.CurrentCategoriesMenu.name : "null")}");
        Debug.Log($"PopulateMenuTest: current mainMenu object = {(mainMenu != null ? mainMenu.name : "null")}, active={(mainMenu != null ? mainMenu.activeSelf.ToString() : "n/a")} ");

        // Always destroy the old categories menu and recreate fresh
        if (categories.Instance != null)
        {
            Debug.Log("PopulateMenuTest: Calling RecreateMenu to get fresh categories.");
            categories.Instance.RecreateMenu();
        }

        // Now get the newly created menu
        categoriesMenuObj = categories.CurrentCategoriesMenu;
        if (categoriesMenuObj != null)
        {
            categoriesMenuObj.SetActive(true);
            var categUIDoc = categoriesMenuObj.GetComponent<UIDocument>();
            if (categUIDoc != null)
            {
                categUIDoc.panelSettings.sortingOrder = 1;
                Debug.Log("PopulateMenuTest: Recreated categories menu set active and top (sorting 1).");
            }
            else
            {
                Debug.LogWarning("PopulateMenuTest: recreated categories menu object has no UIDocument.");
            }
        }

        if (mainMenu != null)
        {
            var mainUIDoc = mainMenu.GetComponent<UIDocument>();
            if (mainUIDoc != null)
            {
                mainUIDoc.panelSettings.sortingOrder = 0;
                Debug.Log("PopulateMenuTest: main menu set to background (sorting 0).");
            }
            else
            {
                Debug.LogWarning("PopulateMenuTest: mainMenu has no UIDocument.");
            }
        }

        // Clear plants BEFORE deactivating mainMenu so UIDocument is still active
        ClearMainMenuPlants();

        if (mainMenu != null)
        {
            mainMenu.SetActive(false);
            Debug.Log("PopulateMenuTest: mainMenu deactivated on back click.");
        }

        if (categoriesMenuObj == null)
        {
            Debug.LogError("PopulateMenuTest: unable to find categories menu on back click.");
        }

        if (mainMenu == null)
        {
            Debug.LogWarning("PopulateMenuTest: mainMenu is null on back click. Attempting to refresh main menu.");
            EnsureMainMenuReady();
        }

        var gameManager = GameObject.FindGameObjectWithTag("GameManager");
        if (gameManager != null)
        {
            var gm = gameManager.GetComponent<GameManager>();
            if (gm != null)
            {
                gm.sceneCounter += 1;
            }
        }

        SoundManager.Instance.PlayDefaultButtonSound();
    }

    void Update()
    {
        if ((backButton == null || mainMenuRoot == null) && mainMenu != null && mainMenu.activeSelf)
        {
            SetupBackButton();
        }
    }
}
