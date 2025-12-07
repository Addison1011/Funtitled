using UnityEngine;

public class SpawnPlant : MonoBehaviour
{

    private SelectedPlantData selectedPlantData;
    private GameObject selectedPlantModel;
    PlantInfo plantInfo;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        selectedPlantData = GameObject.FindGameObjectWithTag("SelectedPlantData").GetComponent<SelectedPlantData>();

        selectedPlantModel = Instantiate(Resources.Load<GameObject>(selectedPlantData.plantInfo.scientificName));
    }

    // Update is called once per frame
    void Update()
    {

    }
}
