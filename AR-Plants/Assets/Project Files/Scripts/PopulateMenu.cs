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

        m_NumPlants = Directory.GetFiles(path).Length; //number of plant folders in Prefabs/Plants
        for (int i = 0; i < m_NumPlants; i++)
        {
            GameObject button = Instantiate(m_ButtonPrefab);

            //parent the item to the content panel
            button.transform.SetParent(m_Content);

            //here will be where the data for each plant will be loaded
            button.GetComponentInChildren<TMP_Text>().text= "Plant " + (i + 1);
        }
    }
}
