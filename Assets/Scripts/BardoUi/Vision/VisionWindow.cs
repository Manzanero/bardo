#pragma warning disable 0649

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BardoUi.Tokens;
using UnityEngine;
using UnityEngine.UI;

namespace BardoUi.Vision
{
    public class VisionWindow : MonoBehaviour
    {
        public Text tilesText;
        public Button showButton;
        public Button hideButton;
        public Button returnButton;

        public Transform visionItemsParent;
        public GameObject visionItemPrefab;

        private GameMaster _gm;
        private Campaign _campaign;

        private void Start()
        {
            _gm = GameMaster.instance;
            _campaign = _gm.campaign;
            showButton.onClick.AddListener(ShowTiles);
            hideButton.onClick.AddListener(HideTiles);
            returnButton.onClick.AddListener(ReturnVision);

            // delete placeholder
            foreach (Transform child in visionItemsParent) Destroy(child.gameObject);

            StartCoroutine(LoadPlayers());
        }

        private void ReturnVision()
        {
            var map = _campaign.activeMap;
            for (var y = 0; y < map.Height; y += 1)
            for (var x = 0; x < map.Width; x += 1)
            {
                map.tiles[x, y].Shadow = false;
                map.tiles[x, y].Luminosity = 1f;
                map.tiles[x, y].Explored = true;
            }

            Campaign.sharingPlayerVision = false;
            foreach (var entity in map.entities) entity.RefreshPermissions();
        }

        private IEnumerator LoadPlayers()
        {
            while (!_campaign.loaded) yield return null;

            foreach (var playerInfo in _campaign.playersInfo.Where(x => !x.master))
            {
                var playerItem = Instantiate(visionItemPrefab, visionItemsParent).GetComponent<PlayerVisionItem>();
                playerItem.playerName.text = playerInfo.name;
                playerItem.visionButton.onClick.AddListener(delegate { StartCoroutine(PlayerVision(playerInfo.id)); });
            }
        }

        private void Update()
        {
            var activeMap = _campaign.activeMap;
            if (!activeMap || !activeMap.loaded) return;

            tilesText.text = activeMap.selectedTiles.Count.ToString();
        }

        private IEnumerator PlayerVision(string playerId)
        {
            var map = _campaign.activeMap;
            var url =
                $"{Server.baseUrl}/world/campaign/{Campaign.campaignId}/map/{map.id}/properties/for/user/{playerId}";
            var request = Server.GetRequest(url);
            while (!request.isDone)
                yield return null;

            var response = Server.GetResponse<Campaign.MapPropertiesResponse>(request);
            var exploredProperty = response.properties.FirstOrDefault(p => p.name == "EXPLORED");
            if (exploredProperty != null)
            {
                var exploredChars = exploredProperty.value.ToCharArray();
                for (var y = 0; y < map.Height; y += 1)
                for (var x = 0; x < map.Width; x += 1)
                {
                    map.tiles[x, y].Shadow = true;
                    map.tiles[x, y].Luminosity = 0.5f;
                    map.tiles[x, y].Explored = exploredChars[x + y * map.Width] == '1';
                }
            }

            map.properties = response.properties;
            Campaign.sharingPlayerVision = true;
            foreach (var entity in map.entities) entity.RefreshPermissions();
            map.ResetVision();
        }

        private string GetArrayFromTiles(IEnumerable<Tile> tiles)
        {
            var map = _campaign.activeMap;
            var exploredInt = new bool[map.Height * map.Width];
            foreach (var tile in tiles) exploredInt[tile.Position.x + tile.Position.y * map.Width] = true;
            return exploredInt.Aggregate("", (current, b) => current + (b ? "1" : "0"));
        }

        private void ShowTiles()
        {
            var map = _campaign.activeMap;
            if (!map.selectedTiles.Any()) return;

            foreach (var tile in map.selectedTiles) tile.Explored = true;

            _gm.RegisterAction(new Action
            {
                map = _campaign.activeMap.id,
                name = GameMaster.ActionNames.ShowTiles,
                strings = new List<string> {GetArrayFromTiles(map.selectedTiles)}
            });
        }

        private void HideTiles()
        {
            var map = _campaign.activeMap;
            if (!map.selectedTiles.Any()) return;

            foreach (var tile in map.selectedTiles) tile.Explored = false;

            _gm.RegisterAction(new Action
            {
                map = _campaign.activeMap.id,
                name = GameMaster.ActionNames.HideTiles,
                strings = new List<string> {GetArrayFromTiles(map.selectedTiles)}
            });
        }
    }
}