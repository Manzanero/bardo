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
    
        private GameMaster _gameMaster;
        private Server _server;
        private string _cachedName;
        private int _cachedLen;

        private void Start()
        {
            _gameMaster = GameMaster.instance;
            _server = Server.instance;
        
            saveButton.onClick.AddListener(delegate { SaveButton(); });
            saveAsButton.onClick.AddListener(delegate { SaveAsButton(); });
        
            StartCoroutine(GetMapList());
        }

        private IEnumerator GetMapList()
        {
            while (_gameMaster.campaign == null) 
                yield return null;
            while (_gameMaster.campaign.mapsLoaded.Count == 0)
                yield return null;
            while (_gameMaster.campaign.ActiveMap == null)
                yield return null;
            
            while (true) 
            {
                // remove previous items
                foreach (Transform child in mapItemsParent)
                    Destroy(child.gameObject);
                                    
                currentMapInput.text = _gameMaster.campaign.ActiveMap.name;
                            
                foreach (var mapName in _gameMaster.campaign.mapNames)
                {
                    var mapItemGo = Instantiate(mapItemPrefab, mapItemsParent);
                    var mapItem = mapItemGo.GetComponent<MapItem>();
                    mapItem.mapName.text = mapName;
                    mapItem.selected.SetActive(_gameMaster.campaign.ActiveMap.name == mapName);
                    mapItem.deleteMapButton.gameObject.SetActive(currentMapInput.text != mapName);
                    mapItem.changeMapButton.gameObject.SetActive(currentMapInput.text != mapName);
                    mapItem.deleteMapButton.onClick.AddListener(delegate { DeleteMapButton(mapName); });
                    mapItem.changeMapButton.onClick.AddListener(delegate { ChangeMapButton(mapName); });
                }
                while (_cachedName == _gameMaster.campaign.ActiveMap.name && 
                       _cachedLen == _gameMaster.campaign.mapNames.Count)
                    yield return null;
                
                _cachedName = _gameMaster.campaign.ActiveMap.name;
                _cachedLen = _gameMaster.campaign.mapNames.Count;
            }
        }
    
        private void SaveButton()
        {
            var mapName = _gameMaster.campaign.ActiveMap.name;
            var newName = currentMapInput.text;
            if (_gameMaster.campaign.mapNames.Contains(newName) && mapName != newName)
            {
                currentMapInput.text = mapName;
                throw new Exception($"[Client] Map {newName} already exist");
            }
            StartCoroutine(SaveMapRequest(mapName, false));
        }
    
        private void SaveAsButton()
        {
            var mapName = _gameMaster.campaign.ActiveMap.name;
            var newName = currentMapInput.text;
            if (_gameMaster.campaign.mapNames.Contains(newName))
            {
                currentMapInput.text = mapName;
                throw new Exception($"[Client] Map {newName} already exist");
            }
            StartCoroutine(SaveMapRequest(newName, true));
        }
    
        private IEnumerator SaveMapRequest(string mapName, bool asNew)
        {
            var url = $"{_server.baseUrl}/world" +
                      $"/campaign/{_gameMaster.campaign.campaignName}" +
                      $"/map/{mapName}/save";
            var newName = currentMapInput.text;
            var data = _gameMaster.campaign.ActiveMap.Serialize();
            data.name = newName;
            var request = _server.PostRequest(url, data);
            while (!request.isDone)
                yield return null;
            _server.GetResponse<Server.Response>(request);
        
            _gameMaster.campaign.ActiveMap.name = newName;
            if (!asNew) 
                _gameMaster.campaign.mapNames.Remove(mapName);
            if (!_gameMaster.campaign.mapNames.Contains(newName)) 
                _gameMaster.campaign.mapNames.Add(newName);
            _gameMaster.campaign.mapNames.Sort();
            StartCoroutine(GetMapList());
        
            Debug.Log($"Map (name={newName}) saved");
        }
    
    
        private void DeleteMapButton(string mapName)
        {
            StartCoroutine(_gameMaster.campaign.DeleteMap(mapName));
        }
    
        private void ChangeMapButton(string mapName)
        {
            StartCoroutine(_gameMaster.campaign.ChangeActiveMap(mapName));
        }
    }
}