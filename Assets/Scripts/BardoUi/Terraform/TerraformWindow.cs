#pragma warning disable 0649

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BardoUi.Terraform
{
    public class TerraformWindow : MonoBehaviour
    {
        public Transform topTilesParent;
        public Button topTilesTab;
        public Transform platformTilesParent;
        public Button platformTilesTab;
        public Transform groundTilesParent;
        public Button groundTilesTab;
        public Transform gapTilesParent;
        public Button gapTilesTab;
        public Transform bottomTilesParent;
        public Button bottomTilesTab;
        public Transform floodedTilesParent;
        public Button floodedTilesTab;

        public Text selectionText;
        public Button consolidateButton;
        public Button walkableButton;
        public Button nonWalkableButton;
        public Button transparentButton;
        public Button visionBlockerButton;

        private GameMaster _gm;
        private Campaign _campaign;
        private List<Tile> _cachedTiles = new List<Tile>();

        private void ShowPanel(string tab)
        {
            topTilesParent.gameObject.SetActive(false);
            platformTilesParent.gameObject.SetActive(false);
            groundTilesParent.gameObject.SetActive(false);
            gapTilesParent.gameObject.SetActive(false);
            bottomTilesParent.gameObject.SetActive(false);
            floodedTilesParent.gameObject.SetActive(false);
            switch (tab)
            {
                case "top": topTilesParent.gameObject.SetActive(true); break;
                case "platform": platformTilesParent.gameObject.SetActive(true); break;
                case "ground": groundTilesParent.gameObject.SetActive(true); break;
                case "gap": gapTilesParent.gameObject.SetActive(true); break;
                case "bottom": bottomTilesParent.gameObject.SetActive(true); break;
                case "flooded": floodedTilesParent.gameObject.SetActive(true); break;
            }
        }
        
        private void Start()
        {
            _gm = GameMaster.instance;
            _campaign = _gm.campaign;
            
            topTilesTab.onClick.AddListener(delegate { ShowPanel("top"); });
            platformTilesTab.onClick.AddListener(delegate { ShowPanel("platform"); });
            groundTilesTab.onClick.AddListener(delegate { ShowPanel("ground"); });
            gapTilesTab.onClick.AddListener(delegate { ShowPanel("gap"); });
            bottomTilesTab.onClick.AddListener(delegate { ShowPanel("bottom"); });
            floodedTilesTab.onClick.AddListener(delegate { ShowPanel("flooded"); });
            
            consolidateButton.onClick.AddListener(Consolidate);
            walkableButton.onClick.AddListener(Walkable);
            nonWalkableButton.onClick.AddListener(NonWalkable);
            transparentButton.onClick.AddListener(Transparent);
            visionBlockerButton.onClick.AddListener(VisionBlocker);
        }

        private void Update()
        {
            if (!_campaign.activeMap)
                return;

            if (!_campaign.activeMap.selectedTiles.Any())
            {
                selectionText.text = "-";
                return;
            }
            
            var selectedTiles = _campaign.activeMap.selectedTiles;
            selectionText.text = $"x: {selectedTiles.First().Position.x}, y: {selectedTiles.First().Position.y} -> " +
                                 $"x: {selectedTiles.Last().Position.x}, y: {selectedTiles.Last().Position.y}";
        }

        public void ChangeTiles(TileBlueprint tileBp)
        { 
            foreach (var tile in _campaign.activeMap.selectedTiles)
            {
                tile.PhysicMeshResource = tileBp.physicMeshResource;
                tile.Altitude = tileBp.altitude;
                tile.GraphicMeshResource = tileBp.graphicMeshResource;
                tile.TextureResource = tileBp.textureResource;
                tile.Translucent = tileBp.translucent;
            }

            _cachedTiles.AddRange(_campaign.activeMap.selectedTiles);
            _cachedTiles = _cachedTiles.Distinct().ToList();
        }

        private void Walkable()
        {
            foreach (var tile in _campaign.activeMap.selectedTiles) tile.Walkable = true;
            _cachedTiles.AddRange(_campaign.activeMap.selectedTiles);
            _cachedTiles = _cachedTiles.Distinct().ToList();
        }

        private void NonWalkable()
        {
            foreach (var tile in _campaign.activeMap.selectedTiles) tile.Walkable = false;
            _cachedTiles.AddRange(_campaign.activeMap.selectedTiles);
            _cachedTiles = _cachedTiles.Distinct().ToList();
        }

        private void Transparent()
        {
            foreach (var tile in _campaign.activeMap.selectedTiles) tile.VisionBlocker = false;
            _cachedTiles.AddRange(_campaign.activeMap.selectedTiles);
            _cachedTiles = _cachedTiles.Distinct().ToList();
        }

        private void VisionBlocker()
        {
            foreach (var tile in _campaign.activeMap.selectedTiles) tile.VisionBlocker = true;
            _cachedTiles.AddRange(_campaign.activeMap.selectedTiles);
            _cachedTiles = _cachedTiles.Distinct().ToList();
        }

        private void Consolidate()
        {
            var tiles = _cachedTiles.Select(x => x.Serialize()).ToList();
            if (!_cachedTiles.Any()) return;
            
            _gm.RegisterAction(new Action
            {
                map = _campaign.activeMap.id,
                name = GameMaster.ActionNames.ChangeTiles,
                tiles = tiles
            });
            
            _cachedTiles.Clear();
        }
    }
}