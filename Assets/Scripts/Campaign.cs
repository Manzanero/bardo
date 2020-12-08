#pragma warning disable 0649

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Campaign : MonoBehaviour
{
    public Transform mapsParent;
    public List<Map> mapsLoaded;


    public static string campaignId;
    public static string campaignName;
    
    public List<MapData> campaignMaps = new List<MapData>();
    
    [Serializable] public class MapData
    {
        public string name;
        public string id;
    }
    
    private GameMaster _gm;
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
        _gm = GameMaster.instance;
        if (!Server.serverReady)
        {
            GameMaster.player = "admin";
            GameMaster.password = "admin";
            GameMaster.master = true;
            campaignName = "test";
            campaignId = "5rhgb76y";
            Server.serverReady = true;     
        }
        StartCoroutine(LoadCampaign());
        StartCoroutine(UpdateActions());
    }

    public Map Map(string mapId)
    {
        return mapsLoaded.FirstOrDefault(m => m.mapId == mapId);
    }

    public IEnumerator ChangeActiveMap(string mapId)
    {
        var previousId = ActiveMap.mapId;
        var mapToActive = Map(mapId);
        if (mapToActive)
        {
            ActiveMap = mapToActive;
            yield break;
        } 
        
        StartCoroutine(LoadMap(mapId));
        StartCoroutine(SaveProperty("ACTIVE_MAP", mapId));
        while (!Map(mapId))
            yield return null;

        ActiveMap = Map(mapId);
        
        // unload previous
        var map = Map(previousId);
        if (map)
        {
            mapsLoaded.Remove(map);
            Destroy(map.gameObject);
        }
    }
    
    public IEnumerator SaveProperty(string propertyName, string propertyValue)
    {
        var url = $"{Server.baseUrl}/world/campaign/{campaignId}/property/{propertyName}/save";
        var request = Server.PostRequest(url, propertyValue);
        while (!request.isDone)
            yield return null;
        
        Server.GetResponse<Server.Response>(request);
    }
    
    public IEnumerator DefaultProperty(string propertyName, string propertyValue)
    {
        var url = $"{Server.baseUrl}/world" +
                  $"/campaign/{campaignId}" +
                  $"/property/{propertyName}/default";
        var request = Server.PostRequest(url, propertyValue);
        while (!request.isDone)
            yield return null;
        
        Server.GetResponse<Server.Response>(request);
    }

    [Serializable] public class CampaignResponse : Server.Response
    {
        public CampaignData campaign;
        
        [Serializable] public class CampaignData
        {
            public List<CampaignProperty> properties;
            public List<MapData> maps;
            
            [Serializable] public class CampaignProperty
            {
                public string name;
                public string value;
            }
        }
    }

    private IEnumerator LoadCampaign()
    {
        while (!Server.serverReady)
            yield return null;
        
        var url = $"{Server.baseUrl}/world/campaign/{campaignId}";
        var campaignRequest = Server.GetRequest(url);
        while (!campaignRequest.isDone) 
            yield return null;
        
        var campaignResponse = Server.GetResponse<CampaignResponse>(campaignRequest);
        var campaign = campaignResponse.campaign;
        
        // get active map
        var property = campaign.properties.FirstOrDefault(p => p.name == "ACTIVE_MAP");
        var mapToActive = property != null ? property.value : campaign.maps[0].id;
        campaignMaps = campaign.maps;
        var mapIds = campaignMaps.Select(x => x.id).ToList();
        if (!mapIds.Contains(mapToActive))
        {
            Debug.LogWarning($"Active map (name={mapToActive}) for user doesn't exist");
            mapToActive = campaign.maps[0].id;
        }

        // load active map
        StartCoroutine(LoadMap(mapToActive));
        while (Map(mapToActive) == null) 
            yield return null;
        
        ActiveMap = Map(mapToActive);
        _campaignLoaded = true;
    }
    
    [Serializable] private class MapResponse : Server.Response
    {
        public Map.SerializableMap map;
    }
    
    [Serializable] private class MapPropertiesResponse : Server.Response
    {
        public List<Map.MapProperty> properties;
    }

    public IEnumerator LoadMap(string mapId)
    {
        var mapRequest = Server.GetRequest($"{Server.baseUrl}/world/campaign/{campaignId}/map/{mapId}");
        while (!mapRequest.isDone) 
            yield return null;
        
        var mapResponse = Server.GetResponse<MapResponse>(mapRequest);
        var map = Instantiate(_gm.mapPrefab, mapsParent).GetComponent<Map>();
        map.Deserialize(mapResponse.map);

        // get actions not saved
        var actionsRequest = Server.GetRequest($"{Server.baseUrl}/world/campaign/{campaignId}/actions/from/map/{mapId}");
        while (!actionsRequest.isDone) 
            yield return null;
            
        var actionsResponse = Server.GetResponse<ActionsResponse>(actionsRequest);
        ActiveMap = map;
        foreach (var action in actionsResponse.actions)
            _gm.ResolveAction(action);
        
        // get map properties
        var request = Server.GetRequest($"{Server.baseUrl}/world/campaign/{campaignId}/map/{mapId}/properties");
        while (!request.isDone) 
            yield return null;
        
        var response = Server.GetResponse<MapPropertiesResponse>(request);
        map.properties = response.properties;
        foreach (var entity in map.entities)
            entity.RefreshSharedProperties();
        
        mapsLoaded.Add(map);
        _actionsFrom = response.date;
    }
    
    public IEnumerator DeleteMap(string mapId)
    {
        var request = Server.DeleteRequest($"{Server.baseUrl}/world/campaign/{campaignId}/map/{mapId}/delete");
        while (!request.isDone)
            yield return null;
        
        Server.GetResponse<Server.Response>(request);
        var map = Map(mapId);
        if (map)
        {
            mapsLoaded.Remove(map);
            Destroy(map.gameObject);
        }
        campaignMaps.RemoveAll(x => x.id == mapId);
    }
 
    [Serializable] private class ActionsRequestBody
    {
        public List<Action> actions;
    }
    
    [Serializable] private class ActionsResponse : Server.Response
    {
        public List<Action> actions;
    }
    
    private IEnumerator UpdateActions()
    {
        while (!_campaignLoaded)
            yield return null;
        
        while (Server.serverReady)
        { 
            yield return new WaitForSecondsRealtime(PostActionsPeriod);

            var data = new ActionsRequestBody {actions = _gm.actionsDone};
            var len = data.actions.Count;
            var url = $"{Server.baseUrl}/world/campaign/{campaignId}/actions/from/date/{_actionsFrom}";
            var request = Server.PostRequest(url, data);
            while (!request.isDone)
                yield return null;

            var response = Server.GetResponse<ActionsResponse>(request, false);
            if (!response)
                continue;
            
            _gm.actionsDone.RemoveRange(0, len);
            _gm.actionsToDo.AddRange(response.actions);
            _actionsFrom = response.date;
        }
    }

}