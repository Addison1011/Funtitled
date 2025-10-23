using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.IO;

public class PopulateMenu : MonoBehaviour
{
        [SerializeField] private Transform m_Content;
        [SerializeField] private GameObject m_ButtonPrefab;

        [SerializeField] private string path;
        private int m_NumPlants;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
                FileInfo[] plantNames = new DirectoryInfo(path).GetFiles(); //array of files in Prefabs/Plants
                m_NumPlants = plantNames.Length; //number of plant folders in Prefabs/Plants
                for (int i = 0; i < m_NumPlants; i++)
                {
                        GameObject button = Instantiate(m_ButtonPrefab);

                        //parent the item to the content panel
                        button.transform.SetParent(m_Content);

                        //here will be where the data for each plant will be loaded
                        string name = plantNames[i].Name;
                        name = name.Substring(0, name.Length - 5); //trim off the file extension of .meta
                        button.GetComponentInChildren<TMP_Text>().text = name;
                }
        }
}
