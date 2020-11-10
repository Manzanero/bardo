
using System;
using System.Collections.Generic;

[Serializable]
public class Action
{
    public string name;
    public string player;
    public List<string> strings;
    public List<Map.SerializableMap> maps;
    public List<Tile.SerializableTile> tiles;
    public List<Entity.SerializableEntity> entities;
}
