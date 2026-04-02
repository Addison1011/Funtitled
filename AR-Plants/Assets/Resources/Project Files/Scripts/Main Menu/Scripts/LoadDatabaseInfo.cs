using UnityEngine;
using UnityEngine.SceneManagement;
public class LoadDatabaseInfo : MonoBehaviour
{
    public PlantInfo plantInfo;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Loaded Plant Info: " + plantInfo.plantName + " with ID: " + plantInfo.plantID);
    }

    //script to instantiate the plant description -> info sent to plantdescriptionui page
    public void OnClick()
    {
        SoundManager.Instance.PlayDefaultButtonSound();
        SelectedPlantData data = GameObject.FindGameObjectWithTag("SelectedPlantData").GetComponent<SelectedPlantData>();
        data.plantInfo = this.plantInfo;
        Debug.Log("Button clicked for " + data.plantInfo.plantName + " with ID: " + data.plantInfo.plantID + ". " + data.plantInfo.scientificName);
        Instantiate(Resources.Load<GameObject>("PlantDescription"));

        Debug.Log(data.plantInfo.scientificName);
    }
    // Update is called once per frame
    void Update()
    {

    }
}
