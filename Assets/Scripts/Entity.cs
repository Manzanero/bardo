using System;
using System.Collections.Generic;
using System.Linq;
using OutlineEffect.OutlineEffect;
using UnityEngine;
using UnityEngine.UI;

public class Entity : MonoBehaviour
{
    public MeshRenderer baseMeshRenderer;
    public MeshCollider baseMeshCollider;
    public MeshFilter bodyMeshFilter;
    public MeshRenderer bodyMeshRenderer;
    public MeshCollider bodyMeshCollider;
    public Canvas nameCanvas;
    public Text nameText;
    public Canvas barsCanvas;
    public GameObject healthBar;
    public Image healthImage;
    public Text healthText;
    public GameObject staminaBar;
    public Image staminaImage;
    public Text staminaText;
    public GameObject manaBar;
    public Image manaImage;
    public Text manaText;
    public Draggable draggableBase;
    public Draggable draggableBody;
    public OutlineMesh baseOutline;
    
    public EntityBlueprint blueprint;
    public Map map;
    public Tile tile;
    public string id;
    public List<Tile> tilesInVision;
    public List<Tile> tilesInLight;
    public List<Aura> auras;

    #region ui
    
    private bool _hasName;
    private bool _hasInitiative;
    private float _initiative;
    private bool _hasHealth;
    private float _health;
    private float _maxHealth = 100;
    private bool _hasStamina;
    private float _stamina;
    private float _maxStamina = 100;
    private bool _hasMana;
    private float _mana;
    private float _maxMana = 100;
    private bool _hasVision;
    private bool _hasShadowVision;
    private float _shadowVisionRange;
    private bool _hasDarkVision;
    private bool _hasLight;
    private float _lightRange;
    
    public bool HasName
    {
        get => _hasName;
        set { nameCanvas.gameObject.SetActive(value); _hasName = value; }
    }

    public string Name
    {
        get => name; 
        set { nameText.text = value; name = value; }
    }
    
    public bool HasInitiative { get => _hasInitiative; set => _hasInitiative = value; }
    
    public float Initiative { get => _initiative; set => _initiative = value; }

    public bool HasHealth
    {
        get => _hasHealth;
        set { healthBar.gameObject.SetActive(value); _hasHealth = value; }
    }

    public bool HasStamina
    {
        get => _hasStamina;
        set { staminaBar.gameObject.SetActive(value); _hasStamina = value; }
    }

    public bool HasMana
    {
        get => _hasMana;
        set { manaBar.gameObject.SetActive(value); _hasMana = value; }
    }
    
    public float Health
    {
        get => _health;
        set
        {
            healthImage.fillAmount = value / _maxHealth;
            healthText.text = $"{value} / {_maxHealth}";
            _health = value;
        }
    }
    
    public float MaxHealth
    {
        get => _maxHealth;
        set
        {
            healthImage.fillAmount = _health / value;
            healthText.text = $"{_health} / {value}";
            _maxHealth = value;
        }
    }
    
    public float Stamina
    {
        get => _stamina;
        set
        {
            staminaImage.fillAmount = value / _maxStamina;
            staminaText.text = $"{value} / {_maxStamina}";
            _stamina = value;
        }
    }
    
    public float MaxStamina
    {
        get => _maxStamina;
        set
        {
            staminaImage.fillAmount = _stamina / value;
            staminaText.text = $"{_stamina} / {value}";
            _maxStamina = value;
        }
    }
    
    public float Mana
    {
        get => _mana;
        set
        {
            manaImage.fillAmount = value / _maxMana;
            manaText.text = $"{value}/{_maxMana}";
            _mana = value;
        }
    }

    public float MaxMana
    {
        get => _maxMana;
        set
        {
            manaImage.fillAmount = _mana / value;
            manaText.text = $"{_mana} / {value}";
            _maxMana = value;
        }
    }
    
    public bool HasVision
    {
        get => _hasVision;
        set { _hasVision = value; _resetVision = true; }
    }

