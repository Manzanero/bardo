#pragma warning disable 0649

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ServerConnector : MonoBehaviour
{
    public bool serverEnabled = true;

    private const float ApiCheckMaxTime = 1.0f;
    private float _apiCheckCountdown = 1.0f;
    private const string BaseUrl = "http://localhost";
    // private const string BaseUrl = "https://manzanero.pythonanywhere.com";
    private GameMaster _gameMaster;
    
    private bool _campaignLoaded;
    private string _actionsFrom;

    private void Start()
    {
        _gameMaster = GameMaster.instance;
        if (!serverEnabled) { Debug.Log("[Server] Not enabled"); return; }
        StartCoroutine(GetCampaign());
        StartCoroutine(GetActions());
        StartCoroutine(PostActions());
    }
    
    [Serializable] private class CampaignProperty
    {
        public string name;
        public string value;
    }
    
    [Serializable] private class CampaignData
    {
        public List<CampaignProperty> properties;
        public List<string> maps;
    }
    
    [Serializable] private class CampaignResponse : Response
    {
        public CampaignData campaign;
    }
    
    [Serializable] private class MapResponse : Response
    {
        public Map.SerializableMap map;
    }

    private IEnumerator GetCampaign()
    {
        var url = $"{BaseUrl}/world/campaign/{_gameMaster.campaign.activeCampaign}";
        var campaignRequest = GetRequest(url);
        while (!campaignRequest.isDone) 
            yield return null;
        
        var campaignResponse = GetResponse<CampaignResponse>(campaignRequest);
        var campaign = campaignResponse.campaign;
        var property = campaign.properties.FirstOrDefault(p => p.name == "ACTIVE_MAP");
        var mapToActive = property != null ? property.value : campaign.maps[0];
        
        // load active map
        var mapUrl = $"{BaseUrl}/world/campaign/{_gameMaster.campaign.activeCampaign}/map/{mapToActive}";
        var mapRequest = GetRequest(mapUrl);
        while (!mapRequest.isDone) 
            yield return null;
        
        var mapResponse = GetResponse<MapResponse>(mapRequest);
        _gameMaster.LoadMap(mapResponse.map);
        _actionsFrom = $"map/{mapResponse.map.name}";
        _campaignLoaded = true;
    }
    
    [Serializable] private class ActionsResponse : Response
    {
        public List<Action> actions;
    }
 
    private IEnumerator GetActions()
    {
        while (serverEnabled)
        {
            while (!_campaignLoaded || _apiCheckCountdown > 0)
            {
                _apiCheckCountdown -= Time.deltaTime;
                yield return null;
            }
            _apiCheckCountdown = ApiCheckMaxTime;
            
            var url = $"{BaseUrl}/world/campaign/{_gameMaster.campaign.activeCampaign}/actions/from/{_actionsFrom}";
            var request = GetRequest(url);
            while (!request.isDone) 
                yield return null;
            
            var response = GetResponse<ActionsResponse>(request);
            _actionsFrom = $"date/{response.date}";
            _gameMaster.actionsToDo.AddRange(response.actions);
            yield return null;
        }
    }
    
    [Serializable] private class ActionsRequestBody
    {
        // ReSharper disable once NotAccessedField.Local
        public List<Action> actions;
    }
    
    private IEnumerator PostActions()
    {
        while (serverEnabled)
        {
            while (!_campaignLoaded || _gameMaster.actionsDone.Count == 0)
                yield return null;

            var data = JsonUtility.ToJson(new ActionsRequestBody {actions = _gameMaster.actionsDone});
            _gameMaster.actionsDone.RemoveRange(0, _gameMaster.actionsDone.Count);

            var url = $"{BaseUrl}/world/campaign/{_gameMaster.campaign.activeCampaign}/actions/add";
            var request = PostRequest(url, data);
            while (!request.isDone)
                yield return null;

            GetResponse<ActionsResponse>(request);
            yield return null;
        }
    }

    private class Response
    {
        public string status;
        public string message;
        public string date;
    }

    private string PlayerBasicAuth()
    {
        var auth = $"{_gameMaster.player}:{_gameMaster.password}";
        return "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(auth));
    }

    private UnityWebRequest GetRequest(string url)
    {
        var request = UnityWebRequest.Get(url);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", PlayerBasicAuth());
        request.SendWebRequest();
        return request;
    }

    private UnityWebRequest PostRequest(string url, string data)
    {
        var request = UnityWebRequest.Post(url, "");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(data));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", PlayerBasicAuth());
        request.SendWebRequest();
        return request;
    }
    
    private static T GetResponse<T>(UnityWebRequest request) where T : Response, new()
    {
        var jsonResponse = Encoding.Default.GetString(request.downloadHandler.data);
        var serializable = new T();
        try { JsonUtility.FromJsonOverwrite(jsonResponse, serializable); }
        catch (ArgumentException) { throw new Exception($"[Server] JSON error: {jsonResponse}"); }
        if (request.result.ToString() != "Success") 
            throw new Exception($"[Server] Result: {request.result}. Body (if any): {jsonResponse}");
        if (serializable.status != "ok") 
            throw new Exception($"[Server] Status error: {serializable.message}");
        // Debug.Log($"[Server] message: {serializable.message}");
        return serializable;
    }
}