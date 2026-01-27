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
using Unity.Collections;
using UnityEngine.EventSystems;

public class levelPlayer : MonoBehaviour
{

    #if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void UploadFile(string gameObjectName, string methodName, string filter, bool multiple);
    #endif

    //public DefaultAsset lvlToLoad;
    levelData currentLvl;

    public InputAction esc;

    [SerializeField] LayerMask groundLayer;

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

    public twoState twoStateGlobal;

    public Transform goos;
    Transform spawn;

    public int deathCount = 0;
    float time = 0;

    [SerializeField] Animator completionAnim;
    [SerializeField] AnimationState completionAnimState;

    // Start is called before the first frame update
    void Start()
    {
        completionAnim.speed = 0;

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

    void Update(){
        time += Time.deltaTime;
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

        goos.gameObject.GetComponent<PlayerMovement>().eggCooldown = true;
        goos.gameObject.GetComponent<PlayerMovement>().space.Disable();
        goos.gameObject.GetComponent<PlayerMovement>().eggHop.Disable();
        goos.gameObject.GetComponent<PlayerMovement>().restart.Disable();
        goos.transform.GetChild(0).gameObject.GetComponent<BoxCollider2D>().enabled = false;
        goos.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;

        Time.timeScale = 1;
        StartCoroutine(library());
    }

    public System.Collections.IEnumerator library(){
        GameObject.FindGameObjectWithTag("transitions").GetComponent<transitions>().playTrans(false, 0);
        yield return new WaitUntil(() => GameObject.FindGameObjectWithTag("transitions").GetComponent<transitions>().anims[0].GetCurrentAnimatorStateInfo(0).normalizedTime < 1);
        yield return new WaitUntil(() => GameObject.FindGameObjectWithTag("transitions").GetComponent<transitions>().anims[0].GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        SceneManager.LoadScene("Level Library");
    }

    public System.Collections.IEnumerator replay(){
        GameObject.FindGameObjectWithTag("transitions").GetComponent<transitions>().playTrans(false, 0);
        yield return new WaitUntil(() => GameObject.FindGameObjectWithTag("transitions").GetComponent<transitions>().anims[0].GetCurrentAnimatorStateInfo(0).normalizedTime < 1);
        yield return new WaitUntil(() => GameObject.FindGameObjectWithTag("transitions").GetComponent<transitions>().anims[0].GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
            currentLvl = GameObject.FindGameObjectWithTag("gameManager").GetComponent<gameManager>().currentLvl;
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
            blockDaddy.gameObject.layer = 8;
            Rigidbody2D newRB = blockDaddy.gameObject.AddComponent<Rigidbody2D>();
            newRB.bodyType = RigidbodyType2D.Static;
            blockDaddy.gameObject.AddComponent<CompositeCollider2D>();
        Transform platformDaddy = Instantiate(new GameObject (), new Vector3(), new Quaternion(), lvlDaddy).transform;
            platformDaddy.gameObject.layer = 8;
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
            death killScript = spikeDaddy.AddComponent<death>();
            killScript.source = 1;


        foreach(KeyValuePair<coordinate2D, block> block in currentLvl.blocks){

            if(block.Value.tags == null){
                block.Value.tags = new Dictionary<string, string>();
            }
            //int x = Mathf.FloorToInt((i - 4) / size[1]);
            //Debug.Log("(" + (i - 4 - x * size[0]) + ", " + x + ")");
            if(block.Value.placePos.x >= 0 && block.Value.placePos.y >= 0){
            if(block.Value.type == blockType.block){
                placedBlocks[block.Value.placePos.x, block.Value.placePos.y] = Instantiate(playerPrefabs.block, new Vector3(block.Value.placePos.x, block.Value.placePos.y, 0), new Quaternion(), blockDaddy);
                //Debug.Log(block.Value.blockVer);
            }
            if(block.Value.type == blockType.spawn && block.Value.coreTile){
                placedBlocks[block.Value.placePos.x, block.Value.placePos.y] = Instantiate(playerPrefabs.spawn, new Vector3(block.Value.placePos.x, block.Value.placePos.y - 0.1f, 0), new Quaternion(), lvlDaddy);
                spawn = placedBlocks[block.Value.placePos.x, block.Value.placePos.y].transform;
            }
            if(block.Value.type == blockType.button && block.Value.coreTile){
                placedBlocks[block.Value.placePos.x, block.Value.placePos.y] = Instantiate(playerPrefabs.button, new Vector3(block.Value.placePos.x, block.Value.placePos.y - 0.1f, 0), new Quaternion(), lvlDaddy);
            }
            if(block.Value.type == blockType.door && block.Value.coreTile){
                placedBlocks[block.Value.placePos.x, block.Value.placePos.y] = Instantiate(playerPrefabs.door, new Vector3(block.Value.placePos.x, block.Value.placePos.y - 0.1f, 0), new Quaternion(), lvlDaddy);
            }
            if(block.Value.type == blockType.platform){
                placedBlocks[block.Value.placePos.x, block.Value.placePos.y] = Instantiate(playerPrefabs.platform, new Vector3(block.Value.placePos.x, block.Value.placePos.y, 0), new Quaternion(), platformDaddy);
            }
            if(block.Value.type == blockType.spike){
                placedBlocks[block.Value.placePos.x, block.Value.placePos.y] = Instantiate(playerPrefabs.spike, new Vector3(block.Value.placePos.x, block.Value.placePos.y, 0), new Quaternion(), spikeDaddy);
            }
            if(block.Value.type == blockType.tempPlat){
                placedBlocks[block.Value.placePos.x, block.Value.placePos.y] = Instantiate(playerPrefabs.tempBlock, new Vector3(block.Value.placePos.x, block.Value.placePos.y, 0), new Quaternion(), platformDaddy);
            }
            if(block.Value.type == blockType.blueBlock){
                placedBlocks[block.Value.placePos.x, block.Value.placePos.y] = Instantiate(playerPrefabs.twoStateBlue, new Vector3(block.Value.placePos.x, block.Value.placePos.y, 0), new Quaternion(), blockDaddy);
            }
            if(block.Value.type == blockType.redBlock){
                placedBlocks[block.Value.placePos.x, block.Value.placePos.y] = Instantiate(playerPrefabs.twoStateRed, new Vector3(block.Value.placePos.x, block.Value.placePos.y, 0), new Quaternion(), blockDaddy);
            }
            if(block.Value.type == blockType.blueSpike){
                placedBlocks[block.Value.placePos.x, block.Value.placePos.y] = Instantiate(playerPrefabs.twoStateSpikeBlue, new Vector3(block.Value.placePos.x, block.Value.placePos.y, 0), new Quaternion(), spikeDaddy);
            }
            if(block.Value.type == blockType.redSpike){
                placedBlocks[block.Value.placePos.x, block.Value.placePos.y] = Instantiate(playerPrefabs.twoStateSpikeRed, new Vector3(block.Value.placePos.x, block.Value.placePos.y, 0), new Quaternion(), spikeDaddy);
            }
            if(block.Value.type == blockType.twoStateButton && block.Value.coreTile){
                placedBlocks[block.Value.placePos.x, block.Value.placePos.y] = Instantiate(playerPrefabs.twoStateButton, new Vector3(block.Value.placePos.x, block.Value.placePos.y, 0), new Quaternion(), blockDaddy);
            }
            if(block.Value.type == blockType.twoStateLever && block.Value.coreTile){
                placedBlocks[block.Value.placePos.x, block.Value.placePos.y] = Instantiate(playerPrefabs.twoStateLever, new Vector3(block.Value.placePos.x, block.Value.placePos.y, 0), new Quaternion(), blockDaddy);
            }
            }
        }

        levelTemp.levelPlaying = currentLvl;
        levelTemp.currentLvl = currentLvl;

        refresh();
        
        goos.position = new Vector3(currentLvl.getTile(blockType.spawn, true).placePos.x, currentLvl.getTile(blockType.spawn, true).placePos.y, 0);
        goos.gameObject.GetComponent<SpriteRenderer>().enabled = true;

        goos.gameObject.GetComponent<PlayerMovement>().enabled = true;
        Animator anim = goos.GetComponent<PlayerMovement>().animate;
        SpriteRenderer sr = goos.GetComponent<PlayerMovement>().sr;
        goos.gameObject.GetComponent<PlayerMovement>().enabled = false;

        goos.transform.GetChild(0).gameObject.GetComponent<BoxCollider2D>().enabled = true;
        goos.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        goos.gameObject.GetComponent<PlayerMovement>().eggCount = currentLvl.eggCount;
        goos.gameObject.GetComponent<PlayerMovement>().ones.SetTrigger("reset");
        goos.gameObject.GetComponent<PlayerMovement>().tens.SetTrigger("reset");
        goos.gameObject.GetComponent<PlayerMovement>().space.Enable();
        goos.gameObject.GetComponent<PlayerMovement>().eggHop.Enable();
        goos.gameObject.GetComponent<PlayerMovement>().restart.Enable();
        goos.GetComponent<PlayerMovement>().animate.enabled = true;
        goos.GetComponent<PlayerMovement>().animate.SetBool("dead", false);
        twoStateGlobal = twoState.red;
        if(currentLvl.eggCount <= 0){
            goos.gameObject.GetComponent<PlayerMovement>().eggCounterDaddy.SetActive(false);
        } else {
            goos.gameObject.GetComponent<PlayerMovement>().eggCounterDaddy.SetActive(true);
        }
        GameObject.FindGameObjectWithTag("transitions").GetComponent<transitions>().playTrans(true, 0);
        spawn.GetChild(1).GetComponent<Animator>().enabled = false;
        spawn.GetChild(1).localScale = new Vector3 (1, 1, 1);
        goos.parent = spawn.GetChild(1);
        spawn.GetChild(1).localScale = new Vector3 (0.45f, 0.45f, 0.45f);
        spawn.GetChild(1).GetComponent<Animator>().enabled = true;
        anim.SetBool("walking", true);
        anim.Play("Walk");
        sr.flipX = false;
        Time.timeScale = 1;
        StartCoroutine(startAnim());
    }

    public IEnumerator startAnim(){
        goos.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;

        yield return new WaitUntil(() => spawn.GetChild(1).GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime < 1);
        yield return new WaitUntil(() => spawn.GetChild(1).GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);

        goos.parent = null;
        goos.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        goos.GetComponent<PlayerMovement>().animate.SetBool("Walking", false);
        goos.gameObject.GetComponent<PlayerMovement>().enabled = true;
    }

    public IEnumerator resetPlayerPos(int src){
        time = 0;

        goos.gameObject.GetComponent<PlayerMovement>().eggCooldown = true;
        goos.gameObject.GetComponent<PlayerMovement>().space.Disable();
        goos.gameObject.GetComponent<PlayerMovement>().eggHop.Disable();
        goos.gameObject.GetComponent<PlayerMovement>().restart.Disable();
        goos.GetComponent<PlayerMovement>().animate.enabled = true;
        goos.gameObject.GetComponent<PlayerMovement>().enabled = false;
        goos.GetComponent<PlayerMovement>().animate.SetBool("dead", true);
        if(src == 0){
            goos.GetComponent<PlayerMovement>().animate.SetTrigger("fall");
        } else if (src == 1){
            goos.GetComponent<PlayerMovement>().animate.SetTrigger("generic");
        }
        goos.transform.GetChild(0).gameObject.GetComponent<BoxCollider2D>().enabled = false;
        goos.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;

        yield return new WaitUntil(() => GameObject.FindGameObjectWithTag("transitions").GetComponent<transitions>().anims[1].GetCurrentAnimatorStateInfo(0).normalizedTime < 1);
        yield return new WaitUntil(() => GameObject.FindGameObjectWithTag("transitions").GetComponent<transitions>().anims[1].GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);

        //goos.gameObject.GetComponent<SpriteRenderer>().enabled = false;
        goos.position = new Vector3(currentLvl.getTile(blockType.spawn, true).placePos.x, currentLvl.getTile(blockType.spawn, true).placePos.y, 0);
        goos.GetComponent<PlayerMovement>().animate.SetBool("dead", false);
        yield return new WaitForSeconds(1.0f);
        deathCount++;
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
            } else if(currentLvl.getTile(x, y).type == blockType.spike){
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
            } else if (currentLvl.blocks[new coordinate2D (x, y)].type == blockType.tempPlat) {
                int n = Math.Abs((Mathf.RoundToInt(x / (float)Math.PI) * Mathf.RoundToInt(currentLvl.seed / 17)) + (Mathf.RoundToInt(y * (float)Math.PI) * Mathf.RoundToInt(currentLvl.seed / 19))) + x * y;
                int index = Mathf.RoundToInt(n / 10) % 3;

                SpriteRenderer sr = placedBlocks[x, y].transform.GetChild(0).GetChild(3).GetComponent<SpriteRenderer>();
                placedBlocks[x, y].transform.GetChild(0).GetComponent<tempPlatform>().type = index;
                Debug.Log(index);
                sr.sprite = this.GetComponent<extraSprites>().tempPlat[index];
                placedBlocks[x, y].transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().sprite = placedBlocks[x, y].transform.GetChild(0).GetComponent<tempPlatform>().spriteSelect(4);
                placedBlocks[x, y].transform.GetChild(0).GetChild(1).GetComponent<SpriteRenderer>().sprite = placedBlocks[x, y].transform.GetChild(0).GetComponent<tempPlatform>().spriteSelect(5);;
                placedBlocks[x, y].transform.GetChild(0).GetChild(2).GetComponent<SpriteRenderer>().sprite = placedBlocks[x, y].transform.GetChild(0).GetComponent<tempPlatform>().spriteSelect(6);;
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

        /*GameObject newBackground = Instantiate(Resources.Load<GameObject>("background"), blockBorderDaddy);
            newBackground.GetComponent<SpriteRenderer>().size = new Vector2 (currentLvl.size[0], currentLvl.size[1]);
            newBackground.transform.position = new Vector3 (currentLvl.size[0] / 2 - 0.5f, currentLvl.size[1] / 2 - 0.5f, 0);*/

        //Places blocks outside the border
        placeBorderVoid(9, new Vector3(-10, currentLvl.size[1] / 2, 0), 18, currentLvl.size[1] + 26);
        placeBorderVoid(9, new Vector3(currentLvl.size[0] + 10, currentLvl.size[1] / 2, 0), 19, currentLvl.size[1] + 26);
        placeBorderVoid(9, new Vector3((currentLvl.size[0] - 1f) / 2f, currentLvl.size[1] + 10, 0), currentLvl.size[0] + 13, 19);
        placeBorderVoid(10, new Vector3((currentLvl.size[0] - 1f) / 2f, -10f, 0), currentLvl.size[0], 19);
    }

    void placeBorderBlock(int block, Vector3 newPos){
        Transform newBorderBlock = Instantiate(tilePrefab, newPos, new Quaternion(), blockBorderDaddy).transform;
        newBorderBlock.gameObject.GetComponent<SpriteRenderer>().sprite = borderBlocks[block];
        if(block == 8){
            newBorderBlock.gameObject.GetComponent<SpriteRenderer>().sortingLayerName = "Goose";
        } else {
            newBorderBlock.GetComponent<BoxCollider2D>().compositeOperation = Collider2D.CompositeOperation.Merge;
        }
        newBorderBlock.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 5;
        newBorderBlock.gameObject.GetComponent<SpriteRenderer>().color = new Color (0.65f, 0.65f, 0.65f);
        newBorderBlock.gameObject.GetComponent<SpriteRenderer>().material = litMat;
        newBorderBlock.localScale = new Vector3 (1, 1, 1);
        if(block == 8){
            newBorderBlock.GetComponent<BoxCollider2D>().enabled = false;
            newBorderBlock.GetComponent<SpriteRenderer>().sortingOrder = 2;
        }
    }

    void placeBorderVoid(int block, Vector3 newPos, float width, float height){
        Transform newBorderBlock = Instantiate(tilePrefab, newPos, new Quaternion(), blockBorderDaddy).transform;
        newBorderBlock.gameObject.GetComponent<SpriteRenderer>().sprite = borderBlocks[block];
        newBorderBlock.gameObject.GetComponent<SpriteRenderer>().color = new Color (0.65f, 0.65f, 0.65f);
        if(block == 10){
            newBorderBlock.gameObject.GetComponent<SpriteRenderer>().sortingLayerName = "Goose";
        }
        newBorderBlock.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 4;
        newBorderBlock.gameObject.GetComponent<SpriteRenderer>().material = litMat;
        newBorderBlock.localScale = new Vector3(width, height, 1);

        if(newPos.y <= 0){
            newBorderBlock.AddComponent<death>();
            BoxCollider2D deathZone = newBorderBlock.AddComponent<BoxCollider2D>();
            deathZone.isTrigger = true;
            newBorderBlock.gameObject.layer = 9;
        }
    }

    public void Finish(){
        if(completionAnim.speed == 1){
            return;
        }
        goos.gameObject.GetComponent<PlayerMovement>().restart.Disable();
        completionAnim.speed = 1;

        string path = Application.persistentDataPath + "/Custom Levels/";
        makerProfile profile = JsonConvert.DeserializeObject<makerProfile>(GameObject.FindGameObjectWithTag("gameManager").GetComponent<gameManager>().decrypt(File.ReadAllText(path + "makerProfile.json")));
        profile.clearedLvls[currentLvl.levelID] = true;
        File.WriteAllText(path + "makerProfile.json", GameObject.FindGameObjectWithTag("gameManager").GetComponent<gameManager>().encrypt(JsonConvert.SerializeObject(profile, Formatting.Indented)), System.Text.Encoding.UTF8);

        completionAnim.transform.GetChild(4).GetComponent<TMP_Text>().text = "\n" + currentLvl.title + "\n" + "\n";
        if(!completionAnim.transform.GetChild(4).GetComponent<TMP_Text>().isTextOverflowing){
            completionAnim.transform.GetChild(4).GetComponent<TMP_Text>().text += "\n";
        }
        int t = Mathf.RoundToInt(time);
        string hours = Mathf.Floor(t / 3600).ToString();
        while(hours.Length < 2){
            hours = "0" + hours;
        }
        string minutes = (Mathf.Floor(t / 60) - Mathf.Floor(t / 3600) * 60).ToString();
        while(minutes.Length < 2){
            minutes = "0" + minutes;
        }
        string seconds = (Mathf.Floor(t) - Mathf.Floor(t / 60) * 60).ToString();
        while(seconds.Length < 2){
            seconds = "0" + seconds;
        }

        completionAnim.transform.GetChild(4).GetComponent<TMP_Text>().text += hours + ":" + minutes + ":" + seconds;
        completionAnim.transform.GetChild(4).GetComponent<TMP_Text>().text += "\n" + "\n" + (deathCount).ToString();
    }

    public void Replay(){
        goos.gameObject.GetComponent<PlayerMovement>().space.Disable();
        goos.gameObject.GetComponent<PlayerMovement>().eggHop.Disable();
        goos.gameObject.GetComponent<PlayerMovement>().restart.Disable();
        esc.Disable();
        StartCoroutine(replay());
    }

    public twoState otherState(){
        if(twoStateGlobal == twoState.blue){
            return twoState.red;
        } else {
            return twoState.blue;
        }
    }
}

public enum twoState
{
    red,
    blue
}
