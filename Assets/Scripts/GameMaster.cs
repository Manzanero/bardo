using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameMaster : MonoBehaviour
{
    public GameObject mapPrefab;
    public GameObject tilePrefab;
    public GameObject entityPrefab;
    
    public GameObject navigationPanel;

    public static bool debugging = true;
    public static string player;
    public static string password;
    public static bool master;
    
    public Campaign campaign;
    public List<Action> actionsToDo = new List<Action>();
    public List<Action> actionsDone = new List<Action>();
    public bool mouseOverUi;
    public bool currentSelectedGameObject;
    
    public static GameMaster instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (!master)
            navigationPanel.SetActive(false);

        // deletes placeholder map
        foreach(Transform child in campaign.mapsParent) Destroy(child.gameObject);
    }

    private void Update()
    {
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
    
    public static class ActionNames
    {
        // entity actions
        public const string ChangeEntity = "ChangeEntity";
        public const string DeleteEntity = "DeleteEntity";
    }
    
    public void RegisterAction(Action action){
        actionsDone.Add(action);
    }

    public void ResolveAction(Action action)
    {

        try
        {
            if (action.map != null && action.map != campaign.ActiveMap.mapId)
                return;
            
            switch (action.name)
            {
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

    private void ChangeEntity(Entity.SerializableEntity serializableEntity)
    {
        var map = campaign.ActiveMap;
        var entity = map.entities.FirstOrDefault(x => x.id == serializableEntity.id);
        if (entity == null)
        {
            entity = Instantiate(entityPrefab, map.entitiesParent).GetComponent<Entity>();
            entity.map = map;  
            map.entities.Add(entity);
        }
        entity.Deserialize(serializableEntity);
    }

    private void DeleteEntity(Entity.SerializableEntity serializableEntity)
    {
        var map = campaign.ActiveMap;
        var entity = map.entities.FirstOrDefault(x => x.id == serializableEntity.id);
        if (entity == null)
            return;
        
        map.entities.Remove(entity);
        map.selectedEntities.Remove(entity);
        Destroy(entity.gameObject);
    }
}