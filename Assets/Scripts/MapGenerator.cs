using System.Linq;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public static Map CreateMapFromDonjon(string textMap)
    {
        var gameMaster = GameMaster.instance;
        
        var lines = textMap.Split('\n').Reverse().ToArray();
        var height = lines.Length;
        var width = lines[0].Split('\t').Length;
        var tiles = new Tile[width * 2, height * 2];
        var map = Instantiate(gameMaster.mapPrefab, gameMaster.campaign.mapsParent).GetComponent<Map>();
        map.tiles = tiles;
        
        for (var rowIndex = 0; rowIndex < height; rowIndex++)
        {
            var line = lines[rowIndex];
            var cols = line.Split('\t');
            for (var colIndex = 0; colIndex < width; colIndex++)
            {
                var col = cols[colIndex];
                
                var tilePos = new Vector3(colIndex * 2, 0, rowIndex * 2);
                var tileGo = Instantiate(gameMaster.tilePrefab, tilePos, Quaternion.identity, map.tilesParent);
                tileGo.name = $"Tile-{colIndex * 2}-{rowIndex * 2}-{col}";
                var tile = tileGo.GetComponent<Tile>();
                tile.Position = new Vector2Int(colIndex * 2, rowIndex * 2);
                tiles[colIndex * 2, rowIndex * 2] = tile;
                LoadTile(map, tile, col);
                
                tilePos = new Vector3(colIndex * 2 + 1, 0, rowIndex * 2);
                tileGo = Instantiate(gameMaster.tilePrefab, tilePos, Quaternion.identity, map.tilesParent);
                tileGo.name = $"Tile-{colIndex * 2 + 1}-{rowIndex * 2}-{col}";
                tile = tileGo.GetComponent<Tile>();
                tile.Position = new Vector2Int(colIndex * 2 + 1, rowIndex * 2);
                tiles[colIndex * 2 + 1, rowIndex * 2] = tile;
                LoadTile(map, tile, col);
                
                tilePos = new Vector3(colIndex * 2, 0, rowIndex * 2 + 1);
                tileGo = Instantiate(gameMaster.tilePrefab, tilePos, Quaternion.identity, map.tilesParent);
                tileGo.name = $"Tile-{colIndex * 2}-{rowIndex * 2 + 1}-{col}";
                tile = tileGo.GetComponent<Tile>();
                tile.Position = new Vector2Int(colIndex * 2, rowIndex * 2 + 1);
                tiles[colIndex * 2, rowIndex * 2 + 1] = tile;
                LoadTile(map, tile, col);
                
                tilePos = new Vector3(colIndex * 2 + 1, 0, rowIndex * 2 + 1);
                tileGo = Instantiate(gameMaster.tilePrefab, tilePos, Quaternion.identity, map.tilesParent);
                tileGo.name = $"Tile-{colIndex * 2 + 1}-{rowIndex * 2 + 1}-{col}";
                tile = tileGo.GetComponent<Tile>();
                tile.Position = new Vector2Int(colIndex * 2 + 1, rowIndex * 2 + 1);
                tiles[colIndex * 2 + 1, rowIndex * 2 + 1] = tile;
                LoadTile(map, tile, col);
            }
        }

        return map;
    }

    private static void LoadTile(Map map, Tile tile, string codename)
    {
        tile.map = map;
        switch (codename)
        {
            case "SDD":  // stairs down 2
                tile.Altitude = -1f;
                tile.Walkable = true;
                tile.VisionBlocker = false;
                tile.GraphicMeshResource = "Tiles/Bottom/Bottom";
                tile.PhysicMeshResource = "Tiles/Bottom/Bottom";
                tile.TextureResource = "Tiles/Bottom/Bottom_StoneStairsDown2";
                break;
            case "SD":  // stairs down 1
                tile.Altitude = -0.5f;
                tile.Walkable = true;
                tile.VisionBlocker = false;
                tile.GraphicMeshResource = "Tiles/Gap/Gap";
                tile.PhysicMeshResource = "Tiles/Gap/Gap";
                tile.TextureResource = "Tiles/Gap/Gap_StoneStairsDown1";
                break;
            case "F":   // floor
                tile.Altitude = 0;
                tile.Walkable = true;
                tile.VisionBlocker = false;
                tile.GraphicMeshResource = "Tiles/Ground/Ground";
                tile.PhysicMeshResource = "Tiles/Ground/Ground";
                tile.TextureResource = "Tiles/Ground/Ground_Stone";
                break;
            case "SU":   // stairs up 1
                tile.Altitude = 0.5f;
                tile.Walkable = true;
                tile.VisionBlocker = false;
                tile.GraphicMeshResource = "Tiles/Platform/Platform";
                tile.PhysicMeshResource = "Tiles/Platform/Platform";
                tile.TextureResource = "Tiles/Platform/Platform_StoneStairsUp1";
                break;
            case "SUU":   // stairs up 2
                tile.Altitude = 1f;
                tile.Walkable = true;
                tile.VisionBlocker = false;
                tile.GraphicMeshResource = "Tiles/Top/Top";
                tile.PhysicMeshResource = "Tiles/Top/Top";
                tile.TextureResource = "Tiles/Top/Top_StoneStairsUp2";
                break;
            case "DT":  // door oriented to north
            case "DB":  // door oriented to north trapped
            case "DL":  // door oriented to est
            case "DR":  // door oriented to est trapped
                tile.Altitude = 1f;
                tile.Walkable = true;
                tile.VisionBlocker = true;
                tile.GraphicMeshResource = "Tiles/Top/Top";
                tile.PhysicMeshResource = "Tiles/Top/Top";
                tile.TextureResource = "Tiles/Top/Top_Door";
                break;
            case "DPT":  // portcullis to north
            case "DPB":  // portcullis to north
            case "DPL":  // portcullis to est
            case "DPR":  // portcullis to est
                tile.Altitude = 1f;
                tile.Walkable = false;
                tile.VisionBlocker = false;
                tile.GraphicMeshResource = "Tiles/Top/Top";
                tile.PhysicMeshResource = "Tiles/Top/Top";
                tile.TextureResource = "Tiles/Top/Top_Portcullis";
                break;
            case "DST":  // door oriented to north secret
            case "DSB":  // door oriented to north secret
            case "DSL":  // door oriented to est secret
            case "DSR":  // door oriented to est secret
                tile.Altitude = 1f;
                tile.Walkable = true;
                tile.VisionBlocker = true;
                tile.GraphicMeshResource = "Tiles/Top/Top";
                tile.PhysicMeshResource = "Tiles/Top/Top";
                tile.TextureResource = "Tiles/Top/Top_Brick";
                break;
            case "":  // wall
                tile.Altitude = 1f;
                tile.Walkable = false;
                tile.VisionBlocker = true;
                tile.GraphicMeshResource = "Tiles/Top/Top";
                tile.PhysicMeshResource = "Tiles/Top/Top";
                tile.TextureResource = "Tiles/Top/Top_Brick";
                break;
            default:
                tile.Altitude = 0;
                tile.Walkable = false;
                tile.VisionBlocker = false;
                tile.GraphicMeshResource = "Tiles/Ground/Ground";
                tile.PhysicMeshResource = "Tiles/Ground/Ground";
                tile.TextureResource = "Tiles/Ground/Ground_Grass";
                break;
        }
        tile.Shadow = false;
        tile.Luminosity = 1f;
        tile.Explored = true;
    }
}