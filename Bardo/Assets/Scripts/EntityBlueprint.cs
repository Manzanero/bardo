using UnityEngine;

[CreateAssetMenu(fileName = "New Entity", menuName = "Entity Blueprint")]
public class EntityBlueprint : ScriptableObject
{
    public string folder;
    
    [Header("Entity Base")] 
    public bool hasBase;
    public float baseSize;
    public Material baseMaterial;
    public bool hasBaseImage;
    public Sprite baseImage;
    
    [Header("Entity Body")] 
    public bool hasBody;
    public float bodySize;
    public Mesh bodyMesh;
    public float scaleCorrection;
    public Material bodyMaterial;
    
    [Header("Living properties")] 
    public bool hasName;
    public bool hasHealth;
    public bool hasStamina;
    public bool hasMana;
    public bool hasVision;
    
    [Header("Light properties")] 
    public bool hasLight;
    public float lightRange;
    
    [Header("Aura properties")] 
    public bool hasAura;
    public float auraRange;
    public Material auraMaterial;
}
