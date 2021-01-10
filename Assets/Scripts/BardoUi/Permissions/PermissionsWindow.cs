
#pragma warning disable 0649

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BardoUi.Tokens;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace BardoUi.Permissions
{
    public class PermissionsWindow : MonoBehaviour
    {
        public Transform selectedItemsParent;
        public GameObject selectedItemPrefab;
        public Button resetButton;

        public Transform playerItemsParent;
        public GameObject playerItemPrefab;
        public Toggle defaultToggle;
        public Button setDefaultButton;

        public Toggle nameToggleShow;
        public Toggle positionToggleShow;
        public Toggle visionToggleShow;
        public Toggle controlToggleShow;
        public Toggle healthToggleShow;
        public Toggle staminaToggleShow;
        public Toggle manaToggleShow;
        public Button clearButton;
        public Button applyButton;

        private GameMaster _gm;
        private Campaign _campaign;
        private List<Toggle> _playerToggles = new List<Toggle>();
        private List<string> _cacheSelectIds = new List<string>();
        private bool _playersLoaded;

        private void Start()
        {
            _gm = GameMaster.instance;
            _campaign = _gm.campaign;
            resetButton.onClick.AddListener(ResetButton);
            setDefaultButton.onClick.AddListener(SetDefaultButton);
            clearButton.onClick.AddListener(ClearButton);
            applyButton.onClick.AddListener(ApplyButton);

            // delete placeholder
            foreach (Transform child in selectedItemsParent) Destroy(child.gameObject);
            foreach (Transform child in playerItemsParent) Destroy(child.gameObject);

            StartCoroutine(LoadPlayers());
        }

        private IEnumerator LoadPlayers()
        {
            while (!_gm.campaign.loaded) yield return null;

            foreach (var playerInfo in _gm.campaign.playersInfo.Where(x => !x.master))
            {
                var playerItem = Instantiate(playerItemPrefab, playerItemsParent).GetComponent<PlayerItem>();
                playerItem.playerName.text = playerInfo.name;
                _playerToggles.Add(playerItem.selectedToggle);
            }

            _playersLoaded = true;
        }

        private void Update()
        {
            if (!_playersLoaded) return;

            var activeMap = _gm.campaign.activeMap;
            if (!activeMap || !activeMap.loaded) return;

            if (activeMap.selectedEntities.Any())
            {
                foreach (var selected in activeMap.selectedEntities)
                {
                    if (_cacheSelectIds.Contains(selected.id)) continue;

                    var selectedItem = Instantiate(selectedItemPrefab, selectedItemsParent)
                        .GetComponent<SelectedItem>();
                    selectedItem.entityName.text = selected.name;
                    selectedItem.name = selected.id;
                    selectedItem.deselectButton.onClick.AddListener(delegate { DeselectItem(selected, selectedItem); });
                    _cacheSelectIds.Add(selected.id);
                }

                var idsToRemove = _cacheSelectIds
                    .Where(id => !activeMap.selectedEntities.Select(x => x.id).Contains(id))
                    .ToList();
                foreach (Transform child in selectedItemsParent)
                    if (idsToRemove.Contains(child.name))
                        Destroy(child.gameObject);
                _cacheSelectIds.RemoveAll(x => idsToRemove.Contains(x));
            }
            else
            {
                foreach (Transform child in selectedItemsParent) Destroy(child.gameObject);
                _cacheSelectIds.Clear();
            }
        }

        private void DeselectItem(Entity entity, SelectedItem item)
        {
            _cacheSelectIds.Remove(item.name);
            Destroy(item.gameObject);
            _gm.campaign.activeMap.selectedEntities.Remove(entity);
        }

        private void ResetButton()
        {
            StartCoroutine(ResetPermissions());
        }

        [Serializable]
        private class ResetRequest
        {
            public List<string> entities;
        }

        private IEnumerator ResetPermissions()
        {
            var entitiesId = _campaign.activeMap.selectedEntities.Select(x => x.id).ToList();
            if (!entitiesId.Any()) yield break;

            var data = new ResetRequest {entities = entitiesId};
            var url = $"{Server.baseUrl}/world/campaign/{Campaign.campaignId}" +
                      $"/map/{_campaign.activeMap.id}/permissions/reset";
            var request = Server.PostRequest(url, data);
            while (!request.isDone) yield return null;

            Server.GetResponse<Server.Response>(request);

            _gm.RegisterAction(new Action {name = GameMaster.ActionNames.RefreshPermissions});
        }

        private void SetDefaultButton()
        {
            StartCoroutine(DefaultPermissions());
        }

        [Serializable]
        private class DefaultRequest
        {
            public List<string> entities;
            public List<string> players;
        }

        private List<string> SelectedPlayersId()
        {
            var playersInfo = _gm.campaign.playersInfo.Where(x => !x.master).ToList();
            var playersId = new List<string>();
            for (var i = 0; i < _playerToggles.Count; i++) if (_playerToggles[i].isOn) playersId.Add(playersInfo[i].id);
            return playersId;
        }

        private IEnumerator DefaultPermissions()
        {
            var entitiesId = _campaign.activeMap.selectedEntities.Select(x => x.id).ToList();
            var playersId = SelectedPlayersId();

            if (!playersId.Any() && !entitiesId.Any() && !defaultToggle.isOn) yield break;

            var data = new DefaultRequest {entities = entitiesId, players = playersId};
            var url = $"{Server.baseUrl}/world/campaign/{Campaign.campaignId}" +
                      $"/map/{_campaign.activeMap.id}/permissions/default";
            var request = Server.PostRequest(url, data);
            while (!request.isDone) yield return null;

            Server.GetResponse<Server.Response>(request);

            _gm.RegisterAction(new Action {name = GameMaster.ActionNames.RefreshPermissions});
        }

        private void ClearButton()
        {
            defaultToggle.isOn = false;
            nameToggleShow.isOn = false;
            positionToggleShow.isOn = false;
            visionToggleShow.isOn = false;
            controlToggleShow.isOn = false;
            healthToggleShow.isOn = false;
            staminaToggleShow.isOn = false;
            manaToggleShow.isOn = false;
            foreach (var toggle in _playerToggles) toggle.isOn = false;
        }

        private void ApplyButton()
        {
            StartCoroutine(StorePermissions());
        }

        [Serializable]
        private class Permission
        {
            public string entity;
            public string player;
            public string permission;
        }

        [Serializable]
        private class PermissionsRequest
        {
            public List<Permission> permissions;
        }

        private IEnumerator StorePermissions()
        {
            var entitiesId = _campaign.activeMap.selectedEntities.Select(x => x.id).ToList();
            var playersId = SelectedPlayersId();

            if (!playersId.Any() && !entitiesId.Any() && !defaultToggle.isOn) yield break;

            var data = new DefaultRequest {entities = entitiesId, players = playersId};
            var url = $"{Server.baseUrl}/world/campaign/{Campaign.campaignId}" +
                      $"/map/{_campaign.activeMap.id}/permissions/default";
            var request = Server.PostRequest(url, data);
            while (!request.isDone) yield return null;

            Server.GetResponse<Server.Response>(request);

            var permissions = new List<Permission>();
            foreach (var entity in entitiesId)
            {
                if (defaultToggle.isOn)
                {
                    var data1 = new DefaultRequest {entities = entitiesId};
                    var url1 = $"{Server.baseUrl}/world/campaign/{Campaign.campaignId}" +
                               $"/map/{_campaign.activeMap.id}/permissions/default";
                    var request1 = Server.PostRequest(url1, data1);
                    while (!request1.isDone) yield return null;

                    Server.GetResponse<Server.Response>(request1);

                    if (nameToggleShow.isOn) 
                        permissions.Add(new Permission {entity = entity, permission = "name"});
                    if (positionToggleShow.isOn)
                        permissions.Add(new Permission {entity = entity, permission = "position"});
                    if (visionToggleShow.isOn) 
                        permissions.Add(new Permission {entity = entity, permission = "vision"});
                    if (controlToggleShow.isOn)
                        permissions.Add(new Permission {entity = entity, permission = "control"});
                    if (healthToggleShow.isOn) 
                        permissions.Add(new Permission {entity = entity, permission = "health"});
                    if (staminaToggleShow.isOn)
                        permissions.Add(new Permission {entity = entity, permission = "stamina"});
                    if (manaToggleShow.isOn) 
                        permissions.Add(new Permission {entity = entity, permission = "mana"});
                }

                foreach (var player in playersId)
                {
                    if (nameToggleShow.isOn)
                        permissions.Add(new Permission {entity = entity, player = player, permission = "name"});
                    if (positionToggleShow.isOn)
                        permissions.Add(new Permission {entity = entity, player = player, permission = "position"});
                    if (visionToggleShow.isOn)
                        permissions.Add(new Permission {entity = entity, player = player, permission = "vision"});
                    if (controlToggleShow.isOn)
                        permissions.Add(new Permission {entity = entity, player = player, permission = "control"});
                    if (healthToggleShow.isOn)
                        permissions.Add(new Permission {entity = entity, player = player, permission = "health"});
                    if (staminaToggleShow.isOn)
                        permissions.Add(new Permission {entity = entity, player = player, permission = "stamina"});
                    if (manaToggleShow.isOn)
                        permissions.Add(new Permission {entity = entity, player = player, permission = "mana"});
                }
            }

            var data2 = new PermissionsRequest {permissions = permissions};
            var url2 =
                $"{Server.baseUrl}/world/campaign/{Campaign.campaignId}/map/{_campaign.activeMap.id}/permissions";
            var request2 = Server.PostRequest(url2, data2);
            while (!request2.isDone) yield return null;

            Server.GetResponse<Server.Response>(request2);

            _gm.RegisterAction(new Action {name = GameMaster.ActionNames.RefreshPermissions});
        }
    }
}