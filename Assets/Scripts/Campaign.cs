using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Campaign : MonoBehaviour
{
    public Transform mapsParent;
    public List<Map> maps;
    
    public string activeCampaign = "";
    public Map activeMap;
    
    private void Start()
    {
        activeCampaign = "test";
    }

    public Map GetMapByName(string mapName)
    {
        return maps.FirstOrDefault(map => map.name == mapName);
    }

    public void ChangeActiveMap(string mapName)
    {
        if (mapName == activeMap.name)
            return;
        
        activeMap.gameObject.SetActive(false);
        activeMap = GetMapByName(mapName);
        activeMap.gameObject.SetActive(true);
    }
}