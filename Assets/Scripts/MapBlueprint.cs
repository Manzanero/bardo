using System;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Map", menuName = "Map Blueprint")]
public class MapBlueprint : ScriptableObject
{
    public string description;

    public MapTheme theme;
}
