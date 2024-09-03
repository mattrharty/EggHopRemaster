using System.Linq.Expressions;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using SFB;
using System;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using UnityEngine.UI;

public class levelPlayer : MonoBehaviour
{

    #if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void UploadFile(string gameObjectName, string methodName, string filter, bool multiple);
    #endif

    //public DefaultAsset lvlToLoad;
    levelData currentLvl;

    public InputAction esc;

    public Transform blockBorderDaddy;
    public GameObject tilePrefab;
    public List<Sprite> borderBlocks;
    public Material litMat;

    public List<Sprite> blocks;
    public List<Sprite> darkBlocks;
    public List<Sprite> darkerBlocks;
    public Sprite darkestBlock;

    GameObject[,] placedBlocks;

    public Transform lvlDaddy;

    public GameObject popBox;

    public GameObject pauseMenu;

    public Transform eggDaddy;

    w1OneWay oneWays;
    public  List<Sprite> w1ColTop;
    public  List<Sprite> w1ColMid;
    public  List<Sprite> w1ColBot;
    public  List<Sprite> w1ColSingle;
    Dictionary<string, List<Sprite>> w1Columns = new Dictionary<string, List<Sprite>>();

    public Transform goos;

    // Start is called before the first frame update
    void Start()
    {
        oneWays = this.GetComponent<w1OneWay>();

        w1Columns.Add("top", w1ColTop);
        w1Columns.Add("mid", w1ColMid); 
        w1Columns.Add("bot", w1ColBot); 
        w1Columns.Add("single", w1ColSingle);

        pauseMenu = GameObject.Find("Pause");
        popBox = GameObject.Find("Alert");

        popBox.SetActive(false);
        pause(false);
        placedBlocks = new GameObject [256,256];
        Load("", true);

        esc.Enable();
        esc.performed += context => pause(!pauseMenu.activeSelf);

        pauseMenu.transform.GetChild(pauseMenu.transform.childCount - 1).gameObject.GetComponent<Toggle>().isOn = Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen;

        goos.GetComponent<PlayerMovement>().restart.performed += context => Load("", true);
    }

    public void pause(bool paused){
        //pauseMenu = GameObject.Find("Pause");
        pauseMenu.SetActive(paused);
        if(paused){
            Time.timeScale = 0;
        } else {
            Time.timeScale = 1;
        }
    }

    public void mainMenu(){
        //goos.gameObject.GetComponent<PlayerMovement>().move.Disable();
        goos.gameObject.GetComponent<PlayerMovement>().restart.Disable();
        goos.gameObject.GetComponent<PlayerMovement>().space.Disable();

        esc.Disable();
        Time.timeScale = 1;
        SceneManager.LoadScene("Main Menu");
    }

    public void setFullscreen(Toggle fullscreen){
        if(fullscreen.isOn){
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        } else {
            Screen.fullScreenMode = FullScreenMode.Windowed;
        }
    }

    void popUp(string text){
        popBox.SetActive(true);
        popBox.transform.GetChild(2).gameObject.GetComponent<TMP_Text>().text = text;
    }

    public void popUpClose (){
        popBox.SetActive(false);
    }

    public void loadPrompt(){
        pause(false);
        Time.timeScale = 0;
        var extensions = new [] {
            new ExtensionFilter("levels", "txt", "goose"),
            new ExtensionFilter("All Files", "*" ),
        };
        string path = "";

        try {
            #if !UNITY_WEBGL
                path = StandaloneFileBrowser.OpenFilePanel("Load custom level", "", extensions, false)[0];
            #else
                [DllImport("__Internal")]
                static extern void UploadFile(string gameObjectName, string methodName, string filter, bool multiple);
                UploadFile(gameObject.name, "OnFileUpload", "goose", false);
                path = loadPath;
            #endif
        } catch (Exception e) {
            popUp(e.ToString());
            return;
        }
        if (path == null || path == ""){
            popUp("no path selected");
            return;
        }

        Load(path, false);
    }

