using System;
using System.Collections.Generic;

[Serializable]
public class Action
{
    public string name;
    public string map;
    public List<string> strings;
    public List<Tile.SerializableTile> tiles;
    public List<Entity.SerializableEntity> entities;
    public bool done;
}
