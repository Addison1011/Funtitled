using UnityEngine;
using SQLite4Unity3d;
using System.IO;
using System.Collections.Generic;
using System.Linq;
public class DatabaseManager : MonoBehaviour
{
    private SQLiteConnection _connection;

    void Awake()
    {
        string dbName = "PlantInfoDB.db";
        string dbPath = Path.Combine(Application.streamingAssetsPath, dbName);

        Debug.Log("DB Path: " + dbPath);

        _connection = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
        Debug.Log("Database opened");

        var plantsInfo = _connection.Table<PlantInfo>().ToList();

        foreach (var plant in plantsInfo)
        {
            Debug.Log("Plant Part: " + plant.plantName);
        }
    }

    public class PlantInfo
    {
        [PrimaryKey, AutoIncrement]
        public int plantID { get; set; }
        public string plantName { get; set; }
        public string plantDesc { get; set; }
        public string typeID { get; set; }
    }
}
    
