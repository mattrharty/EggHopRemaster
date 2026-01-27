using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.IO;
using System;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;
using System.Text;
using System.Collections;
using UnityEngine.Purchasing.MiniJSON;

public class editorController : MonoBehaviour
{

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void UploadFile(string gameObjectName, string methodName, string filter, bool multiple);
#endif

    int[] size = {30, 30};

    public levelData currentLvl;

    [SerializeField]
    Material litMat;

    [SerializeField]
    int maxSize = 3;
    public TMP_InputField levelName;
    public int minSize = 6;
    public int maxSizeNum = 256;
    public TMP_InputField xField;
    public TMP_InputField yField;
    public TMP_InputField seedField;
    public int[] mousePos = {0,0};
    public GameObject tileCursor;
    public Sprite[] cursorPics;
    public Color[] cursorColors;

    public GameObject tilePrefab;
    public Transform gridDaddy;

    public mode placeMode;
    public mode placeSelected;
    public bool redSelect = true;

    public camControl camScript;

    public TextAsset jsonBlocks;
    public List<blockEditor> palette;

    public List<Button> buttons;
    public GameObject blockButtonPrefab;
    public Transform ButtonsDaddy;
    public List<Button> recentButtons;
    public List<mode> recentInts = new List<mode> {};

    int seed;

    //int[,] tileData = new int[30,30];
    int[,] tileDataClone = new int[30,30];

    public InputAction leftClick;
    public InputAction esc;
    public List<InputAction> placeButtons;
    public Button[] tools;
    public Sprite[] blocks;
    public Sprite[] darkBlocks;
    public Sprite[] darkerBlocks;
    public Sprite darkestBlock;
    public Sprite[] borderBlocks;
    public Transform blockBorderDaddy;
    public GameObject[,] placedBlocks = new GameObject[30,30];
    public GameObject[,] placedBlocksClone = new GameObject[30,30];

    public int fillX;
    public int fillY;
    public GameObject borderPrefab;
    public Transform borderDaddy;

    public Toggle singleFill;
    public Toggle blueRed;

    public  List<Sprite> w1ColTop;
    public  List<Sprite> w1ColMid;
    public  List<Sprite> w1ColBot;
    public  List<Sprite> w1ColSingle;
    Dictionary<string, List<Sprite>> w1Columns = new Dictionary<string, List<Sprite>>();

    w1OneWay oneWays;
    editorUI ui;

    int rotate = 0;

    tool toolSelected = tool.draw;    

    [SerializeField] Image snapshot;

    public TMP_Text versionDis;
    string loadPath;
    public GameObject[] otherObjects;
    /*
    0 -> Spawn
    1 -> Button
    2 -> Door 
    */

    bool loadingLevel = false;

    public List<Sprite> blockButton;

    [SerializeField]
    Material normMat;
    [SerializeField]
    Material redMat;

    gameManager gm;

