using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BardoUi.Tokens
{
    public class TokensWindow : MonoBehaviour
    {
        public Transform selectedItemsParent;
        public GameObject selectedItemPrefab;
        
        public Text currentTileText;
        public Button newTokenButton;
        
        public InputField nameInput;
        public Toggle hasNameToggle;
        public InputField healthInput;
        public InputField maxHealthInput;
        public Toggle hasHealthToggle;
        public InputField staminaInput;
        public InputField maxStaminaInput;
        public Toggle hasStaminaToggle;
        public InputField manaInput;
        public InputField maxManaInput;
        public Toggle hasManaToggle;
        public InputField greenInput;
        public InputField redInput;
        public InputField blueInput;
        public InputField lightInputField;
        public Toggle hasLightToggle;

        public Button saveButton;
        public Button deleteButton;
    
        private GameMaster _gm;
        private Tile _cacheSelectTile;
        private List<string> _cacheSelectIds = new List<string>();

        private void Start()
        {
            _gm = GameMaster.instance;
            newTokenButton.onClick.AddListener(NewTokenButton);
            saveButton.onClick.AddListener(SaveButton);
            deleteButton.onClick.AddListener(DeleteButton);

            // delete placeholder
            foreach (Transform child in selectedItemsParent)
                Destroy(child.gameObject);
        }

        private void Update()
        {
            var activeMap = _gm.campaign.ActiveMap;
            if (!activeMap)
                return;
            
            if (activeMap.selectedTiles.Any())
            {
                _cacheSelectTile = activeMap.selectedTiles[0];
                currentTileText.text = $"x: {_cacheSelectTile.Position.x}, y: {_cacheSelectTile.Position.y}";
            }
            else { currentTileText.text = "-"; }

            if (activeMap.selectedEntities.Any())
            {
                foreach (var selected in activeMap.selectedEntities)
                {
                    if (_cacheSelectIds.Contains(selected.id))
                        continue;
                
                    var selectedItem = Instantiate(selectedItemPrefab, selectedItemsParent).GetComponent<SelectedItem>();
                    selectedItem.entityName.text = selected.name;
                    selectedItem.name = selected.id;
                    selectedItem.deselectButton.onClick.AddListener(delegate { DeselectItem(selected, selectedItem); });
                    _cacheSelectIds.Add(selected.id);
                }

                var idsToRemove = _cacheSelectIds.Where(
                    id => !activeMap.selectedEntities.Select(x => x.id).Contains(id)).ToList();
                foreach (Transform child in selectedItemsParent)
                    if (idsToRemove.Contains(child.name)) 
                        Destroy(child.gameObject);
                _cacheSelectIds.RemoveAll(x => idsToRemove.Contains(x));
            }
            else
            {
                foreach (Transform child in selectedItemsParent)
                    Destroy(child.gameObject);
                _cacheSelectIds = new List<string>();
            }
        }

        private void DeselectItem(Entity entity, SelectedItem item)
        {
            _cacheSelectIds.Remove(item.name);
            Destroy(item.gameObject);
            _gm.campaign.ActiveMap.selectedEntities.Remove(entity);
        }

        private void NewTokenButton()
        {
            var tile = _cacheSelectTile;
            if (!tile)
                return;
            
            var map = _gm.campaign.ActiveMap;
            var entity = Instantiate(_gm.entityPrefab, map.entitiesParent).GetComponent<Entity>();
            var pos = tile.Position;
            entity.transform.position = new Vector3(pos.x, tile.Altitude, pos.y);
            map.entities.Add(entity);
            map.selectedEntities = new List<Entity>{entity};
            entity.map = map;
            entity.tile = tile;
        
            entity.id = Guid.NewGuid().ToString().Substring(0, 8);
            entity.HasName = true ;
            entity.Name = entity.id;
            entity.HasInitiative = false;
            entity.Initiative = 0;
            entity.HasHealth = true;
            entity.Health = 50;
            entity.MaxHealth = 100;
            entity.HasStamina = false;
            entity.Stamina = 50;
            entity.MaxStamina = 100;
            entity.HasMana = false ;
            entity.Mana = 50;
            entity.MaxMana = 100;
            entity.HasVision = true;
            entity.HasShadowVision = false;
            entity.ShadowVisionRange = 1;
            entity.HasDarkVision = false;
            entity.HasLight = true;
            entity.LightRange = 6;
            entity.Rotation = 0;
            entity.HasBase = true;
            entity.BaseSize = 0.8f;
            entity.BaseColor = Color.white;
            entity.HasBaseImage = false;
            entity.BaseImageResource = "";
            entity.HasBody = false;
            entity.BodySize = 1 ;
            entity.BodyMeshResource = "";
            entity.ScaleCorrection = 1;
            entity.BodyMaterialResource = "";
            
            _gm.RegisterAction(new Action {
                name = GameMaster.ActionNames.ChangeEntity,
                map = map.mapId,
                entities = map.selectedEntities.Select(x => x.Serialize()).ToList()
            });
        }

        private void SaveButton()
        {
            var map = _gm.campaign.ActiveMap;
            if (map.selectedEntities.Count == 0)
                return;
                
            foreach (var entity in map.selectedEntities)
            {
                if (nameInput.text != "") entity.Name = nameInput.text;
                entity.HasName = hasNameToggle.isOn;
                if (healthInput.text != "") entity.Health = float.Parse(healthInput.text, CultureInfo.InvariantCulture);
                if (maxHealthInput.text != "") entity.MaxHealth = float.Parse(maxHealthInput.text, CultureInfo.InvariantCulture);
                entity.HasHealth = hasHealthToggle.isOn;
                if (staminaInput.text != "") entity.Stamina = float.Parse(staminaInput.text, CultureInfo.InvariantCulture);
                if (maxStaminaInput.text != "") entity.MaxStamina = float.Parse(maxStaminaInput.text, CultureInfo.InvariantCulture);
                entity.HasStamina = hasStaminaToggle.isOn;
                if (manaInput.text != "") entity.Mana = float.Parse(manaInput.text, CultureInfo.InvariantCulture);
                if (maxManaInput.text != "") entity.MaxMana = float.Parse(maxManaInput.text, CultureInfo.InvariantCulture);
                entity.HasMana = hasManaToggle.isOn;
                
                var c = entity.BaseColor;
                if (redInput.text != "") entity.BaseColor = new Color(
                    Mathf.Clamp(float.Parse(redInput.text, CultureInfo.InvariantCulture), 0f, 1f), c.g, c.b); 
                c = entity.BaseColor;
                if (greenInput.text != "") entity.BaseColor = new Color(
                    c.r, Mathf.Clamp(float.Parse(greenInput.text, CultureInfo.InvariantCulture), 0, 1), c.b);
                c = entity.BaseColor; 
                if (blueInput.text != "") entity.BaseColor = new Color(
                    c.r, c.g, Mathf.Clamp(float.Parse(blueInput.text, CultureInfo.InvariantCulture), 0, 1)); 
                if (lightInputField.text != "") 
                    entity.LightRange = Mathf.Clamp(float.Parse(lightInputField.text, CultureInfo.InvariantCulture), 0, 68);
                entity.HasLight = hasLightToggle.isOn;
            }
            
            _gm.RegisterAction(new Action {
                name = GameMaster.ActionNames.ChangeEntity,
                map = map.mapId,
                entities = map.selectedEntities.Select(x => x.Serialize()).ToList()
            });
        }

        private void DeleteButton()
        {
            var map = _gm.campaign.ActiveMap;
            foreach (var entity in map.selectedEntities)
            {
                map.entities.Remove(entity);
                Destroy(entity.gameObject);
            }
            
            _gm.RegisterAction(new Action {
                name = GameMaster.ActionNames.DeleteEntity,
                map = map.mapId,
                entities = map.selectedEntities.Select(x => x.Serialize()).ToList()
            });
            
            map.selectedEntities = new List<Entity>();
        }
    }
}