using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SelectionArea : MonoBehaviour
{
    public MeshRenderer meshRenderer;

    private GameMaster _gm;
    private Tile _startTile;
    private Tile _cachedTile;

    private void Start()
    {   
        _gm = GameMaster.instance;
        meshRenderer.enabled = false;
    }

    private void Update()
    {
        var activeMap = _gm.campaign.ActiveMap;
        if (ReferenceEquals(activeMap, null))
            return;

        if (Input.GetMouseButtonUp(0))
            meshRenderer.enabled = false;
        
        _cachedTile = activeMap.mouseTile ? activeMap.mouseTile : _cachedTile;

        if (Input.GetMouseButtonDown(0))
        {
            if (ReferenceEquals(activeMap.mouseTile, null))
                return;
            
            _startTile = _cachedTile;
            meshRenderer.enabled = true;
            activeMap.selectedTiles = new List<Tile>();
            activeMap.selectedEntities = new List<Entity>();

        }
        
        if (ReferenceEquals(_startTile, null))
            return;
        
        var startPos = _startTile.Position;
        var endPos = _cachedTile.Position;
        var selectionPosX = (startPos.x + endPos.x) / 2f;
        var selectionPosY = (startPos.y + endPos.y) / 2f;
        var scaleX = Mathf.Abs(endPos.x - startPos.x) + 0.875f;
        var scaleY = Mathf.Abs(endPos.y - startPos.y) + 0.875f;
        var selT = transform;
        selT.position = new Vector3(selectionPosX, 0, selectionPosY);
        selT.localScale = new Vector3(scaleX, 2.2f, scaleY);

        if (!Input.GetMouseButtonUp(0)) 
            return;
        
        meshRenderer.enabled = false;
        var minX = Math.Min(startPos.x, endPos.x);
        var minY = Math.Min(startPos.y, endPos.y);
        var maxX = Math.Max(startPos.x, endPos.x);
        var maxY = Math.Max(startPos.y, endPos.y);
        for (var x = minX; x <= maxX; x += 1) 
        for (var y = minY; y <= maxY; y += 1)
            activeMap.selectedTiles.Add(activeMap.tiles[x, y]);

        foreach (var entity in activeMap.entities.Where(
            entity => activeMap.selectedTiles.Contains(entity.tile)))
        {
            activeMap.selectedEntities.Add(entity);
        }
    }
}