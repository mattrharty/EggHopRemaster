using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class block
{
    public blockType type;
    public coordinate2D placePos;
    public double rot;
    public int blockVer;
    public int activeState;
    public bool coreTile;

    public block (blockType type, coordinate2D placePos, double rot, int blockVer, bool coreTile){
        this.type = type;
        this.placePos = placePos;
        this.rot = rot;
        this.blockVer = blockVer;
        this.coreTile = coreTile;
    } 

}

[Serializable]
public class levelData{
    public Dictionary<coordinate2D, block> blocks;
    public int[] size;



    public block getTile(blockType type) {
        foreach(KeyValuePair<coordinate2D, block> block in blocks){
            if(block.Value.type == type){
                return block.Value;
            }
        }
        return null;
    }

    public block getTile(int x, int y) {
        try {
            return blocks[new coordinate2D (x, y)];
        } catch {
            return null;
        }
    }

    /*public int getTileIndex(blockType type) {
        //List<int> coord = new List<int> {x, y};
        for (int i = 0; i < blocks.Count; i++){
            if(blocks[i].type == type){
                return i;
            }
        }
        foreach(KeyValuePair<coordinate2D, block> block in blocks){
            if(block.Value.type == type){
                return i;
            }
        }
        return -1;
    }*/

    public block getTile(blockType type, bool core) {
        foreach(KeyValuePair<coordinate2D, block> block in blocks){
            if(block.Value.type == type && block.Value.coreTile == core){
                return block.Value;
            }
        }
        return null;
    }

    public List<coordinate2D> occupiedTiles;

    public bool checkTile(int x, int y) {
        if(occupiedTiles.Count == 0){
            return false;
        }
        coordinate2D newCoord = new coordinate2D(x, y);
        foreach (coordinate2D coord in occupiedTiles){
            if(newCoord.x == coord.x && newCoord.y == coord.y){
                return true;
            }
        }
        return false;
    }

}

[Serializable] [TypeConverter(typeof(coordConverter))]
public class coordinate2D : IEquatable<coordinate2D> {
    public int x;
    public int y;

    public List<int> toList(){
        List<int> coords = new List<int> {x, y};
        return coords;
    }

    public coordinate2D (int xInput, int yInput){
        x = xInput;
        y = yInput;
    }

    public coordinate2D (List<int> ints){
        x = ints[0];
        y = ints[1];
    }

    public override bool Equals(object obj) => this.Equals(obj as coordinate2D);

    public bool Equals(coordinate2D other)
   {
        if (this.x != other.x) return false;
        if (this.y != other.y) return false;

        // TODO: Compare Members and return false if not the same

        return true;
   }

    public override int GetHashCode() => (x, y).GetHashCode();

    public static bool operator ==(coordinate2D lhs, coordinate2D rhs)
    {
        if (lhs is null)
        {
            if (rhs is null)
            {
                return true;
            }

            // Only the left side is null.
            return false;
        }
        // Equals handles case of null on right side.
        return lhs.Equals(rhs);
    }

    public static bool operator !=(coordinate2D lhs, coordinate2D rhs) => !(lhs == rhs);

    public override string ToString()
    {
        return $"({this.x}, {this.y})";
    }
}

public class coordConverter : TypeConverter {
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
        return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        var casted = value as string;
        return casted != null
            ? new coordinate2D(int.Parse(casted.Split(',')[0].Trim('(')), int.Parse(casted.Split(',')[1].Trim(')')))
            : base.ConvertFrom(context, culture, value);
    }

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        var casted = value as coordinate2D;
        return destinationType == typeof(string)
            ? $"({casted.x}, {casted.y})"
            : base.ConvertTo(context, culture, value, destinationType);
    }
}

[Serializable]
public enum blockType {
    block,
    spawn,
    button,
    door,
    obstacle,
    platform,
    // ^^Priority^^
    slime,
    blueBlock,
    redBlock,
    column,

}