    public void Load(string path, bool firstLoad){
        Time.timeScale = 0;
        //Converts custom level to string
        if(firstLoad){
            if(levelTemp.levelPlaying != null){
                currentLvl = levelTemp.levelPlaying;
            } else {
                currentLvl = levelTemp.currentLvl;
            }
        } else {
            currentLvl = JsonConvert.DeserializeObject<levelData>(File.ReadAllText(path));
        }

        while (lvlDaddy.childCount > 0) {
            DestroyImmediate(lvlDaddy.GetChild(0).gameObject);
        }

        while (eggDaddy.childCount > 0) {
            DestroyImmediate(eggDaddy.GetChild(0).gameObject);
        }

        placedBlocks = new GameObject [currentLvl.size[0], currentLvl.size[1]];

        placeBorder();

        Transform blockDaddy = Instantiate(new GameObject (), new Vector3(), new Quaternion(), lvlDaddy).transform;
            Rigidbody2D newRB = blockDaddy.gameObject.AddComponent<Rigidbody2D>();
            newRB.bodyType = RigidbodyType2D.Static;
            blockDaddy.gameObject.AddComponent<CompositeCollider2D>();
        Transform platformDaddy = Instantiate(new GameObject (), new Vector3(), new Quaternion(), lvlDaddy).transform;
            newRB = platformDaddy.gameObject.AddComponent<Rigidbody2D>();
            newRB.bodyType = RigidbodyType2D.Static;
            CompositeCollider2D newCC = platformDaddy.gameObject.AddComponent<CompositeCollider2D>();
            newCC.usedByEffector = true;
            PlatformEffector2D pE = platformDaddy.gameObject.AddComponent<PlatformEffector2D>();
            pE.surfaceArc = 1;
        Transform spikeDaddy = Instantiate(new GameObject (), new Vector3(), new Quaternion(), lvlDaddy).transform;
            newRB = spikeDaddy.gameObject.AddComponent<Rigidbody2D>();
            newRB.bodyType = RigidbodyType2D.Static;
            newCC = spikeDaddy.gameObject.AddComponent<CompositeCollider2D>();
            newCC.isTrigger = true;
            spikeDaddy.AddComponent<death>();


        foreach(KeyValuePair<coordinate2D, block> block in currentLvl.blocks){
            //int x = Mathf.FloorToInt((i - 4) / size[1]);
            //Debug.Log("(" + (i - 4 - x * size[0]) + ", " + x + ")");
            if(block.Value.type == blockType.block){
                placedBlocks[block.Value.placePos.x, block.Value.placePos.y] = Instantiate(playerPrefabs.block, new Vector3(block.Value.placePos.x, block.Value.placePos.y, 0), new Quaternion(), blockDaddy);
                //Debug.Log(block.Value.blockVer);
            }
            if(block.Value.type == blockType.spawn && block.Value.coreTile){
                placedBlocks[block.Value.placePos.x, block.Value.placePos.y] = Instantiate(playerPrefabs.spawn, new Vector3(block.Value.placePos.x, block.Value.placePos.y, 0), new Quaternion(), lvlDaddy);
            }
            if(block.Value.type == blockType.button && block.Value.coreTile){
                placedBlocks[block.Value.placePos.x, block.Value.placePos.y] = Instantiate(playerPrefabs.button, new Vector3(block.Value.placePos.x, block.Value.placePos.y, 0), new Quaternion(), lvlDaddy);
            }
            if(block.Value.type == blockType.door && block.Value.coreTile){
                placedBlocks[block.Value.placePos.x, block.Value.placePos.y] = Instantiate(playerPrefabs.door, new Vector3(block.Value.placePos.x, block.Value.placePos.y, 0), new Quaternion(), lvlDaddy);
            }
            if(block.Value.type == blockType.platform){
                placedBlocks[block.Value.placePos.x, block.Value.placePos.y] = Instantiate(playerPrefabs.platform, new Vector3(block.Value.placePos.x, block.Value.placePos.y, 0), new Quaternion(), platformDaddy);
            }
            if(block.Value.type == blockType.obstacle){
                placedBlocks[block.Value.placePos.x, block.Value.placePos.y] = Instantiate(playerPrefabs.spike, new Vector3(block.Value.placePos.x, block.Value.placePos.y, 0), new Quaternion(), spikeDaddy);
            }
        }

        levelTemp.levelPlaying = currentLvl;
        levelTemp.currentLvl = currentLvl;

        refresh();
        
        goos.position = new Vector3(currentLvl.getTile(blockType.spawn, true).placePos.x, currentLvl.getTile(blockType.spawn, true).placePos.y, 0);
        goos.gameObject.GetComponent<SpriteRenderer>().enabled = true;
        goos.gameObject.GetComponent<PlayerMovement>().enabled = true;
        goos.gameObject.GetComponent<BoxCollider2D>().enabled = true;
        goos.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        goos.gameObject.GetComponent<PlayerMovement>().eggCount = currentLvl.eggCount;
        goos.gameObject.GetComponent<PlayerMovement>().ones.SetTrigger("reset");
        goos.gameObject.GetComponent<PlayerMovement>().tens.SetTrigger("reset");
        if(currentLvl.eggCount <= 0){
            goos.gameObject.GetComponent<PlayerMovement>().eggCounterDaddy.SetActive(false);
        } else {
            goos.gameObject.GetComponent<PlayerMovement>().eggCounterDaddy.SetActive(true);
        }
        Time.timeScale = 1;
    }

