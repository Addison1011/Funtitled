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
        Debug.Log("Button clicked for " + plantInfo.plantName + " with ID: " + plantInfo.plantID);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
