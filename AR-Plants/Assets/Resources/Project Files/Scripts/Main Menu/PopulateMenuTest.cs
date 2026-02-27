using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using SQLite4Unity3d;
using System.IO;
using System.Linq;


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
    public string typeID { get; set; }

    public float maxSize = 2;
    public float minSize = .3f;

    //public string plantModelName = "Nerium oleander";
    //public string plantModelName = "Monstera";
}

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

        plants = _connection.Table<PlantInfo>().ToList();

        // goes back to previous plant description if going back to main menu from AR scene
        Debug.Log("SceneCounter:" + GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().sceneCounter);
        if (GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().sceneCounter >= 1)
        {
            Instantiate(Resources.Load<GameObject>("PlantDescription"));
        }
        //GameObject tempSettings = Instantiate(settingsPrefab);
        //Destroy(tempSettings);
    }

    public void Start()
    {
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
    }

    void OnEnable()
    {

        // Instantiate UI and data holder prefabs and keep references
        GameObject mainMenu = Instantiate(mainMenuPrefab);
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

        for (int i = 0; i < plants.Count; i++)
        {
            PlantInfo currentPlant = plants[i];

            // Create a new button from the template
            VisualElement newPlantCardInstance = plantCardTemplate.CloneTree();
            Label plantNameLabel = newPlantCardInstance.Q<Label>("PlantName");

            Label scientificNameLabel = newPlantCardInstance.Q<Label>("ScientificName");

            Button button = newPlantCardInstance.Q<Button>("PlantDescriptionButton");


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

}