    public IEnumerator resetPlayerPos(){
        //goos.gameObject.GetComponent<SpriteRenderer>().enabled = false;
        goos.gameObject.GetComponent<PlayerMovement>().eggCooldown = true;
        goos.gameObject.GetComponent<PlayerMovement>().enabled = false;
        goos.gameObject.GetComponent<BoxCollider2D>().enabled = false;
        goos.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        goos.position = new Vector3(currentLvl.getTile(blockType.spawn, true).placePos.x, currentLvl.getTile(blockType.spawn, true).placePos.y, 0);
        yield return new WaitForSeconds(1.0f);
        Load("", true);
    }

    void refresh(){
        for(int x = 0; x < currentLvl.size[0]; x++){
            for(int y = 0; y < currentLvl.size[1]; y++){
                if(checkTileOccupancy(x, y) == 0){
                        //checks if the block is a column
                        bool column = false;
                        if(checkTileOccupancy(x - 1, y) != 0 && checkTileOccupancy(x + 1, y) != 0 && checkTileOccupancy(x, y - 1) == 0 && checkTileOccupancy(x, y + 1) == 0){
                            bool botRoot = false;
                            bool topRoot = false;
                            bool topFound = false;
                            bool botFound = false;
                            if(checkTileOccupancy(x + 1, y + 1) == 0 || checkTileOccupancy(x - 1, y + 1) == 0){
                                topRoot = true;
                                topFound = true;
                                int n = Math.Abs((Mathf.RoundToInt(x / (float)Math.PI) * Mathf.RoundToInt(currentLvl.seed / 17)) + (Mathf.RoundToInt(y * (float)Math.PI) * Mathf.RoundToInt(currentLvl.seed / 19)));
                                int index = n % w1Columns["single"].Count;
                                currentLvl.getTile(x, y).blockVer = index;
                            }
                            if(checkTileOccupancy(x + 1, y - 1) == 0 || checkTileOccupancy(x - 1, y - 1) == 0){
                                botRoot = true;
                                botFound = true;
                            }
                            if(topRoot && botRoot){
                                int n = Math.Abs((Mathf.RoundToInt(x / (float)Math.PI) * Mathf.RoundToInt(currentLvl.seed / 17)) + (Mathf.RoundToInt(y * (float)Math.PI) * Mathf.RoundToInt(currentLvl.seed / 19)));
                                int index = n % w1Columns["single"].Count;
                                //Debug.Log(index);
                                currentLvl.getTile(x, y).blockVer = index;
                                placedBlocks[x, y].GetComponent<SpriteRenderer>().sprite = w1Columns["single"][currentLvl.getTile(x, y).blockVer];
                                column = true;
                            }

                            int i = 0;
                            while(!topFound && checkTileOccupancy(x, y + i) == 0){
                                i++;
                                if(checkTileOccupancy(x, y + i) == 0 && (checkTileOccupancy(x + 1, y + i) == 0 || checkTileOccupancy(x - 1, y + i) == 0)){
                                    int n = Math.Abs((Mathf.RoundToInt(x / (float)Math.PI) * Mathf.RoundToInt(currentLvl.seed / 17)) + (Mathf.RoundToInt((y + i - 1) * (float)Math.PI) * Mathf.RoundToInt(currentLvl.seed / 19)));
                                    int index = n % w1Columns["single"].Count;
                                    currentLvl.getTile(x, y).blockVer = index;
                                    topFound = true;
                                }
                            }
                            i = 0;
                            while(!botFound && checkTileOccupancy(x, y - i) == 0){
                                i++;
                                if(checkTileOccupancy(x, y - i) == 0 && (checkTileOccupancy(x + 1, y - i) == 0 || checkTileOccupancy(x - 1, y - i) == 0)){
                                    botFound = true;
                                }
                            }

                            if(topFound && botFound){
                                try {
                                if(topRoot && !botRoot){
                                    placedBlocks[x, y].GetComponent<SpriteRenderer>().sprite = w1Columns["top"][currentLvl.getTile(x, y).blockVer];
                                    column = true; 
                                }
                                if(!topRoot && botRoot){
                                    placedBlocks[x, y].GetComponent<SpriteRenderer>().sprite = w1Columns["bot"][currentLvl.getTile(x, y).blockVer];
                                    column = true;
                                }
                                if(!topRoot && !botRoot){
                                    placedBlocks[x, y].GetComponent<SpriteRenderer>().sprite = w1Columns["mid"][currentLvl.getTile(x, y).blockVer];
                                    column = true; 
                                }
                                } catch {
                                    //Debug.LogError(new coordinate2D (x, y));
                                }
                            }
                        }
                        if(column == false){
                        //currentLvl.getTile(x, y).blockVer = Mathf.RoundToInt((((blocks.Count - 1) / 2) * Mathf.Sin((currentLvl.seed * 928.359f / 69385) * (currentLvl.seed * (x + 258) * (y + 2)))) + (blocks.Count - 1) / 2);
                        placedBlocks[x, y].GetComponent<SpriteRenderer>().sprite = blocks[
                            currentLvl.getTile(x, y).blockVer];
                    if(checkTileOccupancy(x+1, y) == 0 &&
                        checkTileOccupancy(x-1, y) == 0 &&
                        checkTileOccupancy(x, y+1) == 0 &&
                        checkTileOccupancy(x, y-1) == 0 &&
                        checkTileOccupancy(x+1, y+1) == 0 &&
                        checkTileOccupancy(x-1, y+1) == 0 &&
                        checkTileOccupancy(x+1, y-1) == 0 &&
                        checkTileOccupancy(x-1, y-1) == 0){
                            //currentLvl.getTile(x, y).blockVer = Mathf.RoundToInt((((blocks.Count - 1) / 2) * Mathf.Sin((currentLvl.seed * 928.359f / 69385) * (currentLvl.seed * (x + 258) * (y + 2)))) + (blocks.Count - 1) / 2);
                            placedBlocks[x, y].GetComponent<SpriteRenderer>().sprite = darkBlocks[
                                currentLvl.getTile(x, y).blockVer];
                        
                        if(checkTileOccupancy(x+2, y) == 0 &&
                            checkTileOccupancy(x-2, y) == 0 &&
                            checkTileOccupancy(x, y+2) == 0 &&
                            checkTileOccupancy(x, y-2) == 0 &&
                            checkTileOccupancy(x+1, y+1) == 0 &&
                            checkTileOccupancy(x-1, y+1) == 0 &&
                            checkTileOccupancy(x+1, y-1) == 0 &&
                            checkTileOccupancy(x-1, y-1) == 0){
                                //currentLvl.getTile(x, y).blockVer = Mathf.RoundToInt((((blocks.Count - 1) / 2) * Mathf.Sin((currentLvl.seed * 928.359f / 69385) * (currentLvl.seed * (x + 258) * (y + 2)))) + (blocks.Count - 1) / 2);
                                placedBlocks[x, y].GetComponent<SpriteRenderer>().sprite = darkerBlocks[
                                    currentLvl.getTile(x, y).blockVer];

                            if(checkTileOccupancy(x+3, y) == 0 &&
                                checkTileOccupancy(x-3, y) == 0 &&
                                checkTileOccupancy(x, y+3) == 0 &&
                                checkTileOccupancy(x, y-3) == 0 &&
                                checkTileOccupancy(x+1, y+2) == 0 &&
                                checkTileOccupancy(x-1, y+2) == 0 &&
                                checkTileOccupancy(x+1, y-2) == 0 &&
                                checkTileOccupancy(x-1, y-2) == 0 &&
                                checkTileOccupancy(x+2, y+1) == 0 &&
                                checkTileOccupancy(x-2, y+1) == 0 &&
                                checkTileOccupancy(x+2, y-1) == 0 &&
                                checkTileOccupancy(x-2, y-1) == 0){
                                    placedBlocks[x, y].GetComponent<SpriteRenderer>().sprite = darkestBlock;
                            }
                        }
                    }
                }
            }else if (currentLvl.getTile(x, y) != null){
            if(currentLvl.blocks[new coordinate2D (x, y)].type == blockType.platform){
                int n = Math.Abs((Mathf.RoundToInt(x / (float)Math.PI) * Mathf.RoundToInt(currentLvl.seed / 17)) + (Mathf.RoundToInt(y * (float)Math.PI) * Mathf.RoundToInt(currentLvl.seed / 19))) + x * y;
                int index = Mathf.RoundToInt(n / 10) % 2;
                currentLvl.getTile(x, y).blockVer = index;

                SpriteRenderer sr = placedBlocks[x, y].GetComponent<SpriteRenderer>();
                BoxCollider2D box = placedBlocks[x, y].GetComponent<BoxCollider2D>();
                sr.sprite = oneWays.Middle[currentLvl.getTile(x, y).blockVer];

                if(currentLvl.getTile(x + 1, y) == null && currentLvl.getTile(x - 1, y) != null){
                    if(currentLvl.getTile(x - 1, y).type == blockType.platform){
                        sr.sprite = oneWays.endRight[currentLvl.getTile(x, y).blockVer];
                        box.offset = new Vector2 (-0.12f, box.offset.y);
                        box.size = new Vector2 (0.76f, box.size.y);
                    }
                } else {
                    if(currentLvl.getTile(x - 1, y) != null){
                        if(currentLvl.getTile(x - 1, y).type == blockType.platform && (currentLvl.getTile(x + 1, y).type == blockType.button || currentLvl.getTile(x + 1, y).type == blockType.door || currentLvl.getTile(x + 1, y).type == blockType.spawn)){
                            sr.sprite = oneWays.endRight[currentLvl.getTile(x, y).blockVer];
                            box.offset = new Vector2 (-0.12f, box.offset.y);
                            box.size = new Vector2 (0.76f, box.size.y);
                        }
                    }
                }
                if(currentLvl.getTile(x - 1, y) == null && currentLvl.getTile(x + 1, y) != null){
                    if(currentLvl.getTile(x + 1, y).type == blockType.platform){
                        sr.sprite = oneWays.endLeft[currentLvl.getTile(x, y).blockVer];
                        box.offset = new Vector2 (0.12f, box.offset.y);
                        box.size = new Vector2 (0.76f, box.size.y);
                    }
                } else {
                    if(currentLvl.getTile(x + 1, y) != null){
                        if(currentLvl.getTile(x + 1, y).type == blockType.platform && (currentLvl.getTile(x - 1, y).type == blockType.button || currentLvl.getTile(x - 1, y).type == blockType.door || currentLvl.getTile(x - 1, y).type == blockType.spawn)){
                            sr.sprite = oneWays.endLeft[currentLvl.getTile(x, y).blockVer];
                            box.offset = new Vector2 (0.12f, box.offset.y);
                            box.size = new Vector2 (0.76f, box.size.y);
                        }
                    }
                }

                bool wallLeft = false;
                if((checkTileOccupancy(x - 1, y) == 0 || x - 1 < 0) && (checkTileOccupancy(x - 2, y) == 0 || x - 2 < 0)){
                    sr.sprite = oneWays.endRight[2];
                    if(currentLvl.getTile(x + 1, y) != null){
                        if(currentLvl.getTile(x + 1, y).type == blockType.platform){
                            sr.sprite = oneWays.Middle[2];
                        }
                    }
                    wallLeft = true;
                }
                if((checkTileOccupancy(x + 1, y) == 0 || x + 1 > currentLvl.size[0] - 1) && (checkTileOccupancy(x + 2, y) == 0 || x + 2 > currentLvl.size[0] - 1)){
                    sr.sprite = oneWays.endLeft[2];
                    if(currentLvl.getTile(x - 1, y) != null){
                        if(currentLvl.getTile(x - 1, y).type == blockType.platform){
                            sr.sprite = oneWays.Middle[3];
                        }
                    }
                    if(wallLeft){
                        sr.sprite = oneWays.Middle[4];
                    }
                }
            } else if(currentLvl.getTile(x, y).type == blockType.obstacle){
                SpriteRenderer sr = placedBlocks[x, y].GetComponent<SpriteRenderer>();
                sr.color = new Color(1, .88f, .88f);
                if(checkTileOccupancy(x, y - 1) == 0 && checkTileOccupancy(x, y + 1) != 0){
                    sr.sprite = this.GetComponent<w1Spikes>().bottom;
                    placedBlocks[x, y].GetComponent<BoxCollider2D>().size = new Vector2 (1, 0.4f);
                    placedBlocks[x, y].GetComponent<BoxCollider2D>().offset = new Vector2 (0, -0.3f);
                } else if(checkTileOccupancy(x, y - 1) != 0 && checkTileOccupancy(x, y + 1) == 0){
                    sr.sprite = this.GetComponent<w1Spikes>().top;
                    placedBlocks[x, y].GetComponent<BoxCollider2D>().size = new Vector2 (1, 0.4f);
                    placedBlocks[x, y].GetComponent<BoxCollider2D>().offset = new Vector2 (0, 0.3f);
                } else{
                    sr.sprite = this.GetComponent<w1Spikes>().general;
                    placedBlocks[x, y].GetComponent<BoxCollider2D>().size = new Vector2 (1, 1);
                    placedBlocks[x, y].GetComponent<BoxCollider2D>().offset = new Vector2 (0, 0);
                }
            }}}
        }
    }

