using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EnterARMonstera : MonoBehaviour
{
    private SelectedPlantData selectedPlantData;
    [SerializeField] private GameObject selectedPlantDataHandle;
    [SerializeField] private PlantSO plantSOToSet;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        selectedPlantDataHandle = GameObject.FindGameObjectWithTag("SelectedPlantData");
        selectedPlantData = selectedPlantDataHandle.GetComponent<SelectedPlantData>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnEnterAR()
    {
        EnterAR();
    }

    private void EnterAR()
    {
        selectedPlantData.SetPlantSO(plantSOToSet);
        SceneManager.LoadScene("MainScene00");
    }
}
