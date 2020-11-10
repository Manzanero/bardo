using UnityEngine;

[CreateAssetMenu(fileName = "New Tile", menuName = "Tile Blueprint")]
public class TileBlueprint : ScriptableObject
{
    public string folder;
    public string description;
    public Mesh graphicMesh;
    public Mesh physicMesh;
    public Material material;
    public float altitude;
    public bool walkable;
    public bool visionBlocker;
}
