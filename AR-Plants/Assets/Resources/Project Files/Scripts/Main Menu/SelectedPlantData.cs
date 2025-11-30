using UnityEngine;
using UnityEngine.SceneManagement;


public class SelectedPlantData : MonoBehaviour
{
    public static SelectedPlantData Instance;
    [Header("PlantSO Referenced")]

    public PlantInfo plantInfo;

    public PlantPart selectedPart;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //Singleton
    void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

    }


    // Update is called once per frame
    void Update()
    {

    }

}
