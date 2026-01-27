using UnityEngine;
using System.Collections.Generic;

public class placedBlock : MonoBehaviour
{

    public blockType type;
    public  List<Sprite> w1ColTop;
    public  List<Sprite> w1ColMid;
    public  List<Sprite> w1ColBot;
    public  List<Sprite> w1ColSingle;
    public Sprite[] blocks;
    public Sprite[] darkBlocks;
    public Sprite[] darkerBlocks;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        refresh();
    }

    // Update is called once per frame
    void refresh()
    {
        if (type == blockType.block)
        {
            
        }
    }
}
