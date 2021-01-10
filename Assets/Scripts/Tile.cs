using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Tile : MonoBehaviour
{
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public MeshCollider meshCollider;

    public TileBlueprint blueprint;
    public Map map;

    private Vector2Int _position;
    private float _rotation;
    private bool _walkable;
    private bool _visionBlocker;
    private string _physicMeshResource;
    private float _altitude;
    private string _graphicMeshResource;
    private string _textureResource;
    private bool _translucent;
    
    // calculated
    private bool _explored;
    private bool _shadow;
    private float _luminosity;
    
    // private Camera _mainCamera;
    private int _tilesLayerMask;
    
    public Vector2Int Position
    {
        get => _position;
        set 
        {
            transform.position = new Vector3(value.x, 0, value.y);
            _position = value;
        }
    }

    public float Rotation
    {
        get => _rotation;
        set 
        {
            transform.eulerAngles = new Vector3(0, value, 0);
            _rotation = value;
        }
    }
    
    public float Altitude
    {
        get => _altitude;
        set => _altitude = value;
    }
    
    public bool Walkable
    {
        get => _walkable;
        set => _walkable = value;
    }
    
    public bool VisionBlocker
    {
        get => _visionBlocker;
        set { 
            // gameObject.layer = value ? map.visionBlockerLayer : map.visionLayer; 
            gameObject.layer = value ? 9 : 8; 
            _visionBlocker = value; 
        } 
    }

    public string GraphicMeshResource
    {
        get => _graphicMeshResource;
        set
        {
            meshFilter.mesh = GameMaster.GetResource<Mesh>(value);
            _graphicMeshResource = value;
        }
    }

    public string PhysicMeshResource
    {
        get => _physicMeshResource;
        set
        {
            meshCollider.sharedMesh = GameMaster.GetResource<Mesh>(value);
            _physicMeshResource = value;
        }
    }
    
    public string TextureResource
    {
        get => _textureResource;
        set
        {
            meshRenderer.material.mainTexture = GameMaster.GetResource<Texture>(value);
            _textureResource = value;
        } 
    }
    
    public bool Translucent
    {
        get => _translucent;
        set 
        {
            meshRenderer.material = value ? map.translucentTileMaterial : map.tileMaterial;
            meshRenderer.material.mainTexture = GameMaster.GetResource<Texture>(_textureResource);
            _translucent = value;
        }
    }
    
    public bool Explored
    {
        get => _explored;
        set 
        {
            meshRenderer.enabled = value;
            _explored = value;
        }
    }
    
    public bool Shadow
    {
        get => _shadow;
        set => _shadow = value;
    }

    public float Luminosity
    {
        get => _luminosity;
        set
        {
            meshRenderer.material.color = new Color(value,value,value);
            _luminosity = value;
        }
    }

    [Serializable]
    public class SerializableTile
    {
        public Vector2Int position;
        public float rotation;
        public float altitude;
        public bool walkable;
        public bool visionBlocker;
        public string graphicMeshResource;
        public string physicMeshResource;
        public string textureResource;
        public bool translucent;
    }

    public SerializableTile Serialize()
    {
        return new SerializableTile
        {
            position = Position,
            rotation = Rotation,
            altitude = Altitude,
            walkable = Walkable,
            visionBlocker = VisionBlocker,
            graphicMeshResource = GraphicMeshResource,
            physicMeshResource = PhysicMeshResource,
            textureResource = TextureResource,
            translucent = Translucent
        };
    }

    public void Deserialize(SerializableTile serializableTile)
    {
        Position = serializableTile.position;
        Rotation = serializableTile.rotation;
        Altitude = serializableTile.altitude;
        Walkable = serializableTile.walkable;
        VisionBlocker = serializableTile.visionBlocker;
        GraphicMeshResource = serializableTile.graphicMeshResource;
        PhysicMeshResource = serializableTile.physicMeshResource;
        TextureResource = serializableTile.textureResource;
        Translucent = serializableTile.translucent;
    }
}