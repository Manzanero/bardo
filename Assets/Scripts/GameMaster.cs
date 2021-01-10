using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BardoUi.Chat;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameMaster : MonoBehaviour
{
    public GameObject mapPrefab;
    public GameObject tilePrefab;
    public GameObject entityPrefab;
    
    public GameObject navigationPanel;
    public GameObject loadingImage;
    public bool loading;

    public static bool debugging = true;
    // public static bool debugging = false;
    
    public Campaign campaign;
    public List<Action> actionsToDo = new List<Action>();
    public List<Action> actionsDone = new List<Action>();
    public bool mouseOverUi;
    public GameObject currentSelectedGameObject;
    
    public static GameMaster instance;

    private void Awake()
    {
        instance = this;
        
        // deletes placeholder map
        foreach(Transform child in campaign.mapsParent) Destroy(child.gameObject);
    }

    private void Start()
    {
        if (!Server.serverReady)
        {
            Campaign.playerName = "admin";
            Campaign.playerIsMaster = true;
            const string password = "admin";
            // Campaign.playerName = "Ale";
            // Campaign.playerIsMaster = false;
            // const string password = "admin";
            
            Campaign.campaignName = "test";
            Campaign.campaignId = "5rhgb76y";
            
            Server.SetBaseUrl();
            Server.SetCredentials(Campaign.playerName, password);
            Server.serverReady = true;
        }
        
        if (!Campaign.playerIsMaster)
            navigationPanel.SetActive(false);
    }

    #region Update
    
    private void Update()
    {
        loadingImage.SetActive(loading);
        
        UpdateMouse();
        UpdateActions();
    }

    private void UpdateMouse()
    {
        var current = EventSystem.current;
        mouseOverUi = current.IsPointerOverGameObject();
        currentSelectedGameObject = current.currentSelectedGameObject;
    }

    private void UpdateActions()
    {
        foreach (var action in actionsToDo)
            ResolveAction(action);
        
        var actionsToDelete = actionsToDo.Where(a => a.done).ToList();
        if (actionsToDelete.Any())
            actionsToDo.Remove(actionsToDelete[0]);
    }

    #endregion

    #region Actions
    
    public static class ActionNames
    {
        // campaign actions
        public const string AddMessage = "AddMessage";
        
        // map actions
        public const string ChangeMap = "ChangeMap";
        public const string RefreshPermissions = "RefreshPermissions";

        // tiles actions
        public const string ChangeTiles = "ChangeTiles";
        public const string ShowTiles = "ShowTiles";
        public const string HideTiles = "HideTiles";
        
        // entity actions
        public const string ChangeEntity = "ChangeEntity";
        public const string DeleteEntity = "DeleteEntity";
    }
    
    public void RegisterAction(Action action){
        actionsDone.Add(action);
    }

    public void ResolveAction(Action action)
    {
        if (action.players.Any())
        {
            var player = campaign.playersInfo.FirstOrDefault(x => x.name == Campaign.playerName);
            if (player != null && !action.players.Contains(player.id))
                return;
        }
        
        if (action.map.Any() && action.map != campaign.activeMap.id)
            return;
        
        try
        {
            switch (action.name)
            {
                // campaign actions
                case ActionNames.AddMessage:
                    var strings = action.strings;
                    Chat.instance.NewMessage(strings[0], strings[1], strings[2], strings[3]);
                    break;
                
                // map actions
                case ActionNames.ChangeMap:
                    StartCoroutine(campaign.ChangeActiveMap(action.strings[0]));
                    break;
                case ActionNames.RefreshPermissions:
                    StartCoroutine(RefreshPermissionsCoroutine());
                    break;
                
                // tiles actions
                case ActionNames.ChangeTiles:
                    ChangeTiles(action.tiles);
                    break;
                case ActionNames.ShowTiles:
                    if (!Campaign.playerIsMaster) ShowTiles(action.strings[0]);
                    break;
                case ActionNames.HideTiles:
                    if (!Campaign.playerIsMaster) HideTiles(action.strings[0]);
                    break;
        
                // entity actions
                case ActionNames.ChangeEntity:
                    foreach (var entity in action.entities) ChangeEntity(entity);
                    break;
                case ActionNames.DeleteEntity:
                    foreach (var entity in action.entities) DeleteEntity(entity);
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Action (name={action.name}, map={action.map}) throws error: {e}");
        }
        finally
        {
            action.done = true;
        }
    }

    private void ChangeTiles(List<Tile.SerializableTile> serializableTiles)
    {
        var map = campaign.activeMap;
        foreach (var tile in serializableTiles)
            map.tiles[tile.position.x, tile.position.y].Deserialize(tile);
        
        foreach (var entity in map.entities.Where(x => x.SharedVision)) entity.ResetVision();
    }

    private void ShowTiles(string tileArray)
    {
        var map = campaign.activeMap;
        var charArray = tileArray.ToCharArray();
        for (var y = 0; y < map.Height; y += 1) 
        for (var x = 0; x < map.Width; x += 1)
            if (charArray[x + y * map.Width] == '1')
                map.tiles[x, y].Explored = true;

        foreach (var entity in map.entities.Where(x => x.SharedVision)) entity.ResetVision();
    }

    private void HideTiles(string tileArray)
    {
        var map = campaign.activeMap;
        var charArray = tileArray.ToCharArray();
        for (var y = 0; y < map.Height; y += 1) 
        for (var x = 0; x < map.Width; x += 1)
            if (charArray[x + y * map.Width] == '1')
                map.tiles[x, y].Explored = false;
        
        foreach (var entity in map.entities.Where(x => x.SharedVision)) entity.ResetVision();
    }

    private void ChangeEntity(Entity.SerializableEntity serializableEntity)
    {
        var map = campaign.activeMap;
        var entity = map.entities.FirstOrDefault(x => x.id == serializableEntity.id);
        if (entity == null)
        {
            entity = Instantiate(entityPrefab, map.entitiesParent).GetComponent<Entity>();
            entity.map = map;  
            map.entities.Add(entity);
        }
        entity.Deserialize(serializableEntity);
        
        foreach (var e in map.entities.Where(x => x.SharedVision)) e.ResetVision();
    }

    private void DeleteEntity(Entity.SerializableEntity serializableEntity)
    {
        var map = campaign.activeMap;
        var entity = map.entities.FirstOrDefault(x => x.id == serializableEntity.id);
        if (entity == null)
            return;
        
        map.entities.Remove(entity);
        map.selectedEntities.Remove(entity);
        Destroy(entity.gameObject);
    }

    private IEnumerator RefreshPermissionsCoroutine()
    {
        var map = campaign.activeMap;
        var request = Server.GetRequest($"{Server.baseUrl}/world/campaign/{Campaign.campaignId}/map/{map.id}/properties");
        while (!request.isDone) 
            yield return null;
        
        var response = Server.GetResponse<Campaign.MapPropertiesResponse>(request);
        map.properties = response.properties;
        
        // get shared entity properties
        foreach (var entity in map.entities)
            entity.RefreshPermissions();
    }

    #endregion
    
    public static T GetResource<T>(string path)
    {
        var res = Resources.LoadAll(path, typeof(T)).Cast<T>().ToArray();
        if (!res.Any())
            throw new Exception($"Resource not found: {path}");
        return res[0];
    }

    public static string NewId() => Guid.NewGuid().ToString().Substring(0, 8);

    public static string NowIsoDate()
    {
        var localTime = DateTime.Now;
        var localTimeAndOffset = new DateTimeOffset(localTime, TimeZoneInfo.Local.GetUtcOffset(localTime));
        var str = localTimeAndOffset.ToString("O");
        return str.Substring(0, 26) + str.Substring(27);
    }
    
    public void ReturnToStartScreen()
    {
        SceneManager.LoadScene(0); 
    }
}