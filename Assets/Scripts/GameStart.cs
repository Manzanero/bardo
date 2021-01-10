#pragma warning disable 0649

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameStart : MonoBehaviour
{
    public GameObject errorDialogue;
    public Text errorMessage;
    
    public InputField campaignInput;
    public InputField playerInput;
    public InputField passwordInput;

    private void Start()
    {
        Server.SetBaseUrl();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            Launch();
    }

    public void Launch() => StartCoroutine(LoadWorld());

    [Serializable] private class WorldResponse : Server.Response
    {
        public WorldData world;
    
        [Serializable] public class WorldData
        {
            public List<CampaignData> campaigns;
                
            [Serializable] public class CampaignData
            {
                public string name;
                public string id;
            }
        }
    }
    
    public IEnumerator LoadWorld()
    {
        Server.SetCredentials(playerInput.text, passwordInput.text);
        
        
        // check valid credentials
        var request = Server.GetRequest($"{Server.baseUrl}/world/");
        while (!request.isDone) 
            yield return null;
    
        var worldResponse = Server.GetResponse<WorldResponse>(request, false);
        if (worldResponse.status == 401)
        {
            RaiseError("Bad credentials");
            yield break;
        }

        // check valid campaign
        var world = worldResponse.world;
        var campaignName = campaignInput.text;
        var campaignData = world.campaigns.FirstOrDefault(x => string.Equals(x.name, campaignName, StringComparison.CurrentCultureIgnoreCase));
        if (campaignData == null)
        {
            RaiseError($"Campaign (name={campaignName}) doesn't exist");
            yield break;
        }
        
        Campaign.campaignName = campaignName;
        Campaign.campaignId = campaignData.id;
        
        // check valid player
        var url = $"{Server.baseUrl}/world/campaign/{campaignData.id}";
        var campaignRequest = Server.GetRequest(url);
        while (!campaignRequest.isDone) 
            yield return null;
        
        var campaignResponse = Server.GetResponse<Campaign.CampaignResponse>(campaignRequest);
        var campaign = campaignResponse.campaign;
        var properties = campaign.properties;
        var isPlayer = properties.FirstOrDefault(p => p.name == "IS_PLAYER");
        if (isPlayer == null)
        {
            RaiseError($"Player (name={campaignName}) does not belong to this campaign");
            yield break;
        }

        var isMaster = properties.FirstOrDefault(p => p.name == "IS_MASTER");
        Campaign.playerIsMaster = isMaster != null && isMaster.value == "true";
        
        var playersInfo = campaign.players;
        Campaign.playerName = playersInfo.FirstOrDefault(p => string.Equals(
            p.name, playerInput.text, StringComparison.CurrentCultureIgnoreCase))?.name;

        Server.serverReady = true;     
        
        SceneManager.LoadScene(1); 
    }

    private void RaiseError(string msg)
    {
        errorDialogue.SetActive(true);
        errorMessage.text = msg;
    }
    
}
