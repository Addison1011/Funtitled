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
    //public TMP_Text stemText;
    //public TMP_Text leafText;
    //public TMP_Text flowerText;
    public TMP_Text titleText;
    //public TMP_Text scientificText;
    private PlantInfo plantInfo;


    void Start()
    {
        infoPanel.anchoredPosition = hiddenPosition;
    }

    public void LoadPlantParts(PlantInfo info)
    {
        plantInfo = info;
        titleText.text = plantInfo.plantName;
        //stemText.text = plantInfo.stem;
        //leafText.text = plantInfo.leaf;
        //flowerText.text = plantInfo.flower;
        scientificText.text = plantInfo.scientificName;

        TogglePanel();
    }
    public void TogglePanel()
    {
        //Debug.Log("Info panel reference: " + infoPanel);
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