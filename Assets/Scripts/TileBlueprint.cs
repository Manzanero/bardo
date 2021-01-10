using UnityEngine;

[CreateAssetMenu(fileName = "New Tile", menuName = "Tile Blueprint")]
public class TileBlueprint : ScriptableObject
{
    public string physicMeshResource;
    public float altitude;
    public string graphicMeshResource;
    public string textureResource;
    public bool translucent;
}
