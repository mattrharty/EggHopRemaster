using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using UnityEngine.SceneManagement;

public class gameManager : MonoBehaviour
{
    public TextAsset defaultLvl;
    public levelData currentLvl;
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

        Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;

        //SceneManager.sceneLoaded -= OnSceneLoaded;
        //SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Update(){
        axis.x = Input.GetAxis("Horizontal");
        axis.y = Input.GetAxis("Vertical");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        foreach(SpriteRenderer sr in GameObject.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None)){
            Debug.Log(sr.gameObject);
            sr.gameObject.AddComponent(typeof(renderCheck));
        }
    }
}
