using UnityEngine;

public class LoadDatabaseInfo : MonoBehaviour
{
    public PlantInfo plantInfo;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Loaded Plant Info: " + plantInfo.plantName + " with ID: " + plantInfo.plantID);
    }

    public void OnClick()
    {
        SelectedPlantData data = GameObject.FindGameObjectWithTag("SelectedPlantData").GetComponent<SelectedPlantData>();
        data.plantInfo = plantInfo;
        Debug.Log("Button clicked for " + data.plantInfo.plantName + " with ID: " + data.plantInfo.plantID);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
