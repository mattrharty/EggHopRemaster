using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

public class gameManager : MonoBehaviour
{
    public TextAsset defaultLvl;
    public Vector2 axis;

    void Awake(){
        this.tag = "gameManager";

        GameObject[] objs = GameObject.FindGameObjectsWithTag("gameManager");

        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);

        levelTemp.currentLvl = JsonConvert.DeserializeObject<levelData>(defaultLvl.text);
        levelTemp.levelPlaying = JsonConvert.DeserializeObject<levelData>(defaultLvl.text);
    }

    void Update(){
        axis.x = Input.GetAxis("Horizontal");
        axis.y = Input.GetAxis("Vertical");
    }
}
