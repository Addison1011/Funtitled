using UnityEngine;
using UnityEngine.SceneManagement;


public class SelectedPlantData : MonoBehaviour
{
    public static SelectedPlantData Instance;
    [Header("PlantSO Referenced")]
    public PlantSO plantSO;

    public PlantPart selectedPart;
    public StemTypeSO stemTypeSO;
    public LeafTypeSO leafTypeSO;
    public RootTypeSO rootTypeSO;
    public PlantTypeSO plantTypeSO;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
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

    void Start()
    {

        /*stemTypeSO = plantSO.stemType;
        leafTypeSO = plantSO.leafType;
        rootTypeSO = plantSO.rootType;
        plantTypeSO = plantSO.plantType;
        //SceneManager.LoadScene("MainScene00");*/
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ResetSelectedPlantData()
    {
        plantSO = null;
        selectedPart = PlantPart.None;
        stemTypeSO = null;
        leafTypeSO = null;
        rootTypeSO = null;
        plantTypeSO = null;
    }

    public void SetPlantSO(PlantSO newPlantSO)
    {
        plantSO = newPlantSO;
        stemTypeSO = plantSO.stemType;
        leafTypeSO = plantSO.leafType;
        rootTypeSO = plantSO.rootType;
        plantTypeSO = plantSO.plantType;
    }
}