    int checkTileOccupancy(int x, int y){
        if(y < 0){
            return 0;
        }
        try {
            if(currentLvl.getTile(x, y).type == blockType.block){
                return 0;
            } else {
                return 1;
            }
        } catch {
            return 1;
        }
    }

    void placeBorder(){
                //Creates border around entire grid using old blocks
        foreach(Transform child in blockBorderDaddy){
            GameObject.Destroy(child.gameObject);
        }
        //Places all the blocks bordering the grid
        placeBorderBlock(0, new Vector3(-1, currentLvl.size[1], 0));
        for(int i = 0; i < currentLvl.size[0]; i++){
            placeBorderBlock(1, new Vector3 (i, currentLvl.size[1], 0));
        }

        placeBorderBlock(2, new Vector3(currentLvl.size[0], currentLvl.size[1], 0));
        for(int i = -25; i < currentLvl.size[1]; i++){
            placeBorderBlock(4, new Vector3 (currentLvl.size[0], i, 0));
        }

        for(int i = -25; i < currentLvl.size[1]; i++){
            placeBorderBlock(3, new Vector3 (-1, i, 0));
        }

        for(int i = 0; i < currentLvl.size[0]; i++){
            placeBorderBlock(8, new Vector3 (i, 0, 0));
        }

        //Places blocks outside the border
        placeBorderVoid(9, new Vector3(-10, currentLvl.size[1] / 2, 0), 18, currentLvl.size[1] + 26);
        placeBorderVoid(9, new Vector3(currentLvl.size[0] + 10, currentLvl.size[1] / 2, 0), 19, currentLvl.size[1] + 26);
        placeBorderVoid(9, new Vector3((currentLvl.size[0] - 1f) / 2f, currentLvl.size[1] + 10, 0), currentLvl.size[0] + 13, 19);
        placeBorderVoid(10, new Vector3((currentLvl.size[0] - 1f) / 2f, -10f, 0), currentLvl.size[0], 19);
    }