    public bool HasShadowVision
    {
        get => _hasShadowVision;
        set { _hasShadowVision = value; _resetVision = true; }
    }

    public float ShadowVisionRange
    {
        get => _shadowVisionRange;
        set { _shadowVisionRange = value; _resetVision = true; }
    }

    public bool HasDarkVision
    {
        get => _hasDarkVision;
        set { _hasDarkVision = value; _resetVision = true; }
    }

    public bool HasLight
    {
        get => _hasLight;
        set { _hasLight = value; _resetVision = true; }
    }

    public float LightRange
    {
        get => _lightRange;
        set { _lightRange = value; _resetVision = true; }
    }
    
    #endregion

    #region token
    
    private bool _hasBase;
    private float _baseSize;
    private Color _baseColor;
    private bool _hasBaseImage;
    private string _baseImageResource;
    private bool _hasBody = true;
    private float _bodySize = 1f;
    private string _bodyMeshResource;
    private float _scaleCorrection = 1f;
    private string _bodyMaterialResource;

    public Vector2Int Position
    {
        get => tile.Position;
    }

    public Vector2 TileOffset
    {
        get
        {
            var pos = transform.position;
            return new Vector2(pos.x - Mathf.Round(pos.x), pos.z - Mathf.Round(pos.z));
        } 
    }

    public float Rotation
    {
        get => transform.localEulerAngles.y;
        set => transform.localEulerAngles = new Vector3(0, value, 0);
    }

    public bool HasBase
    {
        get => _hasBase;
        set
        {
            var position = transform.position;
            baseMeshRenderer.transform.position = position + 0.05f * Vector3.up;
            bodyMeshRenderer.transform.position = value ? position + 0.1f * Vector3.up : position;
            baseMeshRenderer.gameObject.SetActive(value);
            _hasBase = value;
        }
    }

    public float BaseSize
    {
        get => _baseSize;
        set
        {
            var t = baseMeshRenderer.transform;
            t.localScale = new Vector3(value, t.localScale.y, value);
            _baseSize = value;
        }
    }

    public Color BaseColor
    {
        get => _baseColor;
        set => _baseColor = value;
    }

    public bool HasBaseImage
    {
        get => _hasBaseImage;
        set => _hasBaseImage = value;
    }

    public string BaseImageResource
    {
        get => _baseImageResource;
        set => _baseImageResource = value;
    }

    public bool HasBody
    {
        get => _hasBody;
        set { bodyMeshRenderer.gameObject.SetActive(value); _hasBody = value; }
    }

    public float BodySize
    {
        get => _bodySize;
        set => _bodySize = value;
    }

    public string BodyMeshResource
    {
        get => _bodyMeshResource;
        set
        {
            // bodyMeshFilter.mesh = value; 
            _bodyMeshResource = value;
        }
    }

    public float ScaleCorrection
    {
        get => _scaleCorrection;
        set
        {
            bodyMeshRenderer.transform.localScale = Vector3.one * value;
            _scaleCorrection = value;
        }
    }

    public string BodyMaterialResource
    {
        get => _bodyMaterialResource;
        set { _bodyMaterialResource = value; }
    }
    
    #endregion 

    #region permissions
    
    private bool _sharedName = false;
    private bool _sharedPosition = true;
    private bool _sharedVision = false;
    private bool _sharedControl = false;
    private bool _sharedHealth = false;
    private bool _sharedStamina = false;
    private bool _sharedMana = false;
    
    public bool SharedName
    {
        get => _sharedName;
        set => _sharedName = value;
    }
    
    public bool SharedPosition
    {
        get => _sharedPosition;
        set { _sharedPosition = value; _resetVision = true; }
    }

    public bool SharedVision
    {
        get => _sharedVision;
        set { _sharedVision = value; _resetVision = true; }
    }

    public bool SharedControl
    {
        get => _sharedControl; 
        set { _sharedControl = value; _resetVision = true; }
    }

