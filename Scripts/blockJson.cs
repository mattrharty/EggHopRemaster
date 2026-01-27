using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

[ExecuteInEditMode]
public class blockJson : MonoBehaviour
{

    [SerializeField]
    string fileName;
    [SerializeField]
    List<blockEditor> blocks;
    blockPalette palette = new blockPalette();
    byteEditor byteObj;
    public Sprite img;

    public void generate()
    {
        blockPalette palette = new blockPalette();
        palette.blocks = new List<byteEditor>();
        Debug.Log(img.texture.isReadable);
        foreach (blockEditor obj in blocks)
        {
            List<byte[]> priTex = new List<byte[]>();
            List<byte[]> secTex = new List<byte[]>();
            foreach (Sprite spr in obj.primaryTex)
            {
                Texture2D newTex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                newTex = spr.texture;
                Debug.Log(spr.texture.isReadable);
                priTex.Add(ImageConversion.EncodeToPNG(spr.texture));
            }
            foreach (Sprite spr in obj.secondaryTex)
            {
                Texture2D newTex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                newTex = spr.texture;
                secTex.Add(ImageConversion.EncodeToPNG(spr.texture));
            }
            palette.blocks.Add(new byteEditor(obj.name, obj.width, obj.height, obj.canFill, obj.isTwoState, obj.canReplace, priTex, secTex));
        }
        File.WriteAllTextAsync("C:/Users/matty/Documents/Unity Projects/Egg Hop V2/Assets/Resources/Block Palettes/" + fileName + ".json", JsonConvert.SerializeObject(palette, Formatting.Indented, new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        }));
        //Debug.Log(JsonConvert.SerializeObject(blocks, Formatting.Indented));
    }
}

[Serializable]
public class blockPalette
{
    public List<byteEditor> blocks;

    public blockPalette(){
        this.blocks = new List<byteEditor>();
    }
}

[Serializable]
public class byteEditor
{
    public string name;
    public int width;
    public int height;
    public bool canFill;
    public bool isTwoState;
    public List<string> canReplace;
    public List<byte[]> primaryTex;
    public List<byte[]> secondaryTex;

    public byteEditor(string name, int width, int height, bool canFill, bool isTwoState, List<string> canReplace, List<byte[]> primaryTex, List<byte[]> secondaryTex)
    {
        this.name = name;
        this.width = width;
        this.height = height;
        this.canFill = canFill;
        this.isTwoState = isTwoState;
        this.canReplace = canReplace;
        this.primaryTex = primaryTex;
        this.secondaryTex = secondaryTex;
    }
}
