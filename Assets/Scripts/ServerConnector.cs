using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ServerConnector : MonoBehaviour
{
    public bool serverEnabled;

    private const float ApiCheckMaxTime = 1.0f;
    private float _apiCheckCountdown = 1.0f;
    private const string BaseUrl = "http://localhost";
    // private const string BaseUrl = "https://manzanero.pythonanywhere.com";
    private string _fromDate = "beginning";
    private bool _firstLoad = true;
    private GameMaster _gameMaster;

    private void Start()
    {
        _gameMaster = GameMaster.instance;
    }

    private void Update()
    {
        if (!serverEnabled)
            return;
        
        _apiCheckCountdown -= Time.deltaTime;
        if (!(_apiCheckCountdown <= 0)) 
            return;
        _apiCheckCountdown = ApiCheckMaxTime;
        
        // receive actions to do
        StartCoroutine(GetActions());
        
        // send actions to do
        StartCoroutine(PostActions());
    }
    
    [Serializable]
    private class MapResponse
    {
        public string status;
        public string message;
        public string date;
        public List<Action> actions;
    }
    
    [Serializable]
    private class ActionsResponse
    {
        public string status;
        public string message;
        public string date;
        public List<Action> actions;
    }
 
    private IEnumerator GetActions()
    {
        using (var request = UnityWebRequest.Get($"{BaseUrl}/world/{_gameMaster.activeCampaign}/{_gameMaster.activeMap.name}/actions/from/{_fromDate}"))
        {
            yield return request.SendWebRequest();
            while (!request.isDone) 
                yield return null;
            var response = request.downloadHandler.data;
            var jsonResponse = Encoding.Default.GetString(response);
            var objectResponse = new ActionsResponse();
            JsonUtility.FromJsonOverwrite(jsonResponse, objectResponse);
            
            if (objectResponse.status != "ok")
                throw new Exception($"[Server] {objectResponse.message}");
            Debug.Log($"[Server] {objectResponse.message}");
            
            _fromDate = objectResponse.date;
            foreach (var action in objectResponse.actions
                .Where(action => action.player != _gameMaster.player || _firstLoad))
                _gameMaster.actionsToDo.Add(action);
            _firstLoad = false;
        }
    }
    
    [Serializable]
    private class ActionsRequestBody
    {
        public List<Action> actions;
    }
    
    private IEnumerator PostActions()
    {
        while (_gameMaster.actionsDone.Count == 0) 
            yield return null;
        
        var bodyJsonString = JsonUtility.ToJson(new ActionsRequestBody {actions = _gameMaster.actionsDone});
        _gameMaster.actionsDone.RemoveRange(0, _gameMaster.actionsDone.Count);
        
        var request = new UnityWebRequest($"{BaseUrl}/world/{_gameMaster.activeCampaign}/{_gameMaster.activeMap.name}/actions/add", "POST");
        var bodyRaw = Encoding.UTF8.GetBytes(bodyJsonString);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();
        while (!request.isDone) 
            yield return null;
        var response = request.downloadHandler.data;
        var jsonResponse = Encoding.Default.GetString(response);
        var objectResponse = new ActionsResponse();
        JsonUtility.FromJsonOverwrite(jsonResponse, objectResponse);
        
        if (objectResponse.status != "ok")
            throw new Exception($"[Server] {objectResponse.message}");
        Debug.Log($"[Server] {objectResponse.message}");
    }
}