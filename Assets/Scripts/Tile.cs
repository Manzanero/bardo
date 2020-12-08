using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

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
    private bool _explored;
    
    // Blueprint
    private float _altitude;
    private bool _walkable;
    private bool _visionBlocker;
    
    // calculated
    private bool _shadow;
    private bool _vision;
    private float _luminosity;
    
    // private Camera _mainCamera;
    private int _tilesLayerMask;
    
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
    
    public TileBlueprint Blueprint
    {
        get => blueprint;
        set
        {
            // graphics
            meshFilter.mesh = value.graphicMesh;
            meshRenderer.material = value.material;
            // physics
            meshCollider.sharedMesh = value.physicMesh;
            VisionBlocker = value.visionBlocker;
            Altitude = value.altitude;
            Walkable = value.walkable;
            
            blueprint = value;
        }
    }

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

    public bool Shadow
    {
        get => _shadow;
        set => _shadow = value;
    }

    public bool Vision
    {
        get => _vision;
        set => _vision = value;
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

    public float Luminosity
    {
        get => _luminosity;
        set
        {
            meshRenderer.material.color = new Color(value,value,value);
            _luminosity = value;
        }
    }

    // private void Start()
    // {
    //     _mainCamera = Camera.main;     
    //     // _tilesLayerMask = LayerMask.GetMask("Tiles", "Vision Blocker");
    // }

    // private void OnMouseOver()
    // {
    //     if (GameMaster.instance.mouseOverUi) 
    //         return;
    //     
    //     var ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
    //     if (!Physics.Raycast(ray, out var rayCastHit, 100f, _tilesLayerMask)) 
    //         return;
    //     map.mousePosition = rayCastHit.point;
    //     map.mouseTile = this;
    // }
    //
    // private void OnMouseExit()
    // {
    //     map.mouseTile = null;
    // }
    
    [Serializable]
    public class SerializableTile
    {
        public string blueprint;
        public Vector2Int position;
        public float rotation;
        public bool explored;
    }

    public SerializableTile Serialize()
    {
        var tileObject = new SerializableTile
        {
            blueprint = Blueprint.folder + "/" + Blueprint.name,
            position = Position,
            rotation = Rotation,
            explored = Explored
        };
        return tileObject;
    }

    public void Deserialize(SerializableTile serializableTile)
    {
        Blueprint = GetTileBlueprint(serializableTile.blueprint);
        Position = serializableTile.position;
        Rotation = serializableTile.rotation;
        Explored = serializableTile.explored;
    }

    private static TileBlueprint GetTileBlueprint(string path)
    {
        return Resources.LoadAll($"Blueprints/Tiles/{path}", 
            typeof(TileBlueprint)).Cast<TileBlueprint>().ToArray()[0];
    }
}