using System.Collections;
using TMPro;
using UnityEngine;

//manages the toggle canvas; gets and displays the selected plant info
public class PopupToggleManager : MonoBehaviour
{
    [Header("PanelMovement")]
    public RectTransform infoPanel;
    public Vector2 hiddenPosition;
    public Vector2 shownPosition;
    private bool isVisible = false;
    public float slidespeed = 500f;

    [Header("TextElements")]
    public TMP_Text scientificText;
    //public TMP_Text partText;
    //public TMP_Text partDescText;
    public TMP_Text partName;
    public TMP_Text partDescription;
    public TMP_Text titleText;
    private PlantInfo plantInfo;


    void Start()
    {
        infoPanel.anchoredPosition = hiddenPosition;
        plantInfo = GameObject.FindGameObjectWithTag("SelectedPlantData").GetComponent<SelectedPlantData>().plantInfo;
    }

//function to display the general info of the plant on the panel
    public void LoadPlantParts(PlantInfo info)
    {
        plantInfo = info;
        titleText.text = plantInfo.plantName;
        scientificText.text = plantInfo.scientificName;

        TogglePanel();
    }
//function to display the info of the part clicked
    public void DisplayPartInfo(PlantPart part)
    {
        //string partName = "";
        //string partDescription = "";
        Debug.Log("got here!");
        switch (part)
        {
            case PlantPart.Flower:
                partName.text = "Flower";
                partDescription.text = plantInfo.flower;
                break;
            case PlantPart.Leaf:
                partName.text = "Leaf";
                partDescription.text = plantInfo.leaf;
                break;
            case PlantPart.Stem:
                partName.text = "Stem";
                partDescription.text = plantInfo.stem;
                Debug.Log("This is stem: " + partName);
                break;
            default:
                partName.text = "";
                partDescription.text = "";
                break;
        }
        //partText.text = partName;
        //partDescText.text = partDescription;

        if (!isVisible)
            TogglePanel();
    }
    public void TogglePanel()
    {
        Debug.Log("Info panel reference: " + infoPanel);
        Debug.Log("Toggling panel. New state: " + isVisible);
        Debug.Log("Plant name: " + plantInfo.plantName);
        Debug.Log("Scientific name: " + plantInfo.scientificName);
        isVisible = !isVisible;
        StopAllCoroutines();
        StartCoroutine(SlidePanel(isVisible ? shownPosition : hiddenPosition));
    }

    private System.Collections.IEnumerator SlidePanel(Vector2 target)
    {
        while (Vector2.Distance(infoPanel.anchoredPosition, target) > 0.1f)
        {
            infoPanel.anchoredPosition = Vector2.Lerp(infoPanel.anchoredPosition, target, Time.deltaTime * 10f);
            yield return null;
        }
        infoPanel.anchoredPosition = target;
        Debug.Log("Arrived at: " + infoPanel.anchoredPosition);
    }

}