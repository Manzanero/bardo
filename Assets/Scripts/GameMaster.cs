using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class GameMaster : MonoBehaviour
{
    public Transform mapsParent;
    public GameObject mapPrefab;
    public GameObject tilePrefab;
    public GameObject entityPrefab;

    public string player = "default";
    public bool master;
    public string activeCampaign = "default";
    public Map activeMap;
    public List<Action> actionsToDo = new List<Action>();
    public List<Action> actionsDone = new List<Action>();
    
    public static GameMaster instance;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (actionsToDo.Count == 0)
            return;
        var action = actionsToDo[0];
        try { ResolveAction(action); } finally { actionsToDo.Remove(action); }
    }

    public static class Actions
    {
        // map actions
        public const string LoadMap = "LoadMap";
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
            // map actions
            case Actions.LoadMap: LoadMap(action.maps[0]); break;
            // entity actions
            case Actions.CreateEntity: CreateEntity(action.entities[0]); break;
            case Actions.ChangeEntity: ChangeEntity(action.entities[0]); break;
            case Actions.DeleteEntity: DeleteEntity(action.entities[0]); break;
        }
    }

    public void LoadMap(Map.SerializableMap serializableMap)
    {
        var mapName = serializableMap.name;
        for (var i = 0; i < mapsParent.childCount; i++)
            mapsParent.GetChild(i).gameObject.SetActive(false);
        var mapTransform = mapsParent.Find(mapName);
        Map map;
        if (mapTransform != null)
        {
            mapTransform.gameObject.SetActive(true);
            map = mapTransform.GetComponent<Map>();
        }
        else
        {
            var mapGo = Instantiate(mapPrefab, Vector3.zero, Quaternion.identity, mapsParent);
            mapGo.name = serializableMap.name;
            map = mapGo.GetComponent<Map>();
            map.Deserialize(serializableMap);
        }
        activeMap = map;
    }

    public void CreateEntity(Entity.SerializableEntity serializableEntity)
    {
        if (activeMap.entities.Any(e => e.name == serializableEntity.name))
            throw new Exception($"Name already exist in map: {serializableEntity.name}");
        var entityGo = Instantiate(entityPrefab, (Vector2) serializableEntity.position, 
            Quaternion.identity, activeMap.entitiesParent);
        var entity = entityGo.GetComponent<Entity>();
        entity.map = activeMap;
        entity.Deserialize(serializableEntity);
        entity.map.entities.Add(entity);
    }

    public void ChangeEntity(Entity.SerializableEntity serializableEntity)
    {
        var entity = activeMap.entities.Find(e => e.Name == serializableEntity.name);
        entity.Deserialize(serializableEntity);
    }

    public void DeleteEntity(Entity.SerializableEntity serializableEntity)
    {
        var entity = activeMap.entities.Find(e => e.Name == serializableEntity.name);
        entity.map.entities.Remove(entity);
        Destroy(entity);
    }
}