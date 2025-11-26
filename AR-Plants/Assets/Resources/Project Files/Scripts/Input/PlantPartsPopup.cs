using UnityEngine;

public class PlantPartsPopup : MonoBehaviour
{
    public PopupToggleManager popup;

    void Start()
    {
        //get the info of the plant clicked
        PlantInfo info = GameObject.FindGameObjectWithTag("SelectedPlantData").GetComponent<SelectedPlantData>().plantInfo;
        popup.LoadPlantParts(info);
    }
}