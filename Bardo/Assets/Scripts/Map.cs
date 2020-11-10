using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Map : MonoBehaviour
{
    public Transform tilesParent;
    public Transform entitiesParent;
    public MapBlueprint blueprint;
    
    public Tile[,] tiles;
    public List<Entity> entities;

    public Vector3 mousePosition;
    public Tile mouseTile;
    public List<Tile> tilesInLight = new List<Tile>();

    public int visionBlockerLayer;
    public int mapLayerMask;
    
    private void Start()
    {
        mapLayerMask = LayerMask.GetMask("Tiles", "Vision Blocker");
        visionBlockerLayer = LayerMask.NameToLayer("Vision Blocker");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            var sr = File.CreateText(@"C:\workspace\unity\bardo\Bardo\Assets\Logs\debug.txt");
            sr.WriteLine(JsonUtility.ToJson(Serialize()));
            sr.Close();
            // Debug.LogWarning(ToJson());
        }
        
        UpdateTiles();
    }

    private void UpdateTiles()
    {
        var allTilesInLight = new List<Tile>();
        allTilesInLight = entities
            .Select(x => x.tilesInLight)
            .Aggregate(allTilesInLight, (x, y) => x.Union(y).ToList());

        var allTilesInVision = new List<Tile>();
        allTilesInVision = entities.Where(x => x.SharedVision)
            .Select(x => x.tilesInVision)
            .Aggregate(allTilesInVision, (x, y) => x.Union(y).ToList());
        
        var tilesToReveal = allTilesInLight.Intersect(allTilesInVision).ToList();

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
        }.Where(x => !ReferenceEquals(x, null)).ToList();
    }
    
    public List<Tile> TilesInRange(Vector2Int position, float range)
    {
        // var a = System.DateTime.Now;
        
        const int rayCount = 256;
        const float angleIncrease = 360f / rayCount;
        var angle = 360f;
        var origin = new Vector3(position.x, -1.05f, position.y);
        var rayCastHits = new RaycastHit[(int) (range - 1) * 2]; 

        var cachedTilesTransform = new List<Transform>();
        var cachedTiles = new List<Tile>();

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
                    cachedTilesTransform.Add(rayCastHits[j].transform);
                
                // if (rayCastHits[j].transform.gameObject.layer != 9) continue;
                // var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                // cube.transform.position = rayCastHits[j].point;
                // cube.transform.localScale = Vector3.one * 0.1f;
                // cube.layer = 5;

            }
            angle -= angleIncrease;
        }
        
        cachedTilesTransform = cachedTilesTransform.Distinct().ToList();
        
        // add current tile transform
        cachedTilesTransform.Add(Tile(position).transform);
        
        // get tiles
        foreach (var t in cachedTilesTransform
            .Select(tileTransform => tileTransform.GetComponent<Tile>())) 
            cachedTiles.Add(t);

        // add walls close to tiles to reveal
        var extraTilesToReveal = new List<Tile>();
        foreach (var extraTiles in cachedTiles.Select(x => AdjacentTiles(x.Position)))
            extraTilesToReveal.AddRange(extraTiles);
        cachedTiles.AddRange(extraTilesToReveal);
        
        // remove duplicates
        cachedTiles = cachedTiles.Distinct().ToList();
        cachedTiles.Remove(null);
        
        // Debug.Log((System.DateTime.Now - a).TotalMilliseconds);

        return cachedTiles;
    }
    
    private static Vector3 VectorFromAngle(float angle)
    {
        var angleRad = angle * (Mathf.PI / 180f);
        return new Vector3(Mathf.Cos(angleRad), 0, Mathf.Sin(angleRad));
    }
    
    public void AddEntity(Entity entity)
    {
        entity.transform.parent = entitiesParent;
        if (entity.map != null)
            entity.map.entities.Remove(entity);
        entities.Add(entity);
        entity.map = this;
        var position = entity.transform.position;
        entity.tile = Tile(new Vector2Int((int) Mathf.Round(position.x), (int) Mathf.Round(position.z)));
    }
    
    [Serializable]
    public class SerializableMap
    {
        public string name;
        public Vector2Int size;
        public List<Tile.SerializableTile> tiles;
        public List<Entity.SerializableEntity> entities;
    }
    
    public SerializableMap Serialize()
    {
        var width = tiles.GetLength(0);
        var height = tiles.GetLength(1);
        
        // tiles
        var serializableTiles = new List<Tile.SerializableTile>();
        for (var x = 0; x < width; x += 1)
            for (var y = 0; y < height; y += 1) 
                serializableTiles.Add(tiles[x, y].Serialize());
        
        // entities
        var serializableEntities = entities.Select(entity => entity.Serialize()).ToList();

        var mapObject = new SerializableMap
        {
            name = name,
            size = new Vector2Int(width, height),
            tiles = serializableTiles,
            entities = serializableEntities
        };
        return mapObject;
    }

    public void Deserialize(SerializableMap serializableMap)
    {
        var gameMaster = GameMaster.instance;
        tiles = new Tile[serializableMap.size.x, serializableMap.size.y];
        foreach (var serializableTile in serializableMap.tiles)
        {
            var tileGo = Instantiate(gameMaster.tilePrefab, (Vector2) serializableTile.position, 
                Quaternion.identity, tilesParent);
            tileGo.name = $"Tile-{serializableTile.position.x}-{serializableTile.position.y}";
            var tile = tileGo.GetComponent<Tile>();
            tile.map = this;
            tile.Deserialize(serializableTile);
            tiles[serializableTile.position.x, serializableTile.position.y] = tile;
        }

        entities = new List<Entity>();
        foreach (var serializableEntity in serializableMap.entities)
        {
            var entityGo = Instantiate(gameMaster.entityPrefab, (Vector2) serializableEntity.position, 
                Quaternion.identity, entitiesParent);
            var entity = entityGo.GetComponent<Entity>();
            entity.map = this;
            entity.Deserialize(serializableEntity);
            entities.Add(entity);
        }
    }
}