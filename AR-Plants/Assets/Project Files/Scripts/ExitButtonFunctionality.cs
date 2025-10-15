using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;

public class ExitButtonFunctionality : MonoBehaviour
{
    public ARSession arSession; // Assign this in the Inspector

    private SelectedPlantData selectedPlantData;
    [SerializeField] private GameObject selectedPlantDataHandle;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        selectedPlantDataHandle = GameObject.FindGameObjectWithTag("SelectedPlantData");
    }

    // Update is called once per frame
    void Update()
    {

    }


    public void EndARSession()
    {
        if (arSession != null)
        {
            arSession.enabled = false;
            Debug.Log("AR Session ended.");
        }
    }


    public void OnExitButtonPressed()
    {
        //EndARSession();
        ReturnToMainMenu();
    }

    private void ReturnToMainMenu()
    {

        selectedPlantData = selectedPlantDataHandle.GetComponent<SelectedPlantData>();
        selectedPlantData.ResetSelectedPlantData();
        SceneManager.LoadScene("Menu");
    }
}