    void placeBorderBlock(int block, Vector3 newPos){
        Transform newBorderBlock = Instantiate(tilePrefab, newPos, new Quaternion(), blockBorderDaddy).transform;
        newBorderBlock.gameObject.GetComponent<SpriteRenderer>().sprite = borderBlocks[block];
        newBorderBlock.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 0;
        newBorderBlock.gameObject.GetComponent<SpriteRenderer>().color = new Color (0.65f, 0.65f, 0.65f);
        newBorderBlock.gameObject.GetComponent<SpriteRenderer>().material = litMat;
        newBorderBlock.localScale = new Vector3 (1, 1, 1);
        if(block == 8){
            newBorderBlock.GetComponent<BoxCollider2D>().enabled = false;
            newBorderBlock.GetComponent<SpriteRenderer>().sortingOrder = 1;
        }
    }

    void placeBorderVoid(int block, Vector3 newPos, float width, float height){
        Transform newBorderBlock = Instantiate(tilePrefab, newPos, new Quaternion(), blockBorderDaddy).transform;
        newBorderBlock.gameObject.GetComponent<SpriteRenderer>().sprite = borderBlocks[block];
        newBorderBlock.gameObject.GetComponent<SpriteRenderer>().color = new Color (0.65f, 0.65f, 0.65f);
        newBorderBlock.gameObject.GetComponent<SpriteRenderer>().sortingOrder = -2;
        newBorderBlock.gameObject.GetComponent<SpriteRenderer>().material = litMat;
        newBorderBlock.localScale = new Vector3(width, height, 1);

        if(newPos.y <= 0){
            newBorderBlock.AddComponent<death>();
            BoxCollider2D deathZone = newBorderBlock.AddComponent<BoxCollider2D>();
            deathZone.isTrigger = true;
        }
    }
}
