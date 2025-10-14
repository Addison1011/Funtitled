using UnityEngine;


[CreateAssetMenu(menuName = "Plants/Plant Type")]
public class PlantTypeSO : ScriptableObject
{
    public int typeID;
    public string typeName;
    [TextArea] public string description;
}

[CreateAssetMenu(menuName = "Plants/Stem Type")]
public class StemTypeSO : ScriptableObject
{
    public int stemID;
    public string stemName;
    [TextArea] public string description;
}

[CreateAssetMenu(menuName = "Plants/Leaf Type")]
public class LeafTypeSO : ScriptableObject
{
    public int leafID;
    public string leafName;
    [TextArea] public string description;
}

[CreateAssetMenu(menuName = "Plants/Root Type")]
public class RootTypeSO : ScriptableObject
{
    public int rootID;
    public string rootName;
    [TextArea] public string description;
}