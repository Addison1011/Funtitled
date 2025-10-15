using UnityEngine;
using UnityEngine.SceneManagement;


public class SelectedPlantData : MonoBehaviour
{

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
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {

        stemTypeSO = plantSO.stemType;
        leafTypeSO = plantSO.leafType;
        rootTypeSO = plantSO.rootType;
        plantTypeSO = plantSO.plantType;
        //SceneManager.LoadScene("MainScene00");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
