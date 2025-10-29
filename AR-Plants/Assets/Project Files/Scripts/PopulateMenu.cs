using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SQLite4Unity3d;
using System.IO;
using System.Linq;
using Mono.Cecil.Cil;

public class PopulateMenu : MonoBehaviour
{
        [SerializeField] private Transform m_Content;
        [SerializeField] private GameObject m_ButtonPrefab;

        [SerializeField] private string path;
        private int m_NumPlants;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private SQLiteConnection _connection;
        private List<PlantInfo> plants;

        void Awake()
        {
                string dbName = "PlantInfoDB.db";
                string dbPath = Path.Combine(Application.streamingAssetsPath, dbName);

                Debug.Log("DB Path: " + dbPath);

                _connection = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
                Debug.Log("Database opened");

                plants = _connection.Table<PlantInfo>().ToList();

                foreach (var plant in plants)
                {
                        Debug.Log("Plant Part: " + plant.plantName);
                }

        }

        public class PlantInfo
        {
                [PrimaryKey, AutoIncrement]
                public int plantID { get; set; }
                public string plantName { get; set; }
        }
        void Start()
        {
                for (int i = 0; i < plants.Count(); i++)
                {
                        GameObject button = Instantiate(m_ButtonPrefab);

                        //parent the item to the content panel
                        button.transform.SetParent(m_Content);

                        //here will be where the data for each plant will be loaded
                        button.GetComponentInChildren<TMP_Text>().text = plants[i].plantName;
                }
        }
}