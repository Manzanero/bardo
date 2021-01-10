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
        public Toggle showNameToggle;
        public Toggle hideNameToggle;
        public InputField healthInput;
        public InputField maxHealthInput;
        public Toggle showHealthToggle;
        public Toggle hideHealthToggle;
        public InputField staminaInput;
        public InputField maxStaminaInput;
        public Toggle showStaminaToggle;
        public Toggle hideStaminaToggle;
        public InputField manaInput;
        public InputField maxManaInput;
        public Toggle showManaToggle;
        public Toggle hideManaToggle;
        public InputField greenInput;
        public InputField redInput;
        public InputField blueInput;
        public InputField sizeInput;
        public InputField lightInputField;
        public Toggle showLightToggle;
        public Toggle hideLightToggle;

        public Button saveButton;
        public Button clearButton;
        public Button deleteButton;
    
        private GameMaster _gm;
        private Tile _cacheSelectTile;
        private readonly List<string> _cacheSelectIds = new List<string>();

        private void Start()
        {
            _gm = GameMaster.instance;
            newTokenButton.onClick.AddListener(NewTokenButton);
            saveButton.onClick.AddListener(SaveButton);
            clearButton.onClick.AddListener(ClearButton);
            deleteButton.onClick.AddListener(DeleteButton);

            // delete placeholder
            foreach (Transform child in selectedItemsParent)
                Destroy(child.gameObject);
        }

        private void Update()
        {
            var activeMap = _gm.campaign.activeMap;
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
                _cacheSelectIds.Clear();
            }
        }

        private void DeselectItem(Entity entity, SelectedItem item)
        {
            _cacheSelectIds.Remove(item.name);
            Destroy(item.gameObject);
            _gm.campaign.activeMap.selectedEntities.Remove(entity);
        }

        private void NewTokenButton()
        {
            var tile = _cacheSelectTile;
            if (!tile)
                return;
            
            var map = _gm.campaign.activeMap;
            var entity = Instantiate(_gm.entityPrefab, map.entitiesParent).GetComponent<Entity>();
            var pos = tile.Position;
            entity.transform.position = new Vector3(pos.x, tile.Altitude, pos.y);
            map.entities.Add(entity);
            map.selectedEntities.Clear();
            map.selectedEntities.Add(entity);
            entity.map = map;
            entity.tile = tile;
            entity.RefreshPermissions();
        
            entity.id = GameMaster.NewId();
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
                map = map.id,
                entities = map.selectedEntities.Select(x => x.Serialize()).ToList()
            });
        }

        private void ClearButton()
        {
            nameInput.text = "";
            showNameToggle.isOn = false;
            hideNameToggle.isOn = false;
            healthInput.text = "";
            maxHealthInput.text = "";
            showHealthToggle.isOn = false;
            hideHealthToggle.isOn = false;
            staminaInput.text = "";
            maxStaminaInput.text = "";
            showStaminaToggle.isOn = false;
            hideStaminaToggle.isOn = false;
            manaInput.text = "";
            maxManaInput.text = "";
            showManaToggle.isOn = false;
            hideManaToggle.isOn = false;
            redInput.text = "";
            greenInput.text = ""; 
            blueInput.text = ""; 
            sizeInput.text = "";
            lightInputField.text = "";
            showLightToggle.isOn = false;
            hideLightToggle.isOn = false;
        }

        private void SaveButton()
        {
            var map = _gm.campaign.activeMap;
            if (map.selectedEntities.Count == 0)
                return;
                
            foreach (var entity in map.selectedEntities)
            {
                if (nameInput.text != "") entity.Name = nameInput.text;
                if (showNameToggle.isOn) entity.HasName = true;
                if (hideNameToggle.isOn) entity.HasName = false;
                if (healthInput.text != "") entity.Health = float.Parse(healthInput.text, CultureInfo.InvariantCulture);
                if (maxHealthInput.text != "") entity.MaxHealth = float.Parse(maxHealthInput.text, CultureInfo.InvariantCulture);
                if (showHealthToggle.isOn) entity.HasHealth = true;
                if (hideHealthToggle.isOn) entity.HasHealth = false;
                if (staminaInput.text != "") entity.Stamina = float.Parse(staminaInput.text, CultureInfo.InvariantCulture);
                if (maxStaminaInput.text != "") entity.MaxStamina = float.Parse(maxStaminaInput.text, CultureInfo.InvariantCulture);
                if (showStaminaToggle.isOn) entity.HasStamina = true;
                if (hideStaminaToggle.isOn) entity.HasStamina = false;
                if (manaInput.text != "") entity.Mana = float.Parse(manaInput.text, CultureInfo.InvariantCulture);
                if (maxManaInput.text != "") entity.MaxMana = float.Parse(maxManaInput.text, CultureInfo.InvariantCulture);
                if (showManaToggle.isOn) entity.HasMana = true;
                if (hideManaToggle.isOn) entity.HasMana = false;
                
                var c = entity.BaseColor;
                if (redInput.text != "") entity.BaseColor = new Color(
                    Mathf.Clamp(float.Parse(redInput.text, CultureInfo.InvariantCulture), 0f, 1f), c.g, c.b); 
                c = entity.BaseColor;
                if (greenInput.text != "") entity.BaseColor = new Color(
                    c.r, Mathf.Clamp(float.Parse(greenInput.text, CultureInfo.InvariantCulture), 0, 1), c.b);
                c = entity.BaseColor; 
                if (blueInput.text != "") entity.BaseColor = new Color(
                    c.r, c.g, Mathf.Clamp(float.Parse(blueInput.text, CultureInfo.InvariantCulture), 0, 1)); 
                if (sizeInput.text != "") 
                    entity.BaseSize = Mathf.Clamp(float.Parse(sizeInput.text, CultureInfo.InvariantCulture), 0, 5);
                if (lightInputField.text != "") 
                    entity.LightRange = Mathf.Clamp(float.Parse(lightInputField.text, CultureInfo.InvariantCulture), 0, 68);
                if (showLightToggle.isOn) entity.HasLight = true;
                if (hideLightToggle.isOn) entity.HasLight = false;
            }
            
            _gm.RegisterAction(new Action {
                name = GameMaster.ActionNames.ChangeEntity,
                map = map.id,
                entities = map.selectedEntities.Select(x => x.Serialize()).ToList()
            });
        }

        private void DeleteButton()
        {
            var map = _gm.campaign.activeMap;
            foreach (var entity in map.selectedEntities)
            {
                map.entities.Remove(entity);
                Destroy(entity.gameObject);
            }
            
            _gm.RegisterAction(new Action {
                name = GameMaster.ActionNames.DeleteEntity,
                map = map.id,
                entities = map.selectedEntities.Select(x => x.Serialize()).ToList()
            });
            
            map.selectedEntities.Clear();
        }
    }
}