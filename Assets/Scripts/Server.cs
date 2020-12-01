#pragma warning disable 0649

using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class Server : MonoBehaviour
{
    public bool serverEnabled = true;
    public string baseUrl = "http://localhost";
    // public const string BaseUrl = "https://manzanero.pythonanywhere.com";
    
    public static Server instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        Debug.Log($"[Server] Enabled: {serverEnabled}");
    }
    
    public class Response
    {
        public int status;
        public string message;
        public string date;
    }

    private static string PlayerBasicAuth()
    {
        var gameMaster = GameMaster.instance;
        var auth = $"{gameMaster.player}:{gameMaster.password}";
        return "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(auth));
    }

    public UnityWebRequest GetRequest(string url)
    {
        var request = UnityWebRequest.Get(url);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", PlayerBasicAuth());
        request.SendWebRequest();
        return request;
    }

    public UnityWebRequest PostRequest<T>(string url, T data)
    {
        var request = UnityWebRequest.Post(url, "");
        var json = data is string s ? s : JsonUtility.ToJson(data);
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", PlayerBasicAuth());
        request.SendWebRequest();
        return request;
    }

    public UnityWebRequest DeleteRequest(string url)
    {
        var request = UnityWebRequest.Delete(url);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", PlayerBasicAuth());
        request.SendWebRequest();
        return request;
    }
    
    public T GetResponse<T>(UnityWebRequest request) where T : Response, new()
    {
        var jsonResponse = Encoding.Default.GetString(request.downloadHandler.data);
        var serializable = new T();
        try
        {
            JsonUtility.FromJsonOverwrite(jsonResponse, serializable);
        }
        catch (ArgumentException)
        {
            throw new Exception($"[Server] JSON error: {jsonResponse}");
        }
        if (serializable.status >= 300) 
            throw new Exception($"[Server] Status {serializable.status}. Error: {serializable.message}");
        if (request.result.ToString() != "Success") 
            throw new Exception($"[Server] Result: {request.result}. Body (if any): {jsonResponse}");
        Debug.Log($"[Server] message: {serializable.message}");
        return serializable;
    }
}