    public bool SharedHealth { get => _sharedHealth; set => _sharedHealth = value; }

    public bool SharedStamina { get => _sharedStamina; set => _sharedStamina = value; }

    public bool SharedMana { get => _sharedMana; set => _sharedMana = value; }

    public void RefreshSharedProperties()
    {
        if (GameMaster.master)
        {
            SharedName = true;
            SharedPosition = true;
            SharedVision = true;
            SharedControl = true;
            SharedHealth = true;
            SharedStamina = true;
            SharedMana = true;
            return;
        }
        
        Debug.Log(map.properties.Count);
        Debug.Log(map.properties[0].value);
        
        foreach (var property in map.properties.Where(x => x.value == id))
        {
            switch (property.name)
            {
                case "SHARED_NAME":       SharedName = true;      break;
                case "UNSHARED_NAME":     SharedName = false;     break;
                case "SHARED_POSITION":   SharedPosition = true;  break;
                case "UNSHARED_POSITION": SharedPosition = false; break;
                case "SHARED_VISION":     SharedVision = true;    break;
                case "UNSHARED_VISION":   SharedVision = false;   break;
                case "SHARED_CONTROL":    SharedControl = true;   break;
                case "UNSHARED_CONTROL":  SharedControl = false;  break;
                case "SHARED_HEALTH":     SharedHealth = true;    break;
                case "UNSHARED_HEALTH":   SharedHealth = false;   break;
                case "SHARED_STAMINA":    SharedStamina = true;   break;
                case "UNSHARED_STAMINA":  SharedStamina = false;  break;
                case "SHARED_MANA":       SharedMana = true;      break;
                case "UNSHARED_MANA":     SharedMana = false;     break;
            }
        }
    }
    
    #endregion

    private Camera _mainCamera;
    private Tile _cachedTile;
    private bool _inMovement;
    private bool _resetVision;

    private void Start()
    {
        _mainCamera = Camera.main;
    }

    #region Update
    
    private void Update()
    {
        if (ReferenceEquals(map, null))
            return;
        if (ReferenceEquals(tile, null))
        {
            var position = transform.position;
            tile = map.Tile(new Vector2Int((int) Mathf.Round(position.x), (int) Mathf.Round(position.z)));
        }
        
        UpdateSelection();
        UpdateUi();
        UpdatePosition();
        UpdateVision();
        UpdateLuminosity();
    }

    private void UpdateSelection()
    {
        if (Clicked)
            map.selectedEntities = new List<Entity>{this};
    }

    private void UpdateUi()
    {
        if (!HasHealth && !HasStamina && !HasMana && !HasName)
            return;

        barsCanvas.gameObject.SetActive(tile.Explored && SharedPosition);
        nameCanvas.gameObject.SetActive(tile.Explored && SharedPosition);

        var thisTransform = transform;
        var thisTransformPosition = thisTransform.position;
        var barsCanvasTransform = barsCanvas.transform;
        var nameCanvasTransform = nameCanvas.transform;
        var mainCameraTransform = _mainCamera.transform;
        var mainCameraPosition = mainCameraTransform.position;
        var mainCameraRotation = mainCameraTransform.rotation;
        barsCanvasTransform.LookAt(mainCameraPosition);
        nameCanvasTransform.LookAt(mainCameraPosition);
        barsCanvasTransform.rotation = mainCameraRotation;
        nameCanvasTransform.rotation = mainCameraRotation;
        nameCanvas.gameObject.SetActive(HasName && SharedName && tile.Explored);
        healthBar.SetActive(HasHealth && SharedHealth && tile.Explored);
        staminaBar.SetActive(HasStamina && SharedStamina && tile.Explored);
        manaBar.SetActive(HasMana && SharedMana && tile.Explored);
        var cameraForward = mainCameraTransform.forward;
        var screenForward = new Vector3(cameraForward.x, 0, cameraForward.z);
        screenForward = screenForward.magnitude < 0.001f ? mainCameraTransform.up : screenForward.normalized;
        if (!HasBody)
            barsCanvasTransform.position = thisTransformPosition + 0.4f * screenForward + 0.1f * Vector3.up;
        else
            barsCanvasTransform.position = thisTransformPosition + 0.2f * screenForward + BodySize * Vector3.up;
        nameCanvasTransform.position = thisTransformPosition - 0.4f * screenForward;
        
        // mouse interaction
        if (map.selectedEntities.Contains(this))
        {
            // var newScale = 0.003f * Vector3.one;
            // nameCanvasTransform.localScale = newScale;
            // barsCanvasTransform.localScale = newScale;
            baseOutline.enabled = true;
        }
        else
        {
            // var newScale = 0.001f * Vector3.one;
            // nameCanvasTransform.localScale = newScale;
            // barsCanvasTransform.localScale = newScale;
            baseOutline.enabled = false;
        }
    }

    private void UpdatePosition()
    {
        if (!SharedControl) 
            return;
        
        if (Dragged)
        {
            _inMovement = true;
            if (!map.mouseTile || !map.mouseTile.Walkable) 
                return;
            
            tile = map.mouseTile;
            ColliderEnabled = false;
            transform.position = new Vector3(map.mousePosition.x, tile.Altitude + 0.2f, map.mousePosition.z);
            if (tile == _cachedTile)
                return;
            
            _cachedTile = tile;
            _resetVision = true;
        }
        else
        {
            ColliderEnabled = true;         
            transform.position = new Vector3(tile.Position.x, tile.Altitude, tile.Position.y);

            if (!_inMovement) 
                return;
            
            _inMovement = false;
            GameMaster.instance.RegisterAction(new Action {
                name = GameMaster.ActionNames.ChangeEntity,
                map = map.mapId,
                entities = new List<SerializableEntity>{Serialize()}
            });
        }
    }

    private void UpdateVision()
    {
        if (!_resetVision)
            return;
        _resetVision = false;
        
        if (HasVision && (SharedControl || SharedVision))
            tilesInVision = map.TilesInRange(tile.Position, 50);
        else if (!HasVision && SharedControl)
            tilesInVision = new List<Tile> { map.Tile(tile.Position) };
        else
            tilesInVision = new List<Tile>();
        
        if (HasLight)
            tilesInLight = map.TilesInRange(tile.Position, LightRange);
        else if (HasVision && SharedControl)
            tilesInLight = new List<Tile> { map.Tile(tile.Position) };
        else
            tilesInLight = new List<Tile>();
    }

    private void UpdateLuminosity()
    {
        if (SharedPosition)
        {
            baseMeshRenderer.gameObject.SetActive(HasBase && (tile.Explored || SharedControl));
            bodyMeshRenderer.gameObject.SetActive(HasBody && (tile.Explored || SharedControl));
        }
        else
        {
            baseMeshRenderer.gameObject.SetActive(false);
            bodyMeshRenderer.gameObject.SetActive(false);
            return;
        }
        var tl = tile.Luminosity;
        var material = baseMeshRenderer.material;
        material.color = new Color(BaseColor.r * tl, BaseColor.g * tl, BaseColor.b * tl);
        bodyMeshRenderer.material.color = new Color(tl,tl,tl);
    }
    #endregion
    
    private bool Dragged => draggableBody.Dragged || draggableBase.Dragged;
    
    private bool MouseOver => draggableBody.MouseOver || draggableBase.MouseOver;
    
    private bool Clicked => MouseOver && Input.GetMouseButtonDown(0);

    private bool ColliderEnabled { set => (baseMeshCollider.enabled, bodyMeshCollider.enabled) = (value, value); }

    #region Serialization
    
    [Serializable]
    public class SerializableEntity
    {
        public string id;
        public bool hasName;
        public string name;
        public bool hasInitiative;
        public float initiative;
        public bool hasHealth;
        public float health;
        public float maxHealth;
        public bool hasStamina;
        public float stamina;
        public float maxStamina;
        public bool hasMana;
        public float mana;
        public float maxMana;
        public bool hasVision;
        public bool hasShadowVision;
        public float shadowVisionRange;
        public bool hasDarkVision;
        public bool hasLight;
        public float lightRange;
        
