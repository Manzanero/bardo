#pragma warning disable 0649

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Map : MonoBehaviour
{
    public Transform tilesParent;
    public Transform entitiesParent;
    public int visionBlockerLayer;
    public int mapLayerMask;
    public Material tileMaterial;
    public Material translucentTileMaterial;
    
    public MapBlueprint blueprint;
    
    public string id;
    public Tile[,] tiles;
    public List<Entity> entities = new List<Entity>();
    public bool loaded;
    
    public Vector3 mousePosition;
    public bool mouseOverTile;
    public Tile mouseTile;
    public List<Tile> selectedTiles;
    public List<Tile> tilesInLight = new List<Tile>();
    public List<Entity> selectedEntities = new List<Entity>();
    public List<MapProperty> properties;

    [Serializable] public class MapProperty
    {
        public string name;
        public string value;
    }
    
    public int Width => tiles.GetLength(0);
    public int Height => tiles.GetLength(1);
    
    private Camera _mainCamera;
    private readonly WaitForSecondsRealtime _updateExploredPeriod = new WaitForSecondsRealtime(10f);
    private List<Tile> _allTilesInLight = new List<Tile>();
    private List<Tile> _allTilesInVision = new List<Tile>();
    private List<Transform> _cachedTilesTransform = new List<Transform>();
    private List<Tile> _cachedTiles = new List<Tile>();
    private List<Tile> _extraTilesToReveal = new List<Tile>();
    private string _cachedExploredArray;

    private void Start()
    {
        _mainCamera = Camera.main;    
        mapLayerMask = LayerMask.GetMask("Tiles", "Vision Blocker");
        visionBlockerLayer = LayerMask.NameToLayer("Vision Blocker");
    }

    
    public IEnumerator SaveMapProperty(string propertyName, string propertyValue)
    {
        var url = $"{Server.baseUrl}/world/campaign/{Campaign.campaignId}/map/{id}/property/{propertyName}/save";
        var request = Server.PostRequest(url, propertyValue);
        while (!request.isDone)
            yield return null;
        
        Server.GetResponse<Server.Response>(request);
    }

    private IEnumerator UpdateExplored()
    {
        if (Campaign.playerIsMaster)
            yield break;
        
        while (!loaded)
            yield return null;
        
        while (loaded)
        {
            var exploredArray = "";
            for (var y = 0; y < Height; y += 1) 
            for (var x = 0; x < Width; x += 1)
                exploredArray += tiles[x, y].Explored ? "1" : "0";

            if (_cachedExploredArray == exploredArray)
            {
                yield return _updateExploredPeriod;
                continue;
            }
                    
            _cachedExploredArray = exploredArray;
            
            StartCoroutine(SaveMapProperty("EXPLORED", exploredArray));
            yield return _updateExploredPeriod;
        }
    }
    
    private void OnEnable()
    {
        StartCoroutine(UpdateExplored());
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            // var sr = File.CreateText(@"C:\workspace\unity\bardo\Bardo\Assets\Logs\debug.txt");
            // sr.WriteLine(JsonUtility.ToJson(Serialize()));
            // sr.Close();
            // // Debug.LogWarning(ToJson());
        }
        
        UpdateVision();
        UpdateMouse();
    }

    private void UpdateVision()
    {
        if (Campaign.playerIsMaster && !Campaign.sharingPlayerVision)
            return;
        
        _allTilesInLight.Clear();
        _allTilesInLight = entities
            .Select(x => x.tilesInLight)
            .Aggregate(_allTilesInLight, (x, y) => x.Union(y).ToList());

        _allTilesInVision.Clear();
        _allTilesInVision = entities.Where(x => x.SharedVision)
            .Select(x => x.tilesInVision)
            .Aggregate(_allTilesInVision, (x, y) => x.Union(y).ToList());
        
        var tilesToReveal = _allTilesInLight.Intersect(_allTilesInVision).ToList();

        foreach (var tile in tilesInLight.Except(tilesToReveal))
        {
            tile.Shadow = true;
            tile.Luminosity = 0.5f;
        }
        
        foreach (var tile in tilesToReveal.Except(tilesInLight))
        {
            tile.Shadow = false;
            tile.Luminosity = 1f;
            tile.Explored = true;
        }
        tilesInLight = tilesToReveal;
    }

    public void ResetVision() => tilesInLight.Clear();

    private void UpdateMouse()
    {
        var ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        Tile tile;
        if (Physics.Raycast(ray, out var rayCastHit, 100f))
            tile = rayCastHit.transform.GetComponent<Tile>();
        else
        {
            mouseOverTile = false;
            return;
        }
        
        mouseOverTile = (bool) tile;
        
        if (mouseOverTile)
        {
            mousePosition = rayCastHit.point;
            mouseTile = tile;
        }
        else
        {
            if (!Physics.Raycast(ray, out rayCastHit, 100f, mapLayerMask))
                return;
        
            mousePosition = rayCastHit.point;
            mouseTile = rayCastHit.transform.GetComponent<Tile>();
        }
            
    }
    
    public Tile Tile(Vector2Int position)
    {
        try
        {
            return tiles[position.x, position.y];
        }
        catch (IndexOutOfRangeException)
        {
            return null;
        }
    }
    
    public List<Tile> AdjacentTiles(Vector2Int position)
    {
        return new List<Tile>
        {
            Tile(new Vector2Int(position.x - 1, position.y + 1)),
            Tile(new Vector2Int(position.x + 1, position.y + 1)),
            Tile(new Vector2Int(position.x - 1, position.y - 1)),
            Tile(new Vector2Int(position.x + 1, position.y - 1)),
            
            Tile(new Vector2Int(position.x - 1, position.y)),
            Tile(new Vector2Int(position.x, position.y - 1)),
            Tile(new Vector2Int(position.x, position.y + 1)),
            Tile(new Vector2Int(position.x + 1, position.y))
        }.Where(x => !ReferenceEquals(x, null) && x.VisionBlocker).ToList();
    }
    
    public List<Tile> TilesInRange(Vector2Int position, float range)
    {
        // var a = System.DateTime.Now;
        
        const int rayCount = 256;
        const float angleIncrease = 360f / rayCount;
        var angle = 360f;
        var origin = new Vector3(position.x, -1.05f, position.y);
        var rayCastHits = new RaycastHit[(int) (range - 1) * 2]; 

        _cachedTilesTransform.Clear();
        _cachedTiles.Clear();

        // get ground transforms in sight
        for (var i = 0; i <= rayCount; i++)
        {
            var hits = Physics.RaycastNonAlloc(origin, VectorFromAngle(angle), 
                rayCastHits, range, mapLayerMask);
            var obstacleDistance = range - 1;
            
            for (var j = 0; j < hits; j++)
            {
                var rayCastHitDistance = rayCastHits[j].distance;
                if (rayCastHitDistance <= obstacleDistance &&
                    rayCastHits[j].transform.gameObject.layer == visionBlockerLayer)
                    obstacleDistance = rayCastHitDistance;
            }
            for (var j = 0; j < hits; j++)
            {
                if (rayCastHits[j].distance < obstacleDistance)
                    _cachedTilesTransform.Add(rayCastHits[j].transform);
                
                // if (rayCastHits[j].transform.gameObject.layer != 9) continue;
                // var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                // cube.transform.position = rayCastHits[j].point;
                // cube.transform.localScale = Vector3.one * 0.1f;
                // cube.layer = 5;

            }
            angle -= angleIncrease;
        }
        
        _cachedTilesTransform = _cachedTilesTransform.Distinct().ToList();
        
        // add current tile transform
        _cachedTilesTransform.Add(Tile(position).transform);
        
        // get tiles
        foreach (var t in _cachedTilesTransform
            .Select(tileTransform => tileTransform.GetComponent<Tile>())) 
            _cachedTiles.Add(t);

        // add walls close to tiles to reveal
        _extraTilesToReveal.Clear();
        foreach (var extraTiles in _cachedTiles.Select(x => AdjacentTiles(x.Position)))
            _extraTilesToReveal.AddRange(extraTiles);
        _cachedTiles.AddRange(_extraTilesToReveal);
        
        // remove duplicates
        _cachedTiles = _cachedTiles.Distinct().ToList();
        _cachedTiles.Remove(null);
        
        // Debug.Log((System.DateTime.Now - a).TotalMilliseconds);

        return _cachedTiles;
    }
    
    private static Vector3 VectorFromAngle(float angle)
    {
        var angleRad = angle * (Mathf.PI / 180f);
        return new Vector3(Mathf.Cos(angleRad), 0, Mathf.Sin(angleRad));
    }

    #region Serialization
    
    [Serializable]
    public class SerializableMap
    {
        public string mapId;
        public string name;
        public Vector2Int size;
        public List<Tile.SerializableTile> tiles;
        public List<Entity.SerializableEntity> entities;
    }
    
    public SerializableMap Serialize()
    {
        // tiles
        var serializableTiles = new List<Tile.SerializableTile>();
        for (var x = 0; x < Width; x += 1)
        for (var y = 0; y < Height; y += 1) 
            serializableTiles.Add(tiles[x, y].Serialize());
        
        // entities
        var serializableEntities = entities.Select(entity => entity.Serialize()).ToList();

        var mapObject = new SerializableMap
        {
            name = name,
            mapId = id,
            size = new Vector2Int(Width, Height),
            tiles = serializableTiles,
            entities = serializableEntities
        };
        return mapObject;
    }

    public void Deserialize(SerializableMap serializableMap)
    {
        name = serializableMap.name;
        id = serializableMap.mapId;
        var gameMaster = GameMaster.instance;
        tiles = new Tile[serializableMap.size.x, serializableMap.size.y];
        foreach (var serializableTile in serializableMap.tiles)
        {
            var tile = Instantiate(gameMaster.tilePrefab, tilesParent).GetComponent<Tile>();
            tile.name = $"Tile-{serializableTile.position.x}-{serializableTile.position.y}";
            tile.map = this;
            tile.Deserialize(serializableTile);
            tiles[serializableTile.position.x, serializableTile.position.y] = tile;
            
            // properties
            if (Campaign.playerIsMaster)
            {
                tile.Shadow = false;
                tile.Luminosity = 1f;
                tile.Explored = true;
            }
            else
            {
                tile.Shadow = true;
                tile.Luminosity = 0.5f;
                tile.Explored = false;
            }
        }

        entities.Clear();

        foreach (var serializableEntity in serializableMap.entities)
        {
            var entity = Instantiate(gameMaster.entityPrefab, entitiesParent).GetComponent<Entity>();
            entity.map = this;
            entity.Deserialize(serializableEntity);
            entities.Add(entity);
        }
    }
    
    #endregion
}