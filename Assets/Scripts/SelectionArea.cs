﻿using System;
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
        var activeMap = _gm.campaign.activeMap;
        if (!activeMap)
            return;

        if (Input.GetMouseButtonUp(0))
        {
            meshRenderer.enabled = false;
        }

        _cachedTile = activeMap.mouseTile ? activeMap.mouseTile : _cachedTile;

        if (Input.GetMouseButtonDown(0) && activeMap.mouseOverTile && !_gm.mouseOverUi)
        {
            _startTile = _cachedTile;
            meshRenderer.enabled = true;
            // foreach (var tile in activeMap.selectedTiles) DeselectionTint(tile);
            activeMap.selectedTiles.Clear();
            activeMap.selectedEntities.Clear();
        }
        
        if (!_startTile)
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

        _startTile = null;
        meshRenderer.enabled = false;
        var minX = Math.Min(startPos.x, endPos.x);
        var minY = Math.Min(startPos.y, endPos.y);
        var maxX = Math.Max(startPos.x, endPos.x);
        var maxY = Math.Max(startPos.y, endPos.y);
        for (var x = minX; x <= maxX; x += 1)
        for (var y = minY; y <= maxY; y += 1)
            activeMap.selectedTiles.Add(activeMap.tiles[x, y]);

        // foreach (var tile in activeMap.selectedTiles) SelectionTint(tile);
        
        var tempSelect = activeMap.entities.Where(
            entity => activeMap.selectedTiles.Contains(entity.tile)).ToList();
        activeMap.selectedEntities = tempSelect;
    }

    // private void SelectionTint(Tile t)
    // {
    //     t.meshRenderer.material.color = new Color(Color.cyan.r * 1.2f, Color.cyan.g * 1.2f, Color.cyan.b * 1.2f);
    // }
    //
    // private void DeselectionTint(Tile t)
    // {
    //     t.meshRenderer.material.color = new Color(t.Luminosity, t.Luminosity, t.Luminosity);
    // }
}