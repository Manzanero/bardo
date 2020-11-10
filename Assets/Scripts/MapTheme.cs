using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Theme", menuName = "Map Theme")]
public class MapTheme : ScriptableObject
{
    public string description;
    
    public TileBlueprint stairsDown2;
    public TileBlueprint stairsDown1;
    public TileBlueprint floor;
    public TileBlueprint stairsUp1;
    public TileBlueprint stairsUp2;
    public TileBlueprint wall;
    public TileBlueprint door;
    public TileBlueprint secretDoor;
    public TileBlueprint portcullis;
}
