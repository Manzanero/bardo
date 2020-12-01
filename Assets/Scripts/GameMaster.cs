using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    public GameObject mapPrefab;
    public GameObject tilePrefab;
    public GameObject entityPrefab;

    public string player;
    public string password;
    public bool master;
    public Campaign campaign;
    public List<Action> actionsToDo = new List<Action>();
    public List<Action> actionsDone = new List<Action>();
    
    public static GameMaster instance;

    private void Awake()
    {
        instance = this;
        campaign = GameObject.Find("Campaign").GetComponent<Campaign>();
    }

    private void Start()
    {
        for (var i = 0; i < campaign.mapsParent.childCount; i++)
            campaign.mapsParent.GetChild(i).gameObject.SetActive(false);
    }

    private void Update()
    {
        foreach (var action in actionsToDo)
        {
            try
            {
                ResolveAction(action);
            }
            finally
            {
                Debug.LogWarning($"Action (name={action.name}, map={action.map}) for user doesn't exist");
                action.done = true;
            }
        }

        var actionsToDelete = actionsToDo.Where(a => a.done).ToList();
        if (actionsToDelete.Any())
            actionsToDo.Remove(actionsToDelete[0]);
    }

    public static class ActionNames
    {
        // entity actions
        public const string CreateEntity = "CreateEntity";
        public const string ChangeEntity = "ChangeEntity";
        public const string DeleteEntity = "DeleteEntity";
    }
    
    public void RegisterAction(Action action){
        actionsDone.Add(action);
    }

    public void ResolveAction(Action action)
    {
        switch (action.name)
        {
            // entity actions
            case ActionNames.CreateEntity: CreateEntity(action.entities[0]); break;
            case ActionNames.ChangeEntity: ChangeEntity(action.entities[0]); break;
            case ActionNames.DeleteEntity: DeleteEntity(action.entities[0]); break;
        }
    }

    public void CreateEntity(Entity.SerializableEntity serializableEntity)
    {
        var map = campaign.ActiveMap;
        if (map.entities.Any(e => e.name == serializableEntity.name))
            throw new Exception($"Name already exist in map: {serializableEntity.name}");
        var entity = Instantiate(entityPrefab, map.entitiesParent).GetComponent<Entity>();
        entity.map = map;
        entity.tile = map.Tile(serializableEntity.position);
        entity.Deserialize(serializableEntity);
        entity.map.entities.Add(entity);
    }

    public void ChangeEntity(Entity.SerializableEntity serializableEntity)
    {
        var map = campaign.ActiveMap;
        var entity = map.entities.Find(e => e.Name == serializableEntity.name);  
        entity.map = map;
        entity.tile = map.Tile(serializableEntity.position);
        entity.Deserialize(serializableEntity);
    }

    public void DeleteEntity(Entity.SerializableEntity serializableEntity)
    {
        var entity = campaign.ActiveMap.entities.Find(e => e.Name == serializableEntity.name);
        entity.map.entities.Remove(entity);
        Destroy(entity);
    }
}