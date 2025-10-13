using UnityEngine;



[CreateAssetMenu(menuName = "Plants/Plant")]
public class PlantSO : ScriptableObject
{


    [Header("Basic Info")]
    public int plant_id;
    public string plant_name;
    [TextArea] public string description;

    [Header("Plant Classification")]
    public PlantTypeSO plantType;         // corresponds to Type_typeID

    [Header("Parts")]
    public StemTypeSO stemType;      // stemType_stemID
    public LeafTypeSO leafType;      // leafType_leafID
    public RootTypeSO rootType;      // rootType_rootID

    [Header("Visuals")]
    public Sprite icon;              // optional thumbnail
    public GameObject prefab;        // optional 3D model prefab
}