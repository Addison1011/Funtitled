using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using SQLite4Unity3d;
using System.IO;
using System.Linq;

public class PlantTypes
{
    [PrimaryKey, AutoIncrement]
    public int typeID{ get; set; }
    public string typeName{ get; set; }
}
public class categories : MonoBehaviour
{


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private SQLiteConnection _connection;
    private List<PlantTypes> types;
    [SerializeField] private GameObject categoryButtonDataHolder;

    [SerializeField] private VisualTreeAsset categoryCardTemplate;
    [SerializeField] private GameObject categMenuPrefab;
    [SerializeField] private GameObject settingsPrefab;
    [SerializeField] private string contentHandleName;
    [SerializeField] private GameObject MainMenuPopulator;

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

        types = _connection.Table<PlantTypes>().ToList();

        // goes back to previous plant description if going back to main menu from AR scene
        //TODO: fix this going back scenario
        Debug.Log("SceneCounter:" + GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().sceneCounter);
        if (GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().sceneCounter >= 1)
        {
            Instantiate(Resources.Load<GameObject>("MainMenuPopulator"));
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
        
        GameObject categMenu = Instantiate(categMenuPrefab);
        categoryButtonDataHolder = Instantiate(Resources.Load<GameObject>("categoryButtonDataHolder"));

        // Get the root and content container
        var root = categMenu.GetComponent<UIDocument>().rootVisualElement;
        var content = root.Q<VisualElement>(contentHandleName);

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

        for (int i = 0; i < types.Count; i++)
        {
            PlantTypes currentType = types[i];

            // Create a new button from the template
            VisualElement newCategoryInstance = categoryCardTemplate.CloneTree();
            Label categoryNameLabel = newCategoryInstance.Q<Label>("CategoryName");
            Button button = newCategoryInstance.Q<Button>("CardButton");

            
            categoryNameLabel.text = currentType.typeName;

            // Add the button to the content container
            content.Add(newCategoryInstance);
            Debug.Log(currentType.typeName);

            // Creates a new GameObject holding its own LoadDatabaseInfo script pertaining to the specific plant in the itteration
            // This allows each button to have its own data loader instance

            //TODO: REVISIT!!
            GameObject dataObj = new GameObject($"TypeData_{currentType.typeName}");
            dataObj.transform.SetParent(categoryButtonDataHolder.transform, false);

            LoadCategoriesMenu dataLoader = dataObj.AddComponent<LoadCategoriesMenu>();
            dataLoader.plantTypes = currentType;

            //Wire button to its corresponding data loader instance
            button.clicked += dataLoader.OnClick;
        }
    }

}
