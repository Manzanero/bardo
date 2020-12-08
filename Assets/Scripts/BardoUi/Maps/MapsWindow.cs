using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BardoUi.Maps
{
    public class MapsWindow : MonoBehaviour
    {
        public Transform mapItemsParent;
        public GameObject mapItemPrefab;

        public InputField currentMapInput;
        public Button saveButton;
        public Button saveAsButton;
    
        private GameMaster _gm;
        private string _cachedId;
        private int _cachedMapsCount;

        private void Start()
        {
            _gm = GameMaster.instance;
        
            saveButton.onClick.AddListener(SaveButton);
            saveAsButton.onClick.AddListener(SaveAsButton);
        
            // remove previous items
            foreach (Transform child in mapItemsParent)
                Destroy(child.gameObject);
            
            StartCoroutine(GetMapList());
        }

        private IEnumerator GetMapList()
        {
            while (!_gm.campaign) 
                yield return null;
            while (_gm.campaign.mapsLoaded.Count == 0)
                yield return null;
            while (!_gm.campaign.ActiveMap)
                yield return null;
            
            var campaign = _gm.campaign;
            
            _cachedId = campaign.ActiveMap.mapId;
            _cachedMapsCount = campaign.campaignMaps.Count;
            
            while (true)
            {
                foreach (Transform child in mapItemsParent)
                    Destroy(child.gameObject);
                
                currentMapInput.text = campaign.ActiveMap.name;
                foreach (var mapData in campaign.campaignMaps)
                {
                    var mapItem = Instantiate(mapItemPrefab, mapItemsParent).GetComponent<MapItem>();
                    mapItem.mapName.text = $"{mapData.name}";
                    mapItem.selected.SetActive(campaign.ActiveMap.mapId == mapData.id);
                    mapItem.deleteMapButton.gameObject.SetActive(campaign.ActiveMap.mapId != mapData.id);
                    mapItem.changeMapButton.gameObject.SetActive(campaign.ActiveMap.mapId != mapData.id);
                    mapItem.deleteMapButton.onClick.AddListener(delegate { DeleteMapButton(mapData.id); });
                    mapItem.changeMapButton.onClick.AddListener(delegate { ChangeMapButton(mapData.id); });
                }
                
                while (_cachedId == campaign.ActiveMap.mapId && _cachedMapsCount == campaign.campaignMaps.Count)
                    yield return null;
                
                _cachedId = campaign.ActiveMap.mapId;
                _cachedMapsCount = campaign.campaignMaps.Count;
            }
        }
    
        private void SaveButton()
        {
            StartCoroutine(SaveMapRequest(false));
        }
    
        private void SaveAsButton()
        {
            StartCoroutine(SaveMapRequest(true));
        }
    
        private IEnumerator SaveMapRequest(bool asNew)
        {
            var newName = currentMapInput.text;
            var campaign = _gm.campaign;
            var map = campaign.ActiveMap;
            var mapId = Guid.NewGuid().ToString().Substring(0, 8);
            if (!asNew) mapId = map.mapId;
            var data = map.Serialize();
            data.name = newName;
            data.mapId = mapId;
            var url = $"{Server.baseUrl}/world" +
                      $"/campaign/{Campaign.campaignId}" +
                      $"/map/{mapId}/save";
            var request = Server.PostRequest(url, data);
            while (!request.isDone)
                yield return null;
            
            Server.GetResponse<Server.Response>(request);
            map.name = newName;
            map.mapId = mapId;
            campaign.campaignMaps.RemoveAll(x => x.id == mapId);
            campaign.campaignMaps.Add(new Campaign.MapData {name = newName, id = mapId});
            campaign.campaignMaps.Sort(
                (p, q) => string.Compare(p.name, q.name, StringComparison.Ordinal));
        
            Debug.Log($"Map (name={newName}, id={mapId}) saved");
        }
    
    
        private void DeleteMapButton(string mapId)
        {
            StartCoroutine(_gm.campaign.DeleteMap(mapId));
        }
    
        private void ChangeMapButton(string mapId)
        {
            StartCoroutine(_gm.campaign.ChangeActiveMap(mapId));
        }
    }
}