using UnityEngine;
using SQLite4Unity3d;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance { get; private set; }

    private SQLiteConnection _connection;
    private const string DbName = "PlantInfoDB.db";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        string persistentPath = Path.Combine(Application.persistentDataPath, DbName);

#if UNITY_ANDROID && !UNITY_EDITOR
        string srcPath = Path.Combine(Application.streamingAssetsPath, DbName);

        using (UnityEngine.Networking.UnityWebRequest req = UnityEngine.Networking.UnityWebRequest.Get(srcPath))
        {
            var op = req.SendWebRequest();
            while (!op.isDone) { }

            if (req.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to copy DB from StreamingAssets: " + req.error);
                return;
            }

            File.WriteAllBytes(persistentPath, req.downloadHandler.data);
            Debug.Log("Testing mode: Android DB overwritten at " + persistentPath);
        }
#else
        string srcPath = Path.Combine(Application.streamingAssetsPath, DbName);
        File.Copy(srcPath, persistentPath, true);
        Debug.Log("Testing mode: Desktop DB copied to " + persistentPath);
#endif

        _connection = new SQLiteConnection(
            persistentPath,
            SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create
        );
    }

    public List<PlantInfo> GetAllPlants()
    {
        return _connection.Table<PlantInfo>().ToList();
    }

    public List<PlantTypes> GetAllPlantTypes()
    {
        return _connection.Table<PlantTypes>().ToList();
    }

    private void OnDestroy()
    {
        if (_connection != null)
        {
            _connection.Close();
            _connection = null;
        }
    }
}