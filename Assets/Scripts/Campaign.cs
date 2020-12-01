#pragma warning disable 0649

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class Campaign : MonoBehaviour
{
    public Transform mapsParent;
    public List<Map> mapsLoaded;

    public string campaignName = "";
    public List<string> mapNames = new List<string>();

    private GameMaster _gameMaster;
    private Server _server;
    private bool _campaignLoaded;
    private const float GetActionsPeriod = 3.0f;
    private const float PostActionsPeriod = 1.0f;
    private string _actionsFrom;
    private Map _activeMap;

    public Map ActiveMap
    {
        get => _activeMap;
        set 
        {
            if (_activeMap != null) _activeMap.gameObject.SetActive(false);
            _activeMap = value;
            _activeMap.gameObject.SetActive(true);
        }
    }

    private void Start()
    {
        _gameMaster = GameMaster.instance;
        _server = Server.instance;
        campaignName = "test";
        if (!_server.serverEnabled) 
            return; 
        StartCoroutine(LoadCampaign());
        StartCoroutine(GetActions());
        StartCoroutine(PostActions());
    }

    public Map Map(string mapName)
    {
        return mapsLoaded.FirstOrDefault(m => m.name == mapName);
    }

    public IEnumerator ChangeActiveMap(string mapName)
    {
        var mapToActive = Map(mapName);
        if (mapToActive != null)
        {
            ActiveMap = mapToActive;
            yield break;
        } 
        
        StartCoroutine(LoadMap(mapName));
        StartCoroutine(SaveProperty("ACTIVE_MAP", mapName));
        while (Map(mapName) == null)
            yield return null;

        var previousName = ActiveMap.name;
        ActiveMap = Map(mapName);
        
        // unload previous
        var map = Map(previousName);
        if (map != null)
        {
            Destroy(Map(previousName).gameObject);
            mapsLoaded.Remove(map);
        }
    }
    
    public IEnumerator SaveProperty(string propertyName, string propertyValue)
    {
        var url = $"{_server.baseUrl}/world" +
                  $"/campaign/{_gameMaster.campaign.campaignName}" +
                  $"/property/{propertyName}/save";
        var request = _server.PostRequest(url, propertyValue);
        while (!request.isDone)
            yield return null;
        
        _server.GetResponse<Server.Response>(request);
    }
    
    public IEnumerator DefaultProperty(string propertyName, string propertyValue)
    {
        var url = $"{_server.baseUrl}/world" +
                  $"/campaign/{_gameMaster.campaign.campaignName}" +
                  $"/property/{propertyName}/default";
        var request = _server.PostRequest(url, propertyValue);
        while (!request.isDone)
            yield return null;
        
        _server.GetResponse<Server.Response>(request);
    }

    [Serializable] private class CampaignResponse : Server.Response
    {
        public CampaignData campaign;
        
        [Serializable] public class CampaignData
        {
            public List<CampaignProperty> properties;
            public List<string> maps;
            
            [Serializable] public class CampaignProperty
            {
                public string name;
                public string value;
            }
        }
    }

    private IEnumerator LoadCampaign()
    {
        var url = $"{_server.baseUrl}/world/campaign/{campaignName}";
        var campaignRequest = _server.GetRequest(url);
        while (!campaignRequest.isDone) 
            yield return null;
        
        var campaignResponse = _server.GetResponse<CampaignResponse>(campaignRequest);
        var campaign = campaignResponse.campaign;
        var property = campaign.properties.FirstOrDefault(p => p.name == "ACTIVE_MAP");
        var mapToActive = property != null ? property.value : campaign.maps[0];
        if (!campaign.maps.Contains(mapToActive))
        {
            Debug.LogWarning($"Active map (name={mapToActive}) for user doesn't exist");
            mapToActive = campaign.maps[0];
        }
        mapNames = campaign.maps;

        // load active map
        StartCoroutine(LoadMap(mapToActive));
        while (Map(mapToActive) == null) 
            yield return null;
        
        ActiveMap = Map(mapToActive);
        _actionsFrom = $"map/{mapToActive}";
        _campaignLoaded = true;
    }
    
    [Serializable] private class MapResponse : Server.Response
    {
        public Map.SerializableMap map;
    }

    public IEnumerator LoadMap(string mapName)
    {
        var mapUrl = $"{_server.baseUrl}/world" +
                     $"/campaign/{_gameMaster.campaign.campaignName}" +
                     $"/map/{mapName}";
        var mapRequest = _server.GetRequest(mapUrl);
        while (!mapRequest.isDone) 
            yield return null;
        
        var mapResponse = _server.GetResponse<MapResponse>(mapRequest);
        var map = Instantiate(_gameMaster.mapPrefab, mapsParent).GetComponent<Map>();
        map.name = mapResponse.map.name;
        mapsLoaded.Add(map);
        map.Deserialize(mapResponse.map);
        
        // get actions not saved
        var url = $"{_server.baseUrl}/world/campaign/{_gameMaster.campaign.campaignName}/actions/from/map/{mapName}";
        var request = _server.GetRequest(url);
        while (!request.isDone) 
            yield return null;
            
        var response = _server.GetResponse<ActionsResponse>(request);
        _actionsFrom = $"date/{response.date}";
        foreach (var action in response.actions)
            _gameMaster.ResolveAction(action);
    }
    
    public IEnumerator DeleteMap(string mapName)
    {
        var url = $"{_server.baseUrl}/world" +
                  $"/campaign/{_gameMaster.campaign.campaignName}" +
                  $"/map/{mapName}/delete";
        var request = _server.DeleteRequest(url);
        while (!request.isDone)
            yield return null;
        
        _server.GetResponse<Server.Response>(request);
        var map = Map(mapName);
        if (map != null)
        {
            mapsLoaded.Remove(map);
            Destroy(Map(mapName).gameObject);
        }
        mapNames.Remove(mapName);
    }
    
    [Serializable] private class ActionsResponse : Server.Response
    {
        public List<Action> actions;
    }
 
    private IEnumerator GetActions()
    {
        while (_server.serverEnabled)
        { 
            while (!_campaignLoaded)
                yield return null;
            yield return new WaitForSecondsRealtime(GetActionsPeriod);
            
            var url = $"{_server.baseUrl}/world/campaign/{_gameMaster.campaign.campaignName}/actions/from/{_actionsFrom}";
            var request = _server.GetRequest(url);
            while (!request.isDone) 
                yield return null;
            
            var response = _server.GetResponse<ActionsResponse>(request);
            _actionsFrom = $"date/{response.date}";
            _gameMaster.actionsToDo.AddRange(response.actions);
            yield return null;
        }
    }
    
    [Serializable] private class ActionsRequestBody
    {
        public List<Action> actions;
    }
    
    private IEnumerator PostActions()
    {
        while (_server.serverEnabled)
        {
            while (!_campaignLoaded)
                yield return null;
            while (_gameMaster.actionsDone.Count == 0)
                yield return null;
            yield return new WaitForSecondsRealtime(PostActionsPeriod);

            var data = new ActionsRequestBody {actions = _gameMaster.actionsDone};
            var len = data.actions.Count;
            var url = $"{_server.baseUrl}/world/campaign/{_gameMaster.campaign.campaignName}/actions/add";
            var request = _server.PostRequest(url, data);
            while (!request.isDone)
                yield return null;

            _gameMaster.actionsDone.RemoveRange(0, len);
            _server.GetResponse<ActionsResponse>(request);
            yield return null;
        }
    }

}