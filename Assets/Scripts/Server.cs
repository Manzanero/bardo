#pragma warning disable 0649

using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class Server : MonoBehaviour
{
    public static bool serverReady;
    public static string baseUrl;
    private static string _username;
    private static string _password;

    public static void SetBaseUrl()
    {
        var url = Application.absoluteURL;
        baseUrl = url == "" ? "http://localhost" : url.Substring(0, url.IndexOf('/', 10));
    }

    public static void SetCredentials(string username, string password)
    {
        _username = username;
        _password = password;
    }
    
    public class Response
    {
        public int status;
        public string message;
        public string date;
        public bool exception;


        public static implicit operator bool(Response value)
        {
            return !value.exception;
        }
    }

    private static string PlayerBasicAuth()
    {
        var auth = $"{_username}:{_password}";
        return "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(auth));
    }

    private static UnityWebRequest AddCommonHeaders(UnityWebRequest request)
    {
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", PlayerBasicAuth());
        return request;
    }
    
    public static UnityWebRequest GetRequest(string url)
    {
        var request = UnityWebRequest.Get(url);
        request = AddCommonHeaders(request);
        request.SendWebRequest();
        return request;
    }

    public static UnityWebRequest PostRequest<T>(string url, T data)
    {
        var request = UnityWebRequest.Post(url, "");
        var json = data is string s ? s : JsonUtility.ToJson(data);
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.SetRequestHeader("Content-Type", "application/json");
        request = AddCommonHeaders(request);
        request.SendWebRequest();
        return request;
    }

    public static UnityWebRequest DeleteRequest(string url)
    {
        var request = UnityWebRequest.Delete(url);
        request = AddCommonHeaders(request);
        request.SendWebRequest();
        return request;
    }
    
    public static T GetResponse<T>(UnityWebRequest request) where T : Response, new()
    {
        return GetResponse<T>(request, true);
    }
    
    public static T GetResponse<T>(UnityWebRequest request, bool raiseExceptions) where T : Response, new()
    {
        var jsonResponse = Encoding.Default.GetString(request.downloadHandler.data);
        var serializable = new T();
        try
        {
            try
            {
                JsonUtility.FromJsonOverwrite(jsonResponse, serializable);
            }
            catch (ArgumentException)
            {
                throw new Exception($"[Server] JSON error: {jsonResponse}");
            }
            if (serializable.status >= 300)
                throw new Exception($"[Server] Status {serializable.status}. Error: {serializable.message}. " +
                                    $"Url: {request.url}");
            if (request.result.ToString() != "Success") 
                throw new Exception($"[Server] Result: {request.result}. Body (if any): {jsonResponse}");
            if (GameMaster.debugging) Debug.Log($"[Server] message: {serializable.message}. " +
                                                $"Url: {request.url}");
            return serializable;
        }
        catch (Exception e)
        {
            if (raiseExceptions)
                throw;
            
            Debug.LogWarning(e.Message);
            serializable.exception = true;
            return serializable;
        }
    }
}