        public Vector2Int position;
        public Vector2 tileOffset;
        public float rotation;
        public bool hasBase;
        public float baseSize;
        public Color baseColor;
        public bool hasBaseImage;
        public string baseImageResource;
        public bool hasBody;
        public float bodySize;
        public string bodyMeshResource;
        public float scaleCorrection;
        public string bodyMaterialResource;
    }
    

    public SerializableEntity Serialize()
    {
        var tileObject = new SerializableEntity
        {
            id = id,
            hasName = HasName,
            name = name,
            hasInitiative = HasInitiative,
            initiative = Initiative,
            hasHealth = HasHealth,
            health = Health,
            maxHealth = MaxHealth,
            hasStamina = HasStamina,
            stamina = Stamina,
            maxStamina = MaxStamina,
            hasMana = HasMana,
            mana = Mana,
            maxMana = MaxMana,
            hasVision = HasVision,
            hasShadowVision = HasShadowVision,
            shadowVisionRange = ShadowVisionRange,
            hasDarkVision = HasDarkVision,
            hasLight = HasLight,
            lightRange = LightRange,
            
            position = Position,
            tileOffset = TileOffset,
            rotation = Rotation,
            hasBase = HasBase,
            baseSize = BaseSize,
            baseColor = BaseColor,
            hasBaseImage = HasBaseImage,
            baseImageResource = BaseImageResource,
            hasBody = HasBody,
            bodySize = BodySize,
            bodyMeshResource = BodyMeshResource,
            scaleCorrection = ScaleCorrection,
            bodyMaterialResource = BodyMaterialResource
        };
        return tileObject;
    }

    public void Deserialize(SerializableEntity serializableEntity)
    {
        id = serializableEntity.id;
        HasName = serializableEntity.hasName;
        Name = serializableEntity.name;
        HasInitiative = serializableEntity.hasInitiative;
        Initiative = serializableEntity.initiative;
        HasHealth = serializableEntity.hasHealth;
        Health = serializableEntity.health;
        MaxHealth = serializableEntity.maxHealth;
        HasStamina = serializableEntity.hasStamina;
        Stamina = serializableEntity.stamina;
        MaxStamina = serializableEntity.maxStamina;
        HasMana = serializableEntity.hasMana;
        Mana = serializableEntity.mana;
        MaxMana = serializableEntity.maxMana;
        HasVision = serializableEntity.hasVision;
        HasShadowVision = serializableEntity.hasShadowVision;
        ShadowVisionRange = serializableEntity.shadowVisionRange;
        HasDarkVision = serializableEntity.hasDarkVision;
        HasLight = serializableEntity.hasLight;
        LightRange = serializableEntity.lightRange;
        Rotation = serializableEntity.rotation;
        HasBase = serializableEntity.hasBase;
        BaseSize = serializableEntity.baseSize;
        BaseColor = serializableEntity.baseColor;
        HasBaseImage = serializableEntity.hasBaseImage;
        BaseImageResource = serializableEntity.baseImageResource;
        HasBody = serializableEntity.hasBody;
        BodySize = serializableEntity.bodySize;
        BodyMeshResource = serializableEntity.bodyMeshResource;
        ScaleCorrection = serializableEntity.scaleCorrection;
        BodyMaterialResource = serializableEntity.bodyMaterialResource;
        
        tile = map.Tile(serializableEntity.position);
        transform.position = new Vector3(
            tile.Position.x + serializableEntity.tileOffset.x, tile.Altitude, 
            tile.Position.y + serializableEntity.tileOffset.y); 
    }

    private static T GetResource<T>(string path)
    {
        return Resources.LoadAll(path, typeof(T)).Cast<T>().ToArray()[0];
    }
    
    #endregion
}