    // Start is called before the first frame update
    void Start()
    {
        gm = GameObject.FindGameObjectWithTag("gameManager").GetComponent<gameManager>();

        ui = GameObject.Find("UIController").GetComponent<editorUI>();

        recentInts.Insert(0, mode.block);

        xField.characterLimit = maxSize;
        yField.characterLimit = maxSize;
        levelName.characterLimit = 100;
        xField.text = "30";
        yField.text = "30";

        w1Columns.Add("top", w1ColTop);
        w1Columns.Add("mid", w1ColMid); 
        w1Columns.Add("bot", w1ColBot); 
        w1Columns.Add("single", w1ColSingle); 

        camScript.bounds[0] = size[1];
        camScript.bounds[1] = size[0];
        camScript.bounds[2] = 0;
        camScript.bounds[3] = 0;
        currentLvl.size = size;

        placeSelected = mode.block;

        blockPalette blockPalette = JsonConvert.DeserializeObject<blockPalette>(jsonBlocks.text);
        foreach (byteEditor obj in blockPalette.blocks)
        {
            List<Sprite> priTex = new List<Sprite>();
            List<Sprite> secTex = new List<Sprite>();
            Debug.Log(obj.primaryTex);
            foreach (byte[] b in obj.primaryTex)
            {
                Texture2D newTex = new Texture2D(2, 2);
                newTex.LoadImage(b);
                priTex.Add(Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), new Vector2(0.5f, 0.5f)));
            }
            foreach (byte[] b in obj.secondaryTex)
            {
                Texture2D newTex = new Texture2D(2, 2);
                newTex.LoadImage(b);
                secTex.Add(Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), new Vector2(0.5f, 0.5f)));
            }
            palette.Add(new blockEditor(obj.name, obj.width, obj.height, obj.canFill, obj.isTwoState, obj.canReplace, priTex, secTex));
        }

        foreach (blockEditor obj in palette)
        {

            GameObject newButton = Instantiate(blockButtonPrefab, ButtonsDaddy);
            //Debug.Log(obj.primaryTex[0]);
            newButton.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = obj.primaryTex[0];

            ui.blockSelect.Add(obj.primaryTex[0]);

            newButton.GetComponent<Button>().onClick.AddListener(() => selectTool(obj));
            newButton.GetComponent<Button>().onClick.AddListener(() => selectTool2(newButton.GetComponent<Button>()));

            buttons.Add(newButton.GetComponent<Button>());
        }

        GameObject[] recentsObj = GameObject.FindGameObjectsWithTag("recent");
        foreach(GameObject obj in recentsObj){
            if (obj.name != "1" && obj.name != "2")
            {
                recentButtons.Add(obj.GetComponent<Button>());
            }
            else if (obj.name != "2")
            {
                recentButtons.Insert(0, obj.GetComponent<Button>());
            }
            else
            {
                recentButtons.Insert(1, obj.GetComponent<Button>());
            }
            obj.GetComponent<Image>().material = normMat;
        }

        seed = UnityEngine.Random.Range(100000, 999999);
        seedField.characterLimit = 6;
        seedField.text = seed.ToString();

        leftClick.Enable();
        leftClick.performed += context => leftClicked();

        esc.Enable();
        esc.performed += context => Quit();

        for (int i = 0; i < tools.Length; i++){
            if (i > 1){
                placeButtons.Add(new InputAction (i.ToString(), binding: "<Keyboard>/" + (i + 1).ToString()));
            } else {
                placeButtons.Add(new InputAction (i.ToString(), binding: "<Keyboard>/" + (Mathf.Abs(i - 1) + 1).ToString()));
            }
            placeButtons[i].Enable();
            Button toolButton = tools[i];
            //placeButtons[i].performed += context => hotkey(toolButton);
        }

        singleFill.interactable = true;
        singleFill.isOn = placeMode == mode.fill;

        blueRed.interactable = false;
        blueRed.isOn = redSelect;

        oneWays = this.GetComponent<w1OneWay>();

        Load(versionDis);

        gridReload();

        versionDis.text = "Egg Hop " + Application.version;
    }

    void hotkey(Button toolButton){
        selectTool(palette[Array.IndexOf(tools, toolButton)]);
        selectTool2(toolButton);
    }

    public void recent(int index)
    {
        if (recentInts.Count > index)
        {
            selectTool(palette[(int)recentInts[index]]);
            selectTool2(buttons[(int)recentInts[index]]);
            recentRefresh();
        }
    }

    void recentRefresh()
    {
        for (int i = 0; i < 3; i++) {
            if (recentInts.Count < i + 2)
            {
                recentButtons[i].transform.GetChild(0).GetChild(0).GetComponent<Image>().enabled = false;
            }
            else
            {
                recentButtons[i].transform.GetChild(0).GetChild(0).GetComponent<Image>().enabled = true;
                recentButtons[i].transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = ui.blockSelect[(int)recentInts[i]];
                recentButtons[i].transform.GetChild(0).GetChild(0).localScale = new Vector3(ui.thumbnailSize[(int)recentInts[i]].x / 55f, ui.thumbnailSize[(int)recentInts[i]].y / 55f, 1f);
            }
        }
    }

    void leftClicked(){
        if (!tileCursor.activeSelf || isMouseOverUI()){
            return;
        }
        bool readyToPlace = true;
        if(placeMode == mode.fill){
            fill();
        } else if (placeSelected == mode.spawn){
            //Checks to see if the placement location is valid
            for (int x = -1; x <= 1; x++){
                for (int y = 0; y <= 2; y++){
                    try {
                        if (currentLvl.getTile(x + mousePos[0], y + mousePos[1]) != null && currentLvl.getTile(x + mousePos[0], y + mousePos[1]).type != blockType.spawn){
                            readyToPlace = false;
                        }
                    } catch {
                        readyToPlace = false;
                    }
                }
            }
            for (int x = -1; x <= 1; x++){
                try {
                    if(currentLvl.getTile(x + mousePos[0], mousePos[1] - 1).type != blockType.block && currentLvl.getTile(x + mousePos[0], mousePos[1] - 1).type != blockType.platform){
                        readyToPlace = false;
                    }
                } catch {
                    readyToPlace = false;
                }
            }
            if(!readyToPlace){
                return;
            }

            //Deletes the previous prefab
            if (currentLvl.getTile(blockType.spawn, true) != null){
                GameObject.Destroy(placedBlocks[
                    currentLvl.getTile(blockType.spawn, true).placePos.x,
                    currentLvl.getTile(blockType.spawn, true).placePos.y]);
                while(currentLvl.getTile(blockType.spawn) != null){
                    currentLvl.occupiedTiles.Remove(currentLvl.getTile(blockType.spawn).placePos);
                    currentLvl.blocks.Remove(currentLvl.getTile(blockType.spawn).placePos);
                }
            }

            //Updates the tile data
            for (int x = -1; x <= 1; x++){
                for (int y = 0; y <= 2; y++){
                    try {
                        currentLvl.occupiedTiles.Add(new coordinate2D(x + mousePos[0], y + mousePos[1]));
                        block newBlock = new block (blockType.spawn, new coordinate2D (x + mousePos[0], y + mousePos[1]), 0, 0, false);
                        currentLvl.blocks.Add(newBlock.placePos, newBlock);
                        if(x == 0 && y == 0){
                            newBlock.coreTile = true;
                        }
                    } catch {
                        return;
                    }
                }
            }

            //Creates the prefab
            GameObject newSpawn = Instantiate(otherObjects[0], new Vector3(mousePos[0], mousePos[1], 0), new Quaternion());
            newSpawn.AddComponent<spriteDropShadow>();

            //Adds the prefab to placed blocks
            placedBlocks[mousePos[0], mousePos[1]] = newSpawn;
        } else if (placeSelected == mode.button){
            //Checks to see if the placement location is valid
            for (int x = -1; x <= 1; x++){
                for (int y = 0; y <= 0; y++){
                    try {
                        if (currentLvl.getTile(x + mousePos[0], y + mousePos[1]) != null && currentLvl.getTile(x + mousePos[0], y + mousePos[1]).type != blockType.button){
                            readyToPlace = false;
                        }
                    } catch {
                        readyToPlace = false;
                    }
                }
            }
            for (int x = -1; x <= 1; x++){
                try {
                    if(currentLvl.getTile(x + mousePos[0], mousePos[1] - 1).type != blockType.block && currentLvl.getTile(x + mousePos[0], mousePos[1] - 1).type != blockType.platform){
                        readyToPlace = false;
                    }
                } catch {
                    readyToPlace = false;
                }
            }
            if(!readyToPlace){
                return;
            }

            //Deletes the previous prefab
            if (currentLvl.getTile(blockType.button, true) != null){
                GameObject.Destroy(placedBlocks[
                    currentLvl.getTile(blockType.button, true).placePos.x,
                    currentLvl.getTile(blockType.button, true).placePos.y]);
                while(currentLvl.getTile(blockType.button) != null){
                    currentLvl.occupiedTiles.Remove(currentLvl.getTile(blockType.button).placePos);
                    currentLvl.blocks.Remove(currentLvl.getTile(blockType.button).placePos);
                }
            }

            //Updates the tile data
            for (int x = -1; x <= 1; x++){
                for (int y = 0; y <= 0; y++){
                    try {
                        currentLvl.occupiedTiles.Add(new coordinate2D(x + mousePos[0], y + mousePos[1]));
                        block newBlock = new block (blockType.button, new coordinate2D (x + mousePos[0], y + mousePos[1]), 0, 0, false);
                        currentLvl.blocks.Add(newBlock.placePos, newBlock);
                        if(x == 0 && y == 0){
                            newBlock.coreTile = true;
                        }
                    } catch {
                        return;
                    }
                }
            }

            //Creates the prefab
            GameObject newButton = Instantiate(otherObjects[1], new Vector3(mousePos[0], mousePos[1], 0), new Quaternion());
            newButton.AddComponent<spriteDropShadow>();

            //Adds the prefab to placed blocks
            placedBlocks[mousePos[0], mousePos[1]] = newButton;
        } else if (placeSelected == mode.door){
            //Checks to see if the placement location is valid
            for (int x = -1; x <= 1; x++){
                for (int y = 0; y <= 3; y++){
                    try {
                        if (currentLvl.getTile(x + mousePos[0], y + mousePos[1]) != null && currentLvl.getTile(x + mousePos[0], y + mousePos[1]).type != blockType.door){
                            readyToPlace = false;
                        }
                    } catch {
                        readyToPlace = false;
                    }
                }
            }
            for (int x = -1; x <= 1; x++){
                try {
                    if(currentLvl.getTile(x + mousePos[0], mousePos[1] - 1).type != blockType.block && currentLvl.getTile(x + mousePos[0], mousePos[1] - 1).type != blockType.platform){
                        readyToPlace = false;
                    }
                } catch {
                    readyToPlace = false;
                }
            }
            if(!readyToPlace){
                return;
            }

            //Deletes the previous prefab
            if (currentLvl.getTile(blockType.door, true) != null){
                GameObject.Destroy(placedBlocks[
                    currentLvl.getTile(blockType.door, true).placePos.x,
                    currentLvl.getTile(blockType.door, true).placePos.y]);
                while(currentLvl.getTile(blockType.door) != null){
                    currentLvl.occupiedTiles.Remove(currentLvl.getTile(blockType.door).placePos);
                    currentLvl.blocks.Remove(currentLvl.getTile(blockType.door).placePos);
                }
            }

            //Updates the tile data
            for (int x = -1; x <= 1; x++){
                for (int y = 0; y <= 3; y++){
                    try {
                        currentLvl.occupiedTiles.Add(new coordinate2D(x + mousePos[0], y + mousePos[1]));
                        block newBlock = new block (blockType.door, new coordinate2D (x + mousePos[0], y + mousePos[1]), 0, 0, false);
                        currentLvl.blocks.Add(newBlock.placePos, newBlock);
                        if(x == 0 && y == 0){
                            newBlock.coreTile = true;
                        }
                    } catch (Exception e){
                        Debug.LogError(e);
                        return;
                    }
                }
            }


            //Creates the prefab
            GameObject newDoor = Instantiate(otherObjects[2], new Vector3(mousePos[0], mousePos[1], 0), new Quaternion());
            newDoor.AddComponent<spriteDropShadow>().children.Add(1);

            //Adds the prefab to placed blocks
            placedBlocks[mousePos[0], mousePos[1]] = newDoor;
        } else if (placeSelected == mode.twoStateButton){
            //Checks to see if the placement location is valid
            for (int y = 0; y <= 1; y++){
                try {
                    if (currentLvl.checkTile(mousePos[0], y + mousePos[1], mode.twoStateButton.ToString())){
                        readyToPlace = false;
                    }
                } catch {
                    readyToPlace = false;
                }
            }
            if(!readyToPlace){
                return;
            }

            //Updates the tile data
            for (int y = 0; y <= 1; y++)
            {
                try
                {
                    if(currentLvl.checkTile(mousePos[0], y + mousePos[1])){
                        erase(mousePos[0], y + mousePos[1]);
                    }
                    currentLvl.occupiedTiles.Add(new coordinate2D(mousePos[0], y + mousePos[1]));
                    block newBlock = new block(blockType.twoStateButton, new coordinate2D(mousePos[0], y + mousePos[1]), 0, 0, false);
                    currentLvl.blocks.Add(newBlock.placePos, newBlock);
                    if (y == 0)
                    {
                        newBlock.coreTile = true;
                    }
                }
                catch
                {
                    return;
                }
            }

            //Creates the prefab
            GameObject newObj = Instantiate(otherObjects[3], new Vector3(mousePos[0], mousePos[1], 0), new Quaternion());
            newObj.AddComponent<spriteDropShadow>();

            //Adds the prefab to placed blocks
            placedBlocks[mousePos[0], mousePos[1]] = newObj;

            reloadBlocks();
        } else if (placeSelected == mode.twoStateLever){
            //Checks to see if the placement location is valid
            for (int y = 0; y <= 1; y++){
                try {
                    if (currentLvl.checkTile(mousePos[0], y + mousePos[1], mode.twoStateLever.ToString())){
                        readyToPlace = false;
                    }
                } catch {
                    readyToPlace = false;
                }
            }
            if(!readyToPlace){
                return;
            }

            //Updates the tile data
            for (int y = 0; y <= 1; y++)
            {
                try
                {
                    if(currentLvl.checkTile(mousePos[0], y + mousePos[1])){
                        erase(mousePos[0], y + mousePos[1]);
                    }
                    currentLvl.occupiedTiles.Add(new coordinate2D(mousePos[0], y + mousePos[1]));
                    block newBlock = new block(blockType.twoStateLever, new coordinate2D(mousePos[0], y + mousePos[1]), 0, 0, false);
                    currentLvl.blocks.Add(newBlock.placePos, newBlock);
                    if (y == 0)
                    {
                        newBlock.coreTile = true;
                    }
                }
                catch
                {
                    return;
                }
            }

            //Creates the prefab
            GameObject newObj = Instantiate(otherObjects[4], new Vector3(mousePos[0], mousePos[1], 0), new Quaternion());
            newObj.AddComponent<spriteDropShadow>();

            //Adds the prefab to placed blocks
            placedBlocks[mousePos[0], mousePos[1]] = newObj;

            reloadBlocks();
        }
    }

    public void Quit () {
        camScript.moveCam.Disable();
        camScript.zooom.Disable();
        camScript.mousePos.Disable();
        camScript.rightClick.Disable();
        camScript.mouseMove.Disable();

        leftClick.Disable();
        esc.Disable();
        GameObject.Find("UIController").GetComponent<editorUI>().esc.Disable();
        
        Time.timeScale = 1;
        StartCoroutine(library());
    }

    public System.Collections.IEnumerator library(){
        GameObject.FindGameObjectWithTag("transitions").GetComponent<transitions>().playTrans(false, 0);
        yield return new WaitUntil(() => GameObject.FindGameObjectWithTag("transitions").GetComponent<transitions>().anims[0].GetCurrentAnimatorStateInfo(0).normalizedTime < 1);
        yield return new WaitUntil(() => GameObject.FindGameObjectWithTag("transitions").GetComponent<transitions>().anims[0].GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        SceneManager.LoadScene("Level Library");
    }

    void fill(){
        if(placeMode == mode.single){
            return;
        }
        if(EventSystem.current.currentSelectedGameObject != null ||
            mousePos[0] > size[0] ||
            mousePos[0] < 0 ||
            mousePos[1] > size[1] ||
            mousePos[1] < 0){

            foreach (Transform child in borderDaddy)
            {
                Destroy(child.gameObject);
            }
            fillX = -1;
            fillY = -1;
            return;
        }
        for (int x = Mathf.Min(fillX, mousePos[0]); x <= Mathf.Max(fillX, mousePos[0]); x++){
            for (int y = Mathf.Min(fillY, mousePos[1]); y <= Mathf.Max(fillY, mousePos[1]); y++){
                if(toolSelected != tool.erase){
                    Place(x, y, true);
                } else {
                    erase(x, y);
                }
            }
        }
        reloadBlocks();

        foreach (Transform child in borderDaddy)
        {
            Destroy(child.gameObject);
        }
        fillX = -1;
        fillY = -1;
    }

    
    public void OnFileUpload(string url) {
        StartCoroutine(OutputRoutine(url));
    }

    bool placingOne(){
        if(placeSelected == mode.block ||
            placeSelected == mode.platform ||
            placeSelected == mode.spike ||
            placeSelected == mode.tempPlat ||
            placeSelected == mode.twoStateBlock ||
            placeSelected == mode.twoStateBlock ||
            placeSelected == mode.twoStateSpike ||
            placeSelected == mode.twoStateSpike){
                return true;
        } else {
            return false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        mousePos[0] = Mathf.Clamp(Mathf.RoundToInt(camScript.cam.GetComponent<Camera>().ScreenToWorldPoint(camScript.mousePos.ReadValue<Vector2>()).x), 0, size[0] - 1);
        mousePos[1] = Mathf.Clamp(Mathf.RoundToInt(camScript.cam.GetComponent<Camera>().ScreenToWorldPoint(camScript.mousePos.ReadValue<Vector2>()).y), 0, size[1] - 1);

        tileCursor.transform.position = new Vector3(mousePos[0], mousePos[1], 0);
        if(Mathf.RoundToInt(camScript.cam.GetComponent<Camera>().ScreenToWorldPoint(camScript.mousePos.ReadValue<Vector2>()).x) < 0 ||
        Mathf.RoundToInt(camScript.cam.GetComponent<Camera>().ScreenToWorldPoint(camScript.mousePos.ReadValue<Vector2>()).x) > size[0] - 1 ||
        Mathf.RoundToInt(camScript.cam.GetComponent<Camera>().ScreenToWorldPoint(camScript.mousePos.ReadValue<Vector2>()).y) < 0 ||
        Mathf.RoundToInt(camScript.cam.GetComponent<Camera>().ScreenToWorldPoint(camScript.mousePos.ReadValue<Vector2>()).y) > size[1] - 1){
            tileCursor.gameObject.SetActive(false);
        } else {
            tileCursor.gameObject.SetActive(true);

            if(placingOne() || toolSelected == tool.erase){
                tileCursor.transform.GetChild(0).gameObject.SetActive(true);
                tileCursor.transform.GetChild(1).gameObject.SetActive(false);
                tileCursor.transform.GetChild(2).gameObject.SetActive(false);
                tileCursor.transform.GetChild(3).gameObject.SetActive(false);
                tileCursor.transform.GetChild(4).gameObject.SetActive(false);
                tileCursor.transform.GetChild(5).gameObject.SetActive(false);
            }else if (placeSelected == mode.spawn) {
                tileCursor.transform.GetChild(0).gameObject.SetActive(false);
                tileCursor.transform.GetChild(1).gameObject.SetActive(true);
                tileCursor.transform.GetChild(2).gameObject.SetActive(false);
                tileCursor.transform.GetChild(3).gameObject.SetActive(false);
                tileCursor.transform.GetChild(4).gameObject.SetActive(true);
                tileCursor.transform.GetChild(5).gameObject.SetActive(false);
            }else if (placeSelected == mode.button) {
                tileCursor.transform.GetChild(0).gameObject.SetActive(false);
                tileCursor.transform.GetChild(1).gameObject.SetActive(false);
                tileCursor.transform.GetChild(2).gameObject.SetActive(true);
                tileCursor.transform.GetChild(3).gameObject.SetActive(false);
                tileCursor.transform.GetChild(4).gameObject.SetActive(true);
                tileCursor.transform.GetChild(5).gameObject.SetActive(false);
            }else if (placeSelected == mode.door) {
                tileCursor.transform.GetChild(0).gameObject.SetActive(false);
                tileCursor.transform.GetChild(1).gameObject.SetActive(false);
                tileCursor.transform.GetChild(2).gameObject.SetActive(false);
                tileCursor.transform.GetChild(3).gameObject.SetActive(true);
                tileCursor.transform.GetChild(4).gameObject.SetActive(true);
                tileCursor.transform.GetChild(5).gameObject.SetActive(false);
            }else if (placeSelected == mode.twoStateButton || placeSelected == mode.twoStateLever) {
                tileCursor.transform.GetChild(0).gameObject.SetActive(false);
                tileCursor.transform.GetChild(1).gameObject.SetActive(false);
                tileCursor.transform.GetChild(2).gameObject.SetActive(false);
                tileCursor.transform.GetChild(3).gameObject.SetActive(false);
                tileCursor.transform.GetChild(4).gameObject.SetActive(false);
                tileCursor.transform.GetChild(5).gameObject.SetActive(true);
            }

            if(toolSelected != tool.erase){
                tileCursor.GetComponent<SpriteRenderer>().sprite = cursorPics[1];
                tileCursor.GetComponent<SpriteRenderer>().color = cursorColors[1];
            } else {
                tileCursor.GetComponent<SpriteRenderer>().sprite = cursorPics[0];
                tileCursor.GetComponent<SpriteRenderer>().color = cursorColors[0];
            }

            if(currentLvl.checkTile(mousePos[0], mousePos[1]) && placingOne() || !currentLvl.checkTile(mousePos[0], mousePos[1]) && toolSelected == tool.erase){
                tileCursor.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().color = cursorColors[0];
            } else if(placingOne() || toolSelected == tool.erase){
                tileCursor.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().color = cursorColors[1];
            } else if(new List<mode> {mode.spawn, mode.button, mode.door, mode.twoStateButton, mode.twoStateLever}.Contains(placeSelected)){
                //Correctly colors the borders for placing spawn, button, or door
                for (int x = -1; x <= 1; x++){
                    try {
                        if (currentLvl.getTile(x + mousePos[0], -1 + mousePos[1]).type == blockType.block || currentLvl.getTile(x + mousePos[0], -1 + mousePos[1]).type == blockType.platform){
                            tileCursor.transform.GetChild(4).GetChild(x + 1).gameObject.GetComponent<SpriteRenderer>().color = cursorColors[1];
                        } else {
                            tileCursor.transform.GetChild(4).GetChild(x + 1).gameObject.GetComponent<SpriteRenderer>().color = cursorColors[0];
                        }
                    } catch {
                        tileCursor.transform.GetChild(4).GetChild(x + 1).gameObject.GetComponent<SpriteRenderer>().color = cursorColors[0];
                    }
                }

                if(placeSelected == mode.spawn){
                    for (int x = -1; x <= 1; x++){
                        for (int y = 0; y <= 2; y++){
                            try {
                                if (!currentLvl.checkTile(x + mousePos[0], y + mousePos[1]) || currentLvl.getTile(x + mousePos[0], y + mousePos[1]).type == blockType.spawn){
                                    tileCursor.transform.GetChild(1).GetChild(y + 3 * x + 3).gameObject.GetComponent<SpriteRenderer>().color = cursorColors[1];
                                } else {
                                    tileCursor.transform.GetChild(1).GetChild(y + 3 * x + 3).gameObject.GetComponent<SpriteRenderer>().color = cursorColors[0];
                                }
                            } catch {
                                tileCursor.transform.GetChild(1).GetChild(y + 3 * x + 3).gameObject.GetComponent<SpriteRenderer>().color = cursorColors[0];
                            }
                        }
                    }
                } else if(placeSelected == mode.button){
                    for (int x = -1; x <= 1; x++){
                        for (int y = 0; y <= 0; y++){
                            try {
                                if (!currentLvl.checkTile(x + mousePos[0], y + mousePos[1]) || currentLvl.getTile(x + mousePos[0], y + mousePos[1]).type == blockType.button){
                                    tileCursor.transform.GetChild(2).GetChild(y + 1 * x + 1).gameObject.GetComponent<SpriteRenderer>().color = cursorColors[1];
                                } else {
                                    tileCursor.transform.GetChild(2).GetChild(y + 1 * x + 1).gameObject.GetComponent<SpriteRenderer>().color = cursorColors[0];
                                }
                            } catch {
                                tileCursor.transform.GetChild(2).GetChild(y + 1 * x + 1).gameObject.GetComponent<SpriteRenderer>().color = cursorColors[0];
                            }
                        }
                    }
                } else if(placeSelected == mode.door){
                    for (int x = -1; x <= 1; x++){
                        for (int y = 0; y <= 3; y++){
                            try {
                                if (!currentLvl.checkTile(x + mousePos[0], y + mousePos[1]) || currentLvl.getTile(x + mousePos[0], y + mousePos[1]).type == blockType.door){
                                    tileCursor.transform.GetChild(3).GetChild(y + 4 * x + 4).gameObject.GetComponent<SpriteRenderer>().color = cursorColors[1];
                                } else {
                                    tileCursor.transform.GetChild(3).GetChild(y + 4 * x + 4).gameObject.GetComponent<SpriteRenderer>().color = cursorColors[0];
                                }
                            } catch {
                                tileCursor.transform.GetChild(3).GetChild(y + 4 * x + 4).gameObject.GetComponent<SpriteRenderer>().color = cursorColors[0];
                            }
                        }
                    }
                } else if (placeSelected == mode.twoStateButton || placeSelected == mode.twoStateLever){
                    for (int y = 0; y <= 1; y++){
                        try {
                            if (!currentLvl.checkTile(mousePos[0], y + mousePos[1])){
                                tileCursor.transform.GetChild(5).GetChild(y).gameObject.GetComponent<SpriteRenderer>().color = cursorColors[1];
                            } else {
                                if(currentLvl.checkTile(mousePos[0], y + mousePos[1], ((mode)placeSelected).ToString())){
                                    tileCursor.transform.GetChild(5).GetChild(y).gameObject.GetComponent<SpriteRenderer>().color = cursorColors[0];
                                } else {
                                    tileCursor.transform.GetChild(5).GetChild(y).gameObject.GetComponent<SpriteRenderer>().color = cursorColors[1]; 
                                }
                            }
                        } catch {
                           tileCursor.transform.GetChild(5).GetChild(y).gameObject.GetComponent<SpriteRenderer>().color = cursorColors[0];
                        }
                    }
                }
            }
        }

        if(leftClick.IsPressed() && !isMouseOverUI()){
            if (placeMode == mode.single){
                if(placingOne() && tileCursor.activeSelf){
                    Place(mousePos[0], mousePos[1], true);
                    reloadBlocks();
                } else if(toolSelected == tool.erase){
                    erase(mousePos[0], mousePos[1]);
                    reloadBlocks();
                }
            } else if (placeMode == mode.fill){
                if(fillX < 0){
                    fillX = mousePos[0];
                    fillY = mousePos[1];
                }

                foreach (Transform child in borderDaddy)
                {
                    Destroy(child.gameObject);
                }
                for (int x = Mathf.Min(fillX, mousePos[0]); x <= Mathf.Max(fillX, mousePos[0]); x++){
                    for (int y = Mathf.Min(fillY, mousePos[1]); y <= Mathf.Max(fillY, mousePos[1]); y++){
                        GameObject newBorder = Instantiate(borderPrefab, new Vector3(x, y, 0), new Quaternion(), borderDaddy);
                        if(currentLvl.checkTile(x, y) && toolSelected != tool.erase || !currentLvl.checkTile(x, y) && toolSelected == tool.erase){
                            newBorder.GetComponent<SpriteRenderer>().color = cursorColors[0];
                        } else {
                            newBorder.GetComponent<SpriteRenderer>().color = cursorColors[1];
                        }
                    }
                }
            }
        }
    }  

    //Sets the size of level based on input fields
    public void setSize(){
        checkField(yField);
        checkField(xField);
        if(int.Parse(xField.text) > 999 || int.Parse(yField.text) > 999){
            return;
        }
        size[0] = int.Parse(xField.text);
        size[1] = int.Parse(yField.text);
        currentLvl.size = size;

        camScript.bounds[0] = size[1];
        camScript.bounds[1] = size[0];
        camScript.bounds[2] = 0;
        camScript.bounds[3] = 0;

        placedBlocksClone = placedBlocks;
        placedBlocks = new GameObject[size[0], size[1]];
        for(int x = 0; x < Mathf.Min(placedBlocks.GetLength(0), placedBlocksClone.GetLength(0)); x++){
            for(int y = 0; y < Mathf.Min(placedBlocks.GetLength(1), placedBlocksClone.GetLength(1)); y++){
                placedBlocks[x, y] = placedBlocksClone[x, y];
            }
        }

        List<coordinate2D> outOfBounds = new List<coordinate2D>();
        foreach(KeyValuePair<coordinate2D, block> block in currentLvl.blocks){
            if(block.Value.placePos.x >= size[0] || block.Value.placePos.y >= size[1]){
                outOfBounds.Add(block.Key);
            }
        }
        for(int i = 0; i < outOfBounds.Count; i++){
            currentLvl.occupiedTiles.Remove(outOfBounds[i]);
            currentLvl.blocks.Remove(outOfBounds[i]);
        }

        for(int x = 0; x < placedBlocksClone.GetLength(0); x++){
            for(int y = 0; y < placedBlocksClone.GetLength(1); y++){
                try{
                    if(placedBlocksClone[x, y] != placedBlocks[x, y]){
                        GameObject.Destroy(placedBlocksClone[x, y]);
                        //currentLvl.occupiedTiles.Remove(new coordinate2D (x, y));
                        //currentLvl.blocks.Remove(currentLvl.getTile(x, y));
                    }
                } catch {
                    GameObject.Destroy(placedBlocksClone[x, y]);
                }
            }
        }

        gridReload();
    }

    System.Collections.IEnumerator flashButton(GameObject button){
        button.GetComponent<Image>().material = redMat;
        yield return new WaitForSecondsRealtime(0.25f);
        button.GetComponent<Image>().material = normMat;
    }

    bool isMouseOverUI(){
        if(EventSystem.current != null){
            return EventSystem.current.IsPointerOverGameObject();
        } else return false;
    }

    //Exports the custom level to a local file
    public void Save(TMP_Text log){
        bool spawnPlaced = false;
        spawnPlaced = false;
        bool buttonPlaced = false;
        buttonPlaced = false;
        bool doorPlaced = false;
        doorPlaced = false;
        if (currentLvl.getTile(blockType.spawn) != null){
            spawnPlaced = true;
        } if (currentLvl.getTile(blockType.button) != null){
            buttonPlaced = true;
        } if (currentLvl.getTile(blockType.door) != null){
            doorPlaced = true;
        }
        log.text = "";
        if(!spawnPlaced || !buttonPlaced || !doorPlaced){
            if(currentLvl.tags.Count == 0){
               currentLvl.tags.Add("unplayable"); 
            } else if(!currentLvl.tags.Contains("unplayable")){
                currentLvl.tags.Add("unplayable");
            }
        } else {
            if(currentLvl.tags.Contains("unplayable")){
                currentLvl.tags.Remove("unplayable");
            }
        }

        string saveName = "Custom Level";
        if(levelName.text.Length > 0){
            saveName = levelName.text;
            currentLvl.title = levelName.text;
        }

        //Common.DownloadFileHelper.DownloadToFile(content, saveName);
        string path = Application.persistentDataPath + "/Custom Levels/" + currentLvl.levelID + ".goose";
        
        
        currentLvl.size = size;
        StartCoroutine(finishLoad(path));

        makerProfile profile = JsonConvert.DeserializeObject<makerProfile>(GameObject.FindGameObjectWithTag("gameManager").GetComponent<gameManager>().decrypt(File.ReadAllText(Application.persistentDataPath + "/Custom Levels/" + "makerProfile.json")));
        profile.clearedLvls[currentLvl.levelID] = false;
        File.WriteAllText(Application.persistentDataPath + "/Custom Levels/" + "makerProfile.json", GameObject.FindGameObjectWithTag("gameManager").GetComponent<gameManager>().encrypt(JsonConvert.SerializeObject(profile, Formatting.Indented)), System.Text.Encoding.UTF8);
    }

    IEnumerator finishLoad(string path){
        Camera.allCameras[1].transform.position = Camera.main.transform.position;
        Camera.allCameras[1].orthographicSize = Camera.main.orthographicSize;
        yield return new WaitForEndOfFrame();

        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
        Camera.allCameras[1].targetTexture = rt;
        Texture2D newTex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        Camera.allCameras[1].Render();
        RenderTexture.active = rt;
        newTex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        Camera.allCameras[1].targetTexture = null;
        RenderTexture.active = Camera.main.activeTexture;
        Destroy(rt);
        currentLvl.thumbnail = newTex.EncodeToJPG();
        newTex.LoadImage(currentLvl.thumbnail);
        snapshot.transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, Screen.height);
        snapshot.sprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), new Vector2 (0.5f, 0.5f));
        snapshot.transform.parent.GetComponent<Animator>().SetTrigger("snap");

        File.WriteAllText(path, gm.encrypt(JsonConvert.SerializeObject(currentLvl, Formatting.Indented, new JsonSerializerSettings()
                        { 
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        })),
                        Encoding.UTF8);
    }

    public void setFullscreen(Toggle fullscreen){
        if(fullscreen.isOn){
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        } else {
            Screen.fullScreenMode = FullScreenMode.Windowed;
        }
    }

    //Checks field for non-numbers and numbers out of bounds
    public void checkField(TMP_InputField field){
        field.text = Regex.Replace(field.text, @"[^0-9 ]", "");
    }

    public void checkMinMax(TMP_InputField field){
        //Checks minimum
        try {
            if(int.Parse(field.text) < minSize){
                field.text = "6";
            }
        } catch {
            field.text = "6";
        }

        //Checks Maximum
        if(int.Parse(field.text) > maxSizeNum){
            field.text = "256";
        }
    }

    //Places a block
    void Place(int x, int y, bool playerPlaced){
        if(x < 0 || y < 0){
            return;
        }
        if(currentLvl.checkTile(x, y, ((mode)placeSelected).ToString()) && playerPlaced)
        {
            return;
        }
        if(currentLvl.checkTile(x, y) && playerPlaced){
            erase(x, y);
        }

        coordinate2D newCoord = new coordinate2D (x, y);
        if(!currentLvl.occupiedTiles.Contains(newCoord)){
            currentLvl.occupiedTiles.Add(newCoord);
        }
        
        GameObject newBlock = Instantiate(tilePrefab, new Vector3 (x, y, 0), new Quaternion());
        newBlock.AddComponent<spriteDropShadow>();

        if (placeSelected == (int)mode.block)
        {
            placeBlock(x, y, newBlock, playerPlaced);
        }
        else if (placeSelected == mode.platform)
        {
            placePlatform(x, y, newBlock, playerPlaced);
        }
        else if (placeSelected == mode.spike)
        {
            placeDeath(x, y, newBlock, playerPlaced);
        }
        else if (placeSelected == mode.tempPlat)
        {
            placeTempPlat(x, y, newBlock, playerPlaced);
        }
        else if (placeSelected == mode.blueBlock)
        {
            placeTwoStateBlue(x, y, newBlock, playerPlaced);
        }
        else if (placeSelected == mode.redBlock)
        {
            placeTwoStateRed(x, y, newBlock, playerPlaced);
        }
        else if (placeSelected == mode.blueSpike)
        {
            placeBlueDeath(x, y, newBlock, playerPlaced);
        }
        else if (placeSelected == mode.redSpike)
        {
            placeRedDeath(x, y, newBlock, playerPlaced);
        }

        newBlock.GetComponent<SpriteRenderer>().sortingOrder = -1;
        newBlock.GetComponent<SpriteRenderer>().color = Color.white;
        newBlock.gameObject.GetComponent<SpriteRenderer>().material = litMat;
        newBlock.transform.localScale = new Vector3 (1, 1, 1);
        placedBlocks[x, y] = newBlock;
    }

    void placeBlock(int x, int y, GameObject newBlock, bool playerPlaced){
        int num = Mathf.RoundToInt((((blocks.Length - 1) / 2) * Mathf.Sin((seed * 928.359f / 69385) * (seed * (x + 258) * (y + 2)))) + (blocks.Length - 1) / 2);

        coordinate2D newCoord = new coordinate2D (x, y);
        if(playerPlaced){
            currentLvl.blocks.Add(newCoord, new block (blockType.block, newCoord, 0, num, false));
            
        }

        if(!currentLvl.blocks[newCoord].tags.ContainsKey("replaceBy")){
            currentLvl.blocks[newCoord].tags.Add("replaceBy", "twoStateButton,twoStateLever");
        }

        newBlock.GetComponent<SpriteRenderer>().sprite = blocks[num];
        newBlock.name = "Placed Block";
    }

    void placePlatform(int x, int y, GameObject newPlatform, bool playerPlaced){
        int n = Math.Abs((Mathf.RoundToInt(x / (float)Math.PI) * Mathf.RoundToInt(seed / 17)) + (Mathf.RoundToInt(y * (float)Math.PI) * Mathf.RoundToInt(seed / 19))) + x * y;
        int index = Mathf.RoundToInt(n / 10) % 2;

        coordinate2D newCoord = new coordinate2D (x, y);
        if(playerPlaced){
            currentLvl.blocks.Add(newCoord, new block (blockType.platform, newCoord, 0, index, false));
        }
        newPlatform.GetComponent<SpriteRenderer>().sprite = oneWays.Middle[index];
        newPlatform.name = "Placed Platform";
    }

    void placeDeath(int x, int y, GameObject newObstacle, bool playerPlaced){
        coordinate2D newCoord = new coordinate2D (x, y);
        if(playerPlaced){
            currentLvl.blocks.Add(newCoord, new block (blockType.spike, newCoord, 0, 0, false));
        }

        newObstacle.GetComponent<SpriteRenderer>().sprite = this.GetComponent<w1Spikes>().general;
        newObstacle.name = "Placed Obstacle";
    }

    void placeTempPlat(int x, int y, GameObject newObj, bool playerPlaced){
        int n = Math.Abs((Mathf.RoundToInt(x / (float)Math.PI) * Mathf.RoundToInt(seed / 17)) + (Mathf.RoundToInt(y * (float)Math.PI) * Mathf.RoundToInt(seed / 19))) + x * y;
        int index = Mathf.RoundToInt(n / 10) % 3;
        
        coordinate2D newCoord = new coordinate2D (x, y);
        if(playerPlaced){
            currentLvl.blocks.Add(newCoord, new block (blockType.tempPlat, newCoord, 0, index, false));
        }

        newObj.GetComponent<SpriteRenderer>().sprite = this.GetComponent<extraSprites>().tempPlat[index];
        newObj.name = "Placed Obstacle";
    }

    void placeTwoStateBlue(int x, int y, GameObject newObj, bool playerPlaced){
        coordinate2D newCoord = new coordinate2D (x, y);
        if(playerPlaced){
            currentLvl.blocks.Add(newCoord, new block (blockType.blueBlock, newCoord, 0, 0, false));
        }

        newObj.GetComponent<SpriteRenderer>().sprite = this.GetComponent<extraSprites>().blockBlue;
        newObj.name = "Placed Obstacle";
    }

    void placeTwoStateRed(int x, int y, GameObject newObj, bool playerPlaced){
        coordinate2D newCoord = new coordinate2D (x, y);
        if(playerPlaced){
            currentLvl.blocks.Add(newCoord, new block (blockType.redBlock, newCoord, 0, 0, false));
        }

        newObj.GetComponent<SpriteRenderer>().sprite = this.GetComponent<extraSprites>().blockRed;
        newObj.name = "Placed Obstacle";
    }

    void placeRedDeath(int x, int y, GameObject newObstacle, bool playerPlaced){
        coordinate2D newCoord = new coordinate2D (x, y);
        if(playerPlaced){
            currentLvl.blocks.Add(newCoord, new block (blockType.redSpike, newCoord, 0, 0, false));
        }

        newObstacle.GetComponent<SpriteRenderer>().sprite = this.GetComponent<extraSprites>().spikeRed;
        newObstacle.name = "Placed Obstacle";
    }

    void placeBlueDeath(int x, int y, GameObject newObstacle, bool playerPlaced){
        coordinate2D newCoord = new coordinate2D (x, y);
        if(playerPlaced){
            currentLvl.blocks.Add(newCoord, new block (blockType.blueSpike, newCoord, 0, 0, false));
        }

        newObstacle.GetComponent<SpriteRenderer>().sprite = this.GetComponent<extraSprites>().spikeBlue;
        newObstacle.name = "Placed Obstacle";
    }

    void erase(int x, int y){ 
        if (currentLvl.getTile(x, y) == null || !tileCursor.activeSelf || x < 0 || y < 0){
            return;
        }


        //Deletes special blocks
        if(currentLvl.getTile(x, y).type == blockType.spawn || currentLvl.getTile(x, y).type == blockType.button || currentLvl.getTile(x, y).type == blockType.door){
            blockType typeToDel = currentLvl.getTile(x, y ).type;
            GameObject.Destroy(placedBlocks[
                currentLvl.getTile(currentLvl.getTile(x, y).type, true).placePos.x,
                currentLvl.getTile(currentLvl.getTile(x, y).type, true).placePos.y]);
            while(currentLvl.getTile(typeToDel) != null){
                block specTileToDelete = currentLvl.getTile(typeToDel);
                currentLvl.occupiedTiles.Remove(specTileToDelete.placePos);
                currentLvl.blocks.Remove(specTileToDelete.placePos);
            }
            return;
        }
        if(currentLvl.getTile(x, y).type == blockType.twoStateButton || currentLvl.getTile(x, y).type == blockType.twoStateLever){
            if(currentLvl.getTile(x, y).coreTile){
                GameObject.Destroy(placedBlocks[x, y]);
                currentLvl.occupiedTiles.Remove(currentLvl.getTile(x, y).placePos);
                currentLvl.blocks.Remove(currentLvl.getTile(x, y).placePos);
                currentLvl.occupiedTiles.Remove(currentLvl.getTile(x, y + 1).placePos);
                currentLvl.blocks.Remove(currentLvl.getTile(x, y + 1).placePos);
            } else {
                GameObject.Destroy(placedBlocks[x, y - 1]);
                currentLvl.occupiedTiles.Remove(currentLvl.getTile(x, y - 1).placePos);
                currentLvl.blocks.Remove(currentLvl.getTile(x, y - 1).placePos);
                currentLvl.occupiedTiles.Remove(currentLvl.getTile(x, y).placePos);
                currentLvl.blocks.Remove(currentLvl.getTile(x, y).placePos);
            }
        }

        //Deletes 1x1 blocks
        GameObject.Destroy(placedBlocks[x, y]);
        coordinate2D delCoord = new coordinate2D (x, y);
        currentLvl.occupiedTiles.Remove(delCoord);
        currentLvl.blocks.Remove(delCoord);

        if(currentLvl.getTile(x, y + 1) != null){
            if(currentLvl.getTile(x, y + 1).type == blockType.spawn || currentLvl.getTile(x, y + 1).type == blockType.button || currentLvl.getTile(x, y + 1).type == blockType.door){
                erase(x, y + 1);
            }
        }
    }

    public void setSeed(){
        checkField(seedField);
        while(int.Parse(seedField.text) < 100000){
            seedField.text = (int.Parse(seedField.text) * 10).ToString();
        }
        reloadBlocks();
    }

    public void randomizeSeed(){
        seed = UnityEngine.Random.Range(100000, 999999);
        seedField.text = seed.ToString();
        reloadBlocks();
    }

    //Open file select prompt and loads the level in that file
    public void Load(TMP_Text log){
        //Gets the path to the level
        /*var extensions = new [] {
            new ExtensionFilter("levels", "txt", "goose"),
            new ExtensionFilter("All Files", "*" ),
        };
        string path = "";
        log.color = cursorColors[0];
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
            log.text = e.ToString();
            return;
        }
        if (path == null || path == ""){
            log.text = "no path selected";
            return;
        }
        log.color = cursorColors[1];
        log.text = "loading " + path;*/

        //Converts custom level to string
        currentLvl = gm.currentLvl;

        //Clears all currently placed blocks
        for(int x = 0; x < size[0]; x++){
            for(int y = 0; y < size[1]; y++){
                GameObject.Destroy(placedBlocks[x, y]);
            }
        }

        levelName.text = currentLvl.title;

        //Reads and interprets the level size
        size = currentLvl.size;
        camScript.bounds[0] = size[1];
        camScript.bounds[1] = size[0];
        camScript.bounds[2] = 0;
        camScript.bounds[3] = 0;
        xField.text = size[0].ToString();
        yField.text = size[1].ToString();
        seed = currentLvl.seed;

        //Reads the tiledata and builds the level accordingly
        //Debug.Log("Size: " + size[0] + ", " + size[1]);
        placedBlocks = new GameObject [size[0], size[1]];

        loadingLevel = true;
        foreach(KeyValuePair<coordinate2D, block> block in currentLvl.blocks){
            if(block.Value.tags == null){
                block.Value.tags = new Dictionary<string, string>();
            }
            placeSelected = mode.block;
            if (block.Value.type == blockType.block)
            {
                Place(block.Value.placePos.x, block.Value.placePos.y, false);
            }
            else
            if (block.Value.type == blockType.spawn && block.Value.coreTile)
            {
                placedBlocks[block.Value.placePos.x, block.Value.placePos.y] = Instantiate(otherObjects[0], new Vector3(block.Value.placePos.x, block.Value.placePos.y, 0), new Quaternion());
                placedBlocks[block.Value.placePos.x, block.Value.placePos.y].AddComponent<spriteDropShadow>();
            }
            else
            if (block.Value.type == blockType.button && block.Value.coreTile)
            {
                placedBlocks[block.Value.placePos.x, block.Value.placePos.y] = Instantiate(otherObjects[1], new Vector3(block.Value.placePos.x, block.Value.placePos.y, 0), new Quaternion());
                placedBlocks[block.Value.placePos.x, block.Value.placePos.y].AddComponent<spriteDropShadow>();
            }
            else
            if (block.Value.type == blockType.door && block.Value.coreTile)
            {
                placedBlocks[block.Value.placePos.x, block.Value.placePos.y] = Instantiate(otherObjects[2], new Vector3(block.Value.placePos.x, block.Value.placePos.y, 0), new Quaternion());
                placedBlocks[block.Value.placePos.x, block.Value.placePos.y].AddComponent<spriteDropShadow>().children.Add(1);
            }
            else
            if (block.Value.type == blockType.platform)
            {
                placeSelected = mode.platform;
                Place(block.Value.placePos.x, block.Value.placePos.y, false);
            }
            else
            if (block.Value.type == blockType.spike)
            {
                placeSelected = mode.spike;
                Place(block.Value.placePos.x, block.Value.placePos.y, false);
            }
            else
            if (block.Value.type == blockType.tempPlat)
            {
                placeSelected = mode.tempPlat;
                Place(block.Value.placePos.x, block.Value.placePos.y, false);
            }
            else
            if (block.Value.type == blockType.blueBlock)
            {
                placeSelected = mode.blueBlock;
                Place(block.Value.placePos.x, block.Value.placePos.y, false);
            }
            else
            if (block.Value.type == blockType.redBlock)
            {
                placeSelected = mode.redBlock;
                Place(block.Value.placePos.x, block.Value.placePos.y, false);
            }
            else
            if (block.Value.type == blockType.blueSpike)
            {
                placeSelected = mode.blueSpike;
                Place(block.Value.placePos.x, block.Value.placePos.y, false);
            }
            else
            if (block.Value.type == blockType.redSpike)
            {
                placeSelected = mode.redSpike;
                Place(block.Value.placePos.x, block.Value.placePos.y, false);
            }
            else
            if (block.Value.type == blockType.twoStateButton && block.Value.coreTile)
            {
                placedBlocks[block.Value.placePos.x, block.Value.placePos.y] = Instantiate(otherObjects[3], new Vector3(block.Value.placePos.x, block.Value.placePos.y, 0), new Quaternion());
                placedBlocks[block.Value.placePos.x, block.Value.placePos.y].AddComponent<spriteDropShadow>();
            }
            else
            if (block.Value.type == blockType.twoStateLever && block.Value.coreTile)
            {
                placedBlocks[block.Value.placePos.x, block.Value.placePos.y] = Instantiate(otherObjects[4], new Vector3(block.Value.placePos.x, block.Value.placePos.y, 0), new Quaternion());
                placedBlocks[block.Value.placePos.x, block.Value.placePos.y].AddComponent<spriteDropShadow>();
            }
        }
        loadingLevel = false;

        gridReload();
        placeSelected = mode.block;
        //log.text = "Successfully loaded " + path;
    }

    public void selectTool(blockEditor select){
        Enum.TryParse(select.name, out mode result);
        placeSelected = result;
        recentInts.Insert(0, (mode)result);
        recentRefresh();

        if (result != mode.spike)
        {
            rotate = 0;
        }

        singleFill.interactable = select.canFill;
        if (!select.canFill)
        {
            singleFill.isOn = false;
            placeMode = mode.single;
        }
        else
        {
            if (singleFill.isOn)
            {
                placeMode = mode.fill;
            }
            else
            {
                placeMode = mode.single;
            }
        }

        blueRed.interactable = select.isTwoState;
        blueRed.isOn = redSelect;
        if (result == mode.twoStateBlock)
        {
            if (redSelect)
            {
                placeSelected = mode.redBlock;
            }
            else
            {
                placeSelected = mode.blueBlock;
            }
        }
        else
        {
            if (result == mode.twoStateSpike)
            {
                if (redSelect)
                {
                    placeSelected = mode.redSpike;
                }
                else
                {
                    placeSelected = mode.blueSpike;
                }
            }
        }

    }

    public void selectTool2(Button button){
        for(int i = 0; i < buttons.Count; i++){
            buttons[i].interactable = true;
            buttons[i].gameObject.GetComponent<Image>().sprite = blockButton[0];
        }
        button.interactable = false;
        button.gameObject.GetComponent<Image>().sprite = blockButton[1];
    }

    public void drawErase(int i)
    {
        toolSelected = (tool)i;
    }

    public void selectPlaceMethod(Toggle toggle)
    {
        if (toggle.isOn)
        {
            placeMode = mode.fill;
        }
        else
        {
            placeMode = mode.single;
        }
    }

    //Reloads the grid to match dimensions
    void gridReload(){
        /*//Deletes past grid
        for(int i = 0; i < gridDaddy.childCount; i++){
            Destroy(gridDaddy.GetChild(i).gameObject);
        }

        //Creates new grid
        for(int x = 0; x < size[0]; x++){
            for (int y = 0; y < size[1]; y++){
                Vector3 tilePos = new Vector3 (x, y, 0);
                GameObject newTile = Instantiate(tilePrefab, tilePos, gridDaddy.rotation, gridDaddy);
                newTile.GetComponent<SpriteRenderer>().sortingOrder = 1;
                newTile.gameObject.layer = 5;
            }
        }*/
        


        //Creates border around level
        foreach (Transform child in blockBorderDaddy)
        {
            GameObject.Destroy(child.gameObject);
        }
            //Places all the blocks bordering the grid
            placeBorderBlock(0, new Vector3(-1, size[1], 0));
            for(int i = 0; i < size[0]; i++){
                placeBorderBlock(1, new Vector3 (i, size[1], 0));
            }

            placeBorderBlock(2, new Vector3(size[0], size[1], 0));
            for(int i = -25; i < size[1]; i++){
                placeBorderBlock(4, new Vector3 (size[0], i, 0));
            }

            for(int i = -25; i < size[1]; i++){
                placeBorderBlock(3, new Vector3 (-1, i, 0));
            }

            for(int i = 0; i < size[0]; i++){
                placeBorderBlock(8, new Vector3 (i, 0, 0));
            }

            GameObject newBackground = Instantiate(Resources.Load<GameObject>("background"), blockBorderDaddy);
            newBackground.GetComponent<SpriteRenderer>().size = new Vector2(size[0], size[1]);
            newBackground.transform.localPosition = new Vector3 (size[0] / 2 - 0.5f, (size[1] / 4f) - 0.5f, 0);

            //Places blocks outside the border
            placeBorderVoid(9, new Vector3(-10, size[1] / 2, 0), 18, size[1] + 26);
            placeBorderVoid(9, new Vector3(size[0] + 10, size[1] / 2, 0), 19, size[1] + 26);
            placeBorderVoid(9, new Vector3((size[0] - 1f) / 2f, size[1] + 10, 0), size[0] + 13, 19);
            placeBorderVoid(10, new Vector3((size[0] - 1f) / 2f, -10f, 0), size[0], 19);

            reloadBlocks();
    }

    void placeBorderBlock(int block, Vector3 newPos){
        Transform newBorderBlock = Instantiate(tilePrefab, newPos, new Quaternion(), blockBorderDaddy).transform;
        newBorderBlock.gameObject.GetComponent<SpriteRenderer>().sprite = borderBlocks[block];
        newBorderBlock.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 0;
        newBorderBlock.gameObject.GetComponent<SpriteRenderer>().color = new Color (0.65f, 0.65f, 0.65f);
        newBorderBlock.gameObject.GetComponent<SpriteRenderer>().material = litMat;
        newBorderBlock.localScale = new Vector3 (1, 1, 1);
        if(block == 8){
            //newBorderBlock.GetComponent<BoxCollider2D>().enabled = false;
            newBorderBlock.GetComponent<SpriteRenderer>().sortingOrder = 2;
        }
    }

    void placeBorderVoid(int block, Vector3 newPos, float width, float height){
        Transform newBorderBlock = Instantiate(tilePrefab, newPos, new Quaternion(), blockBorderDaddy).transform;
        newBorderBlock.gameObject.GetComponent<SpriteRenderer>().sprite = borderBlocks[block];
        newBorderBlock.localScale = new Vector3 (1 / 0.444444f, 1 / 0.444444f, 1);
        newBorderBlock.gameObject.GetComponent<SpriteRenderer>().drawMode = SpriteDrawMode.Tiled;
        newBorderBlock.gameObject.GetComponent<SpriteRenderer>().color = new Color (0.65f, 0.65f, 0.65f);
        newBorderBlock.gameObject.GetComponent<SpriteRenderer>().sortingOrder = -2;
        newBorderBlock.gameObject.GetComponent<SpriteRenderer>().material = litMat;
        newBorderBlock.gameObject.GetComponent<SpriteRenderer>().size = new Vector2(width, height);
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

    void reloadBlocks(){
        if(loadingLevel){
            return;
        }
        for(int x = 0; x < size[0]; x++){
            for(int y = 0; y < size[1]; y++){
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
                                int n = Math.Abs((Mathf.RoundToInt(x / (float)Math.PI) * Mathf.RoundToInt(seed / 17)) + (Mathf.RoundToInt(y * (float)Math.PI) * Mathf.RoundToInt(seed / 19)));
                                int index = n % w1Columns["single"].Count;
                                currentLvl.getTile(x, y).blockVer = index;
                            }
                            if(checkTileOccupancy(x + 1, y - 1) == 0 || checkTileOccupancy(x - 1, y - 1) == 0){
                                botRoot = true;
                                botFound = true;
                            }
                            if(topRoot && botRoot){
                                int n = Math.Abs((Mathf.RoundToInt(x / (float)Math.PI) * Mathf.RoundToInt(seed / 17)) + (Mathf.RoundToInt(y * (float)Math.PI) * Mathf.RoundToInt(seed / 19)));
                                int index = n % w1Columns["single"].Count;
                                currentLvl.getTile(x, y).blockVer = index;
                                placedBlocks[x, y].GetComponent<SpriteRenderer>().sprite = w1Columns["single"][currentLvl.getTile(x, y).blockVer];
                                column = true;
                            }

                            int i = 0;
                            while(!topFound && checkTileOccupancy(x, y + i) == 0){
                                i++;
                                if(checkTileOccupancy(x, y + i) == 0 && (checkTileOccupancy(x + 1, y + i) == 0 || checkTileOccupancy(x - 1, y + i) == 0)){
                                    int n = Math.Abs((Mathf.RoundToInt(x / (float)Math.PI) * Mathf.RoundToInt(seed / 17)) + (Mathf.RoundToInt((y + i - 1) * (float)Math.PI) * Mathf.RoundToInt(seed / 19)));
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
                                    Debug.LogError(new coordinate2D (x, y));
                                }
                            }
                        }
                        if(column == false){
                        currentLvl.getTile(x, y).blockVer = Mathf.RoundToInt((((blocks.Length - 1) / 2) * Mathf.Sin((seed * 928.359f / 69385) * (seed * (x + 258) * (y + 2)))) + (blocks.Length - 1) / 2);
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
                            currentLvl.getTile(x, y).blockVer = Mathf.RoundToInt((((blocks.Length - 1) / 2) * Mathf.Sin((seed * 928.359f / 69385) * (seed * (x + 258) * (y + 2)))) + (blocks.Length - 1) / 2);
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
                                currentLvl.getTile(x, y).blockVer = Mathf.RoundToInt((((blocks.Length - 1) / 2) * Mathf.Sin((seed * 928.359f / 69385) * (seed * (x + 258) * (y + 2)))) + (blocks.Length - 1) / 2);
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
                int n = Math.Abs((Mathf.RoundToInt(x / (float)Math.PI) * Mathf.RoundToInt(seed / 17)) + (Mathf.RoundToInt(y * (float)Math.PI) * Mathf.RoundToInt(seed / 19))) + x * y;
                int index = Mathf.RoundToInt(n / 10) % 2;
                currentLvl.getTile(x, y).blockVer = index;

                SpriteRenderer sr = placedBlocks[x, y].GetComponent<SpriteRenderer>();
                sr.sprite = oneWays.Middle[currentLvl.getTile(x, y).blockVer];

                if(currentLvl.getTile(x + 1, y) == null && currentLvl.getTile(x - 1, y) != null){
                    if(currentLvl.getTile(x - 1, y).type == blockType.platform){
                        sr.sprite = oneWays.endRight[currentLvl.getTile(x, y).blockVer];
                    }
                } else {
                    if(currentLvl.getTile(x - 1, y) != null){
                        if(currentLvl.getTile(x - 1, y).type == blockType.platform && (currentLvl.getTile(x + 1, y).type == blockType.button || currentLvl.getTile(x + 1, y).type == blockType.door || currentLvl.getTile(x + 1, y).type == blockType.spawn)){
                            sr.sprite = oneWays.endRight[currentLvl.getTile(x, y).blockVer];
                        }
                    }
                }
                if(currentLvl.getTile(x - 1, y) == null && currentLvl.getTile(x + 1, y) != null){
                    if(currentLvl.getTile(x + 1, y).type == blockType.platform){
                        sr.sprite = oneWays.endLeft[currentLvl.getTile(x, y).blockVer];
                    }
                } else {
                    if(currentLvl.getTile(x + 1, y) != null){
                        if(currentLvl.getTile(x + 1, y).type == blockType.platform && (currentLvl.getTile(x - 1, y).type == blockType.button || currentLvl.getTile(x - 1, y).type == blockType.door || currentLvl.getTile(x - 1, y).type == blockType.spawn)){
                            sr.sprite = oneWays.endLeft[currentLvl.getTile(x, y).blockVer];
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
                if((checkTileOccupancy(x + 1, y) == 0 || x + 1 > size[0] - 1) && (checkTileOccupancy(x + 2, y) == 0 || x + 2 > size[0] - 1)){
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
                } else if(checkTileOccupancy(x, y - 1) != 0 && checkTileOccupancy(x, y + 1) == 0){
                    sr.sprite = this.GetComponent<w1Spikes>().top;
                } else{
                    sr.sprite = this.GetComponent<w1Spikes>().general;
                }
            }}}
        }
    }

    private System.Collections.IEnumerator OutputRoutine(string url) {
        var loader = new UnityWebRequest(url);
        yield return loader;
        loadPath = loader.ToString();
    }
}

public enum mode
{
    block = 0,
    platform = 1,
    spike = 2,
    tempPlat = 3,
    spawn = 4,
    door = 5,
    button = 6,
    twoStateBlock = 7,
    twoStateSpike = 8,
    twoStateButton = 9,
    twoStateLever = 10,

    redBlock = 20,
    blueBlock = 21,
    redSpike = 22,
    blueSpike = 23,

    single = 0,
    fill = 1
}

public enum tool
{
    draw = 0,
    erase = 1
}

namespace Common
{
    public static class DownloadFileHelper
    {
#if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void downloadToFile(string content, string filename);
#endif

        public static void DownloadToFile(string content, string filename)
        {
#if UNITY_WEBGL
            downloadToFile(content, filename);
#endif
        }
    }
}


