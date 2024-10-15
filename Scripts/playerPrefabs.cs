using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class playerPrefabs
{
    public static GameObject block = Resources.Load<GameObject>("block");
    public static GameObject platform = Resources.Load<GameObject>("platform");
    public static GameObject spike = Resources.Load<GameObject>("spike");
    public static GameObject spawn = Resources.Load<GameObject>("spawn");
    public static GameObject button = Resources.Load<GameObject>("button");
    public static GameObject door = Resources.Load<GameObject>("door");
    public static GameObject tempBlock = Resources.Load<GameObject>("tempBlock");
    public static GameObject twoStateRed = Resources.Load<GameObject>("twoStateBlockRed");
    public static GameObject twoStateBlue = Resources.Load<GameObject>("twoStateBlockBlue");
    public static GameObject twoStateButton = Resources.Load<GameObject>("twoStateButton");
    public static GameObject twoStateLever = Resources.Load<GameObject>("twoStateLever");

    /*void Awake(){
        block = Resources.Load<GameObject>("block");
        platform = Resources.Load<GameObject>("platform");
        spike = Resources.Load<GameObject>("spike");
        spawn = Resources.Load<GameObject>("spawn");
        button = Resources.Load<GameObject>("button");
        door = Resources.Load<GameObject>("door");
    }*/
}
