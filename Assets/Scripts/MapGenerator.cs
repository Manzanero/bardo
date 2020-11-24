using System.Linq;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public void CreateMapFromDonjon(string textMap)
    {
        var gameMaster = GameMaster.instance;
        var thisTransform = transform;
        var mapPosition = thisTransform.position;
        var mapRotation = thisTransform.rotation;
        
        var lines = textMap.Split('\n').Reverse().ToArray();
        var height = lines.Length;
        var width = lines[0].Split('\t').Length;
        var tiles = new Tile[width * 2, height * 2];
        var mapGo = Instantiate(gameMaster.mapPrefab, Vector3.zero, mapRotation, gameMaster.campaign.mapsParent);
        mapGo.name = "Donjon Map";
        var map = mapGo.GetComponent<Map>();
        map.tiles = tiles;
        
        for (var rowIndex = 0; rowIndex < height; rowIndex++)
        {
            var line = lines[rowIndex];
            var cols = line.Split('\t');
            for (var colIndex = 0; colIndex < width; colIndex++)
            {
                var col = cols[colIndex];
                
                var tilePos = new Vector3(mapPosition.x + colIndex * 2,
                    mapPosition.y,
                    mapPosition.z + rowIndex * 2);
                var tileGo = Instantiate(gameMaster.tilePrefab, tilePos, mapRotation, map.tilesParent);
                tileGo.name = $"Tile-{colIndex * 2}-{rowIndex * 2}-{col}";
                var tile = tileGo.GetComponent<Tile>();
                tile.Position = new Vector2Int(colIndex * 2, rowIndex * 2);
                tiles[colIndex * 2, rowIndex * 2] = tile;
                LoadTile(map, tile, col);
                
                tilePos = new Vector3(mapPosition.x + colIndex * 2 + 1,
                    mapPosition.y,
                    mapPosition.z + rowIndex * 2);
                tileGo = Instantiate(gameMaster.tilePrefab, tilePos, mapRotation, map.tilesParent);
                tileGo.name = $"Tile-{colIndex * 2 + 1}-{rowIndex * 2}-{col}";
                tile = tileGo.GetComponent<Tile>();
                tile.Position = new Vector2Int(colIndex * 2 + 1, rowIndex * 2);
                tiles[colIndex * 2 + 1, rowIndex * 2] = tile;
                LoadTile(map, tile, col);
                
                tilePos = new Vector3(mapPosition.x + colIndex * 2,
                    mapPosition.y,
                    mapPosition.z + rowIndex * 2 + 1);
                tileGo = Instantiate(gameMaster.tilePrefab, tilePos, mapRotation, map.tilesParent);
                tileGo.name = $"Tile-{colIndex * 2}-{rowIndex * 2 + 1}-{col}";
                tile = tileGo.GetComponent<Tile>();
                tile.Position = new Vector2Int(colIndex * 2, rowIndex * 2 + 1);
                tiles[colIndex * 2, rowIndex * 2 + 1] = tile;
                LoadTile(map, tile, col);
                
                tilePos = new Vector3(mapPosition.x + colIndex * 2 + 1,
                    mapPosition.y,
                    mapPosition.z + rowIndex * 2 + 1);
                tileGo = Instantiate(gameMaster.tilePrefab, tilePos, mapRotation, map.tilesParent);
                tileGo.name = $"Tile-{colIndex * 2 + 1}-{rowIndex * 2 + 1}-{col}";
                tile = tileGo.GetComponent<Tile>();
                tile.Position = new Vector2Int(colIndex * 2 + 1, rowIndex * 2 + 1);
                tiles[colIndex * 2 + 1, rowIndex * 2 + 1] = tile;
                LoadTile(map, tile, col);
            }
        }
    }

    private static void LoadTile(Map map, Tile tile, string codename)
    {
        tile.map = map;
        switch (codename)
        {
            case "SDD":  // stairs down 2
                tile.Blueprint = map.blueprint.theme.stairsDown2;
                break;
            case "SD":  // stairs down 1
                tile.Blueprint = map.blueprint.theme.stairsDown1;
                break;
            case "F":   // floor
                tile.Blueprint = map.blueprint.theme.floor;
                break;
            case "SU":   // stairs up 1
                tile.Blueprint = map.blueprint.theme.stairsUp1;
                break;
            case "SUU":   // stairs up 2
                tile.Blueprint = map.blueprint.theme.stairsUp2;
                break;
            case "DT":  // door oriented to north
            case "DB":  // door oriented to north trapped
            case "DL":  // door oriented to est
            case "DR":  // door oriented to est trapped
                tile.Blueprint = map.blueprint.theme.door;
                break;
            case "DPT":  // portcullis to north
            case "DPB":  // portcullis to north
            case "DPL":  // portcullis to est
            case "DPR":  // portcullis to est
                tile.Blueprint = map.blueprint.theme.portcullis;
                break;
            case "DST":  // door oriented to north secret
            case "DSB":  // door oriented to north secret
            case "DSL":  // door oriented to est secret
            case "DSR":  // door oriented to est secret
                tile.Blueprint = map.blueprint.theme.secretDoor;
                break;
            case "":  // wall
                tile.Blueprint = map.blueprint.theme.wall;
                break;
            default:
                Debug.LogError($"not expected tile code: {codename}");
                break;
        }
        tile.Shadow = true;
        tile.Luminosity = 0.5f;
        tile.Explored = false;
    }
}