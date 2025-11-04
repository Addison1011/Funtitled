using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    public float minSize = .5f;

    //public string plantModelName = "Nerium oleander";
    //public string plantModelName = "Monstera";
}

public class PopulateMenu : MonoBehaviour
{
    [SerializeField] private Transform m_Content;
    [SerializeField] private GameObject m_ButtonPrefab;

    [SerializeField] private string path;
    private int m_NumPlants;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private SQLiteConnection _connection;
    private List<PlantInfo> plants;

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
    }


    void Start()
    {
        Debug.Log(plants[0]);
        for (int i = 0; i < plants.Count(); i++)
        {
            GameObject button = Instantiate(m_ButtonPrefab);

            //parent the item to the content panel
            button.transform.SetParent(m_Content);

            //local copy
            PlantInfo currentPlant = plants[i];

            //set the text on the button
            var text = button.GetComponentInChildren<TMP_Text>();
            if (text != null)
            {
                text.text = currentPlant.plantName;
            }

            //load data onto the button for later use
            var dataLoader = button.GetComponent<LoadDatabaseInfo>();
            if (dataLoader != null)
            {
                dataLoader.plantInfo = currentPlant;
            }

            //get the actual button component within the prefab
            Button uiButton = button.GetComponent<Button>();
            if (uiButton != null && dataLoader != null)
            {
                //register the prefab's handler which will use dataLoader.plantInfo
                uiButton.onClick.AddListener(dataLoader.OnClick);
            }
        }
    }
}
