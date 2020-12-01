using System;
using System.Collections.Generic;
using System.Linq;
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

    
    public EntityBlueprint blueprint;
    public Map map;
    public Tile tile;
    public List<Tile> tilesInVision;
    public List<Tile> tilesInLight;

    [SerializeField] private float initiative;
    [SerializeField] private Vector2 tileOffset;
    [SerializeField] private float rotation;
    [SerializeField] private float health;
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float stamina;
    [SerializeField] private float maxStamina = 100;
    [SerializeField] private float mana;
    [SerializeField] private float maxMana = 100;
    [SerializeField] private float visionInShadows;
    [SerializeField] private float visionInDarkness;
    
    // Blueprint
    [SerializeField] private bool hasBase;
    [SerializeField] private float baseSize;
    [SerializeField] private Material baseMaterial;
    [SerializeField] private bool hasBaseImage;
    [SerializeField] private Sprite baseImage;
    [SerializeField] private bool hasBody = true;
    [SerializeField] private float bodySize = 1f;
    [SerializeField] private Mesh bodyMesh;
    [SerializeField] private float scaleCorrection;
    [SerializeField] private Material bodyMaterial;
    [SerializeField] private bool hasName;
    [SerializeField] private bool hasHealth;
    [SerializeField] private bool hasStamina;
    [SerializeField] private bool hasMana;
    [SerializeField] private bool hasVision;
    [SerializeField] private bool hasLight;
    [SerializeField] private float lightRange;
    [SerializeField] private bool hasAura;
    [SerializeField] private float auraRange;
    [SerializeField] private Material auraMaterial;

    // Permissions
    [SerializeField] private bool sharedName;
    [SerializeField] private bool sharedPosition;
    [SerializeField] private bool sharedVision;
    [SerializeField] private bool sharedControl;
    [SerializeField] private bool sharedHealth = true;
    [SerializeField] private bool sharedStamina = true;
    [SerializeField] private bool sharedMana = true;

    private Camera _mainCamera;
    private Tile _cachedTile;
    private bool _inMovement;
    private bool _resetVision;

    public string Name
    {
        get => name;
        set
        {
            nameText.text = value;
            name = value;
        }
    }

    public float Health
    {
        get => health;
        set
        {
            healthImage.fillAmount = value / maxHealth;
            healthText.text = $"{value} / {maxHealth}";
            health = value;
        }
    }

    public float MaxHealth
    {
        get => maxHealth;
        set
        {
            healthImage.fillAmount = health / value;
            healthText.text = $"{health} / {value}";
            maxHealth = value;
        }
    }

    public float Stamina
    {
        get => stamina;
        set
        {
            staminaImage.fillAmount = value / maxStamina;
            staminaText.text = $"{value} / {maxStamina}";
            stamina = value;
        }
    }

    public float MaxStamina
    {
        get => maxStamina;
        set
        {
            staminaImage.fillAmount = stamina / value;
            staminaText.text = $"{stamina} / {value}";
            maxStamina = value;
        }
    }

    public float Mana
    {
        get => mana;
        set
        {
            manaImage.fillAmount = value / maxMana;
            manaText.text = $"{value}/{maxMana}";
            mana = value;
        }
    }

    public float MaxMana
    {
        get => maxMana;
        set
        {
            manaImage.fillAmount = mana / value;
            manaText.text = $"{mana} / {value}";
            maxMana = value;
        }
    }

    public EntityBlueprint Blueprint
    {
        get => blueprint;
        set
        {
            blueprint = value;
            HasBase = value.hasBase;
            baseSize = value.baseSize; // BaseSize = value.baseSize;
            BaseMaterial = value.baseMaterial;
            hasBaseImage = value.hasBaseImage; // HasBaseImage = value.hasBaseImage;
            baseImage = value.baseImage; // BaseImage = value.baseImage;
            HasBody = value.hasBody;
            BodySize = value.bodySize;
            BodyMesh = value.bodyMesh;
            ScaleCorrection = value.scaleCorrection;
            BodyMaterial = value.bodyMaterial;
            HasName = value.hasName;
            HasHealth = value.hasHealth;
            HasStamina = value.hasStamina;
            HasMana = value.hasMana;
            HasVision = value.hasVision;
            HasLight = value.hasLight;
            LightRange = value.lightRange;
            hasAura = value.hasAura; // HasAura = value.hasAura;
            auraRange = value.auraRange; // AuraRange = value.auraRange;
            auraMaterial = value.auraMaterial; // AuraMaterial = value.auraMaterial;

            _resetVision = true;
        }
    }

    public bool HasBase
    {
        get => hasBase;
        set
        {
            var position = transform.position;
            baseMeshRenderer.transform.position = position + 0.05f * Vector3.up;
            bodyMeshRenderer.transform.position = value ? position + 0.1f * Vector3.up : position;
            baseMeshRenderer.gameObject.SetActive(value);
            hasBase = value;
        }
    }

    public Material BaseMaterial
    {
        get => baseMaterial;
        set
        {
            baseMeshRenderer.material = value;
            baseMaterial = value;
        }
    }

    public bool HasBody
    {
        get => hasBody;
        set
        {
            bodyMeshRenderer.gameObject.SetActive(hasBody);
            hasBody = value;
        }
    }

    public float BodySize
    {
        get => bodySize;
        set => bodySize = value;
    }

    public Mesh BodyMesh
    {
        get => bodyMesh;
        set
        {
            bodyMeshFilter.mesh = value;
            bodyMesh = value;
        }
    }

    public float ScaleCorrection
    {
        get => scaleCorrection;
        set
        {
            bodyMeshRenderer.transform.localScale = Vector3.one * value;
            scaleCorrection = value;
        }
    }

    public Material BodyMaterial
    {
        get => bodyMaterial;
        set
        {
            bodyMeshRenderer.material = value;
            bodyMaterial = value;
        }
    }

    public bool HasName
    {
        get => hasName;
        set
        {
            nameCanvas.gameObject.SetActive(value);
            hasName = value;
        }
    }

    public bool HasHealth
    {
        get => hasHealth;
        set
        {
            healthBar.gameObject.SetActive(value);
            hasHealth = value;
        }
    }

    public bool HasStamina
    {
        get => hasStamina;
        set
        {
            staminaBar.gameObject.SetActive(value);
            hasStamina = value;
        }
    }

    public bool HasMana
    {
        get => hasMana;
        set
        {
            manaBar.gameObject.SetActive(value);
            hasMana = value;
        }
    }

    public bool HasVision
    {
        get => hasVision;
        set
        {
            hasVision = value;
            _resetVision = true;
        }
    }

    public bool HasLight
    {
        get => hasLight;
        set
        {
            hasLight = value;
            _resetVision = true;
        }
    }

    public float LightRange
    {
        get => lightRange;
        set
        {
            lightRange = value;
            _resetVision = true;
        }
    }

    public bool SharedName
    {
        get => sharedName;
        set => sharedName = value;
    }

    public bool SharedPosition
    {
        get => sharedPosition;
        set
        {
            sharedPosition = value;
            _resetVision = true;
        }
    }

    public bool SharedVision
    {
        get => sharedVision;
        set
        {
            sharedVision = value;
            _resetVision = true;
        }
    }

    public bool SharedControl
    {
        get => sharedControl;
        set
        {
            sharedControl = value;
            _resetVision = true;
        }
    }

    public bool SharedHealth { get => sharedHealth; set => sharedHealth = value; }

    public bool SharedStamina { get => sharedStamina; set => sharedStamina = value; }

    public bool SharedMana { get => sharedMana; set => sharedMana = value; }

    private void Start()
    {
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        if (ReferenceEquals(map, null))
            return;
        if (ReferenceEquals(tile, null))
        {
            var position = transform.position;
            tile = map.Tile(new Vector2Int((int) Mathf.Round(position.x), (int) Mathf.Round(position.z)));
        }

        UpdateUi();
        UpdatePosition();
        UpdateVision();
        UpdateLuminosity();
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
            var newScale = 0.002f * Vector3.one;
            nameCanvasTransform.localScale = newScale;
            barsCanvasTransform.localScale = newScale;
        }
        else
        {
            var newScale = 0.001f * Vector3.one;
            nameCanvasTransform.localScale = newScale;
            barsCanvasTransform.localScale = newScale;
        }
    }

    private void UpdatePosition()
    {
        if (!SharedControl) 
            return;
        
        if (Dragged)
        {
            _inMovement = true;
            if (ReferenceEquals(map.mouseTile, null) || !map.mouseTile.Walkable) 
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
            GameMaster.instance.RegisterAction(new Action
            {
                map = map.name,
                name = GameMaster.ActionNames.ChangeEntity,
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
            baseMeshRenderer.gameObject.SetActive(tile.Explored || SharedControl);
            bodyMeshRenderer.gameObject.SetActive(tile.Explored || SharedControl);
        }
        else
        {
            baseMeshRenderer.gameObject.SetActive(false);
            bodyMeshRenderer.gameObject.SetActive(false);
        }
        var tileLuminosity = tile.Luminosity;
        var baseColor = baseMaterial.color;
        baseMeshRenderer.material.color = new Color(baseColor.r * tileLuminosity,baseColor.g * tileLuminosity,
            baseColor.b * tileLuminosity);
        bodyMeshRenderer.material.color = new Color(tileLuminosity,tileLuminosity,tileLuminosity);
    }
    
    private bool Dragged => draggableBody.Dragged || draggableBase.Dragged;
    
    private bool MouseOver => draggableBody.MouseOver || draggableBase.MouseOver;

    private bool ColliderEnabled { set => (baseMeshCollider.enabled, bodyMeshCollider.enabled) = (value, value); }
    
    [Serializable]
    public class SerializableEntity
    {
        public string name;
        public string map;
        public string blueprint;
        public float initiative;
        public Vector2Int position;
        public Vector2 tileOffset;
        public float rotation;
        public float health;
        public float maxHealth;
        public float stamina;
        public float maxStamina;
        public float mana;
        public float maxMana;
        public float visionInShadows;
        public float visionInDarkness;
    }
    

    public SerializableEntity Serialize()
    {
        var tileObject = new SerializableEntity
        {
            name = name,
            map = map.name,
            blueprint = Blueprint.folder + "/" + Blueprint.name,
            initiative = initiative,
            position = tile.Position,
            tileOffset = tileOffset,
            rotation = rotation,
            health = Health,
            maxHealth = MaxHealth,
            stamina = Stamina,
            maxStamina = MaxStamina,
            mana = Mana,
            maxMana = MaxMana,
            visionInShadows = visionInShadows,
            visionInDarkness = visionInDarkness,
        };
        return tileObject;
    }

    public void Deserialize(SerializableEntity serializableEntity)
    {
        name = serializableEntity.name;
        Blueprint = GetEntityBlueprint(serializableEntity.blueprint);
        initiative = serializableEntity.initiative;
        tileOffset = serializableEntity.tileOffset;
        transform.position = new Vector3(
            serializableEntity.position.x + tileOffset.x, 1, serializableEntity.position.y + tileOffset.y); 
        rotation = serializableEntity.rotation;
        Health = serializableEntity.health;
        MaxHealth = serializableEntity.maxHealth;
        Stamina = serializableEntity.stamina;
        MaxStamina = serializableEntity.maxStamina;
        Mana = serializableEntity.mana;
        MaxMana = serializableEntity.maxMana;
        visionInShadows = serializableEntity.visionInShadows;
        visionInDarkness = serializableEntity.visionInDarkness;
    }

    private static EntityBlueprint GetEntityBlueprint(string path)
    {
        return Resources.LoadAll($"Blueprints/Entities/{path}", 
            typeof(EntityBlueprint)).Cast<EntityBlueprint>().ToArray()[0];
    }
}