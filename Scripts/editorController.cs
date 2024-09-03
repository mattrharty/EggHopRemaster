using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.IO;
using System;
using SFB;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine.U2D;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

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
    public TMP_InputField seedField;   public int[] mousePos = {0,0};
    public GameObject tileCursor;
    public Sprite[] cursorPics;
    public Color[] cursorColors;

    public GameObject tilePrefab;
    public Transform gridDaddy;

    public int placeMode;
    public int placeSelected;

    public camControl camScript;

    public List<Button> buttons;

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

    public  List<Sprite> w1ColTop;
    public  List<Sprite> w1ColMid;
    public  List<Sprite> w1ColBot;
    public  List<Sprite> w1ColSingle;
    Dictionary<string, List<Sprite>> w1Columns = new Dictionary<string, List<Sprite>>();

    w1OneWay oneWays;

    int rotate = 0;

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

    // Start is called before the first frame update
    void Start()
    {
        xField.characterLimit = maxSize;
        yField.characterLimit = maxSize;
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

        GameObject[] buttonsObj = GameObject.FindGameObjectsWithTag("blockPanel");
        foreach(GameObject obj in buttonsObj){
            buttons.Add(obj.GetComponent<Button>());
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
        singleFill.isOn = placeMode == (int)mode.fill;

        currentLvl = new levelData ();
        currentLvl.blocks = new Dictionary<coordinate2D, block>();
        currentLvl.occupiedTiles = new List<coordinate2D> ();

        gridReload();

        versionDis.text = "Egg Hop " + Application.version;

        oneWays = this.GetComponent<w1OneWay>();
    }

    void hotkey(Button toolButton){
        selectTool(Array.IndexOf(tools, toolButton));
        selectTool2(toolButton);
    }

    void leftClicked(){
        if (!tileCursor.activeSelf){
            return;
        }
        bool readyToPlace = true;
        if(placeMode == (int)mode.fill){
            fill();
        } else if (placeSelected == (int)mode.spawn){
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

            //Adds the prefab to placed blocks
            placedBlocks[mousePos[0], mousePos[1]] = newSpawn;
        } else if (placeSelected == (int)mode.button){
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

            //Adds the prefab to placed blocks
            placedBlocks[mousePos[0], mousePos[1]] = newButton;
        } else if (placeSelected == (int)mode.door){
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

            //Adds the prefab to placed blocks
            placedBlocks[mousePos[0], mousePos[1]] = newDoor;
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
        SceneManager.LoadScene("Main Menu");
    }

    void fill(){
        if(placeMode == (int)mode.single){
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
                if(placeSelected != (int)mode.erase){
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
        if(placeSelected == (int)mode.block ||
            placeSelected == (int)mode.platform ||
            placeSelected == (int)mode.obstacle){
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

            if(placingOne() || placeSelected == (int)mode.erase){
                tileCursor.transform.GetChild(0).gameObject.SetActive(true);
                tileCursor.transform.GetChild(1).gameObject.SetActive(false);
                tileCursor.transform.GetChild(2).gameObject.SetActive(false);
                tileCursor.transform.GetChild(3).gameObject.SetActive(false);
                tileCursor.transform.GetChild(4).gameObject.SetActive(false);
            }else if (placeSelected == (int)mode.spawn) {
                tileCursor.transform.GetChild(0).gameObject.SetActive(false);
                tileCursor.transform.GetChild(1).gameObject.SetActive(true);
                tileCursor.transform.GetChild(2).gameObject.SetActive(false);
                tileCursor.transform.GetChild(3).gameObject.SetActive(false);
                tileCursor.transform.GetChild(4).gameObject.SetActive(true);
                //tileCursor.transform.GetChild(5).gameObject.SetActive(false);
            }else if (placeSelected == (int)mode.button) {
                tileCursor.transform.GetChild(0).gameObject.SetActive(false);
                tileCursor.transform.GetChild(1).gameObject.SetActive(false);
                tileCursor.transform.GetChild(2).gameObject.SetActive(true);
                tileCursor.transform.GetChild(3).gameObject.SetActive(false);
                tileCursor.transform.GetChild(4).gameObject.SetActive(true);
                //tileCursor.transform.GetChild(5).gameObject.SetActive(false);
            }else if (placeSelected == (int)mode.door) {
                tileCursor.transform.GetChild(0).gameObject.SetActive(false);
                tileCursor.transform.GetChild(1).gameObject.SetActive(false);
                tileCursor.transform.GetChild(2).gameObject.SetActive(false);
                tileCursor.transform.GetChild(3).gameObject.SetActive(true);
                tileCursor.transform.GetChild(4).gameObject.SetActive(true);
                //tileCursor.transform.GetChild(5).gameObject.SetActive(false);
            }

            if(placeSelected != (int)mode.erase){
                tileCursor.GetComponent<SpriteRenderer>().sprite = cursorPics[1];
                tileCursor.GetComponent<SpriteRenderer>().color = cursorColors[1];
            } else {
                tileCursor.GetComponent<SpriteRenderer>().sprite = cursorPics[0];
                tileCursor.GetComponent<SpriteRenderer>().color = cursorColors[0];
            }

            if(currentLvl.getTile(mousePos[0], mousePos[1]) != null && placingOne() || currentLvl.getTile(mousePos[0], mousePos[1]) == null && placeSelected == (int)mode.erase){
                tileCursor.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().color = cursorColors[0];
            } else if(placingOne() || placeSelected == (int)mode.erase){
                tileCursor.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().color = cursorColors[1];
            } else if(new List<int> {(int)mode.spawn, (int)mode.button, (int)mode.door}.Contains(placeSelected)){
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

                if(placeSelected == (int)mode.spawn){
                    for (int x = -1; x <= 1; x++){
                        for (int y = 0; y <= 2; y++){
                            try {
                                if (currentLvl.getTile(x + mousePos[0], y + mousePos[1]) == null || currentLvl.getTile(x + mousePos[0], y + mousePos[1]).type == blockType.spawn){
                                    tileCursor.transform.GetChild(1).GetChild(y + 3 * x + 3).gameObject.GetComponent<SpriteRenderer>().color = cursorColors[1];
                                } else {
                                    tileCursor.transform.GetChild(1).GetChild(y + 3 * x + 3).gameObject.GetComponent<SpriteRenderer>().color = cursorColors[0];
                                }
                            } catch {
                                tileCursor.transform.GetChild(1).GetChild(y + 3 * x + 3).gameObject.GetComponent<SpriteRenderer>().color = cursorColors[0];
                            }
                        }
                    }
                } else if(placeSelected == (int)mode.button){
                    for (int x = -1; x <= 1; x++){
                        for (int y = 0; y <= 0; y++){
                            try {
                                if (currentLvl.getTile(x + mousePos[0], y + mousePos[1]) == null || currentLvl.getTile(x + mousePos[0], y + mousePos[1]).type == blockType.button){
                                    tileCursor.transform.GetChild(2).GetChild(y + 1 * x + 1).gameObject.GetComponent<SpriteRenderer>().color = cursorColors[1];
                                } else {
                                    tileCursor.transform.GetChild(2).GetChild(y + 1 * x + 1).gameObject.GetComponent<SpriteRenderer>().color = cursorColors[0];
                                }
                            } catch {
                                tileCursor.transform.GetChild(2).GetChild(y + 1 * x + 1).gameObject.GetComponent<SpriteRenderer>().color = cursorColors[0];
                            }
                        }
                    }
                } else if(placeSelected == (int)mode.door){
                    for (int x = -1; x <= 1; x++){
                        for (int y = 0; y <= 3; y++){
                            try {
                                if (currentLvl.getTile(x + mousePos[0], y + mousePos[1]) == null || currentLvl.getTile(x + mousePos[0], y + mousePos[1]).type == blockType.door){
                                    tileCursor.transform.GetChild(3).GetChild(y + 4 * x + 4).gameObject.GetComponent<SpriteRenderer>().color = cursorColors[1];
                                } else {
                                    tileCursor.transform.GetChild(3).GetChild(y + 4 * x + 4).gameObject.GetComponent<SpriteRenderer>().color = cursorColors[0];
                                }
                            } catch {
                                tileCursor.transform.GetChild(3).GetChild(y + 4 * x + 4).gameObject.GetComponent<SpriteRenderer>().color = cursorColors[0];
                            }
                        }
                    }
                }
            }
        }

        if(leftClick.IsPressed() && EventSystem.current.currentSelectedGameObject == null){
            if (placeMode == (int)mode.single){
                if(placingOne() && tileCursor.activeSelf){
                    Place(mousePos[0], mousePos[1], true);
                    reloadBlocks();
                } else if(placeSelected == (int)mode.erase){
                    erase(mousePos[0], mousePos[1]);
                    reloadBlocks();
                }
            } else if (placeMode == (int)mode.fill){
                if(fillX < 0){
                    //Debug.Log("resetting fill positions");
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
                        if(currentLvl.getTile(x, y) != null && placeSelected != (int)mode.erase || currentLvl.getTile(x, y) == null && placeSelected == (int)mode.erase){
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
        string newLog = "";
        newLog = "";
        if(!spawnPlaced || !buttonPlaced || !doorPlaced){
            log.color = cursorColors[0];
            if(!spawnPlaced){
                newLog += "spawn is missing\n";
                flashButton(buttons[2].gameObject);
            } if(!buttonPlaced){
                newLog += "button is missing\n";
                flashButton(buttons[3].gameObject);
            } if(!doorPlaced){
                newLog += "door is missing\n";
                flashButton(buttons[4].gameObject);
            }
            //Debug.Log(newLog + spawnPlaced + ", " + buttonPlaced + ", " + doorPlaced);
            log.text = newLog;
            return;
        }

        /*string saveLoc = Application.persistentDataPath;
        if(saveLoc == null){
            newLog = "Failed to save to Downloads folder";
            log.color = cursorColors[0];
            log.text = newLog;
            return;
        }*/
        string saveName = "Custom Level " + DateTime.Now.TimeOfDay.TotalSeconds;
        
            
        /*if(levelName.text != null){
            saveName = levelName.text;
        }*/
        if(levelName.text.Length > 0){
            saveName = levelName.text;
        }

        //Common.DownloadFileHelper.DownloadToFile(content, saveName);
        var extensionList = new [] {
            new ExtensionFilter("Level", "goose"),
            //new ExtensionFilter("Text", "txt"),
            new ExtensionFilter("All files", "*")
        };
        string path = "";
        currentLvl.size = size;
        #if !UNITY_WEBGL
            path = StandaloneFileBrowser.SaveFilePanel("Save custom level", "", saveName, extensionList);
        #endif
        if(path == "" || path == null){
            newLog = "No download path selected";
            log.color = cursorColors[0];
            log.text = newLog;
        }
        File.WriteAllText(path, JsonConvert.SerializeObject(currentLvl));

        levelTemp.currentLvl = currentLvl;
        levelTemp.levelPlaying = currentLvl;

        newLog = "Saved " + path;
        log.color = cursorColors[1];
        log.text = newLog;
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
        
        if(currentLvl.checkTile(x, y) && playerPlaced){
            return;
        }

        

        coordinate2D newCoord = new coordinate2D (x, y);
        if(!currentLvl.occupiedTiles.Contains(newCoord)){
            currentLvl.occupiedTiles.Add(newCoord);
        }
        
        GameObject newBlock = Instantiate(tilePrefab, new Vector3 (x, y, 0), new Quaternion());

        if(placeSelected == (int)mode.block){
            placeBlock(x, y, newBlock, playerPlaced);
        } else if (placeSelected == (int)mode.platform){
            placePlatform(x, y, newBlock, playerPlaced);
        } else if (placeSelected == (int)mode.obstacle){
            placeDeath(x, y, newBlock, playerPlaced);
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
            currentLvl.blocks.Add(newCoord, new block (blockType.obstacle, newCoord, 0, 0, false));
        }

        newObstacle.GetComponent<SpriteRenderer>().sprite = this.GetComponent<w1Spikes>().general;
        newObstacle.name = "Placed Obstacle";
    }

    void erase(int x, int y){ 
        if (currentLvl.getTile(x, y) == null || !tileCursor.activeSelf){
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

        //Deletes 1x1 blocks
        GameObject.Destroy(placedBlocks[x, y]);
        coordinate2D delCoord = new coordinate2D (x, y);
        currentLvl.occupiedTiles.Remove(delCoord);
        currentLvl.blocks.Remove(delCoord);
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
        var extensions = new [] {
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
        log.text = "loading " + path;

        //Converts custom level to string
        currentLvl = JsonConvert.DeserializeObject<levelData>(File.ReadAllText(path));

        //Clears all currently placed blocks
        for(int x = 0; x < size[0]; x++){
            for(int y = 0; y < size[1]; y++){
                GameObject.Destroy(placedBlocks[x, y]);
            }
        }

        //Reads and interprets the level size
        size = currentLvl.size;
        camScript.bounds[0] = size[1];
        camScript.bounds[1] = size[0];
        camScript.bounds[2] = 0;
        camScript.bounds[3] = 0;
        xField.text = size[0].ToString();
        yField.text = size[1].ToString();

        //Reads the tiledata and builds the level accordingly
        placedBlocks = new GameObject [size[0], size[1]];

        loadingLevel = true;
        foreach(KeyValuePair<coordinate2D, block> block in currentLvl.blocks){
            //int x = Mathf.FloorToInt((i - 4) / size[1]);
            //Debug.Log("(" + (i - 4 - x * size[0]) + ", " + x + ")");
            placeSelected = (int)mode.block;
            if(block.Value.type == blockType.block){
                Place(block.Value.placePos.x, block.Value.placePos.y, false);
            }
            if(block.Value.type == blockType.spawn && block.Value.coreTile){
                placedBlocks[block.Value.placePos.x, block.Value.placePos.y] = Instantiate(otherObjects[0], new Vector3(block.Value.placePos.x, block.Value.placePos.y, 0), new Quaternion());
            }
            if(block.Value.type == blockType.button && block.Value.coreTile){
                placedBlocks[block.Value.placePos.x, block.Value.placePos.y] = Instantiate(otherObjects[1], new Vector3(block.Value.placePos.x, block.Value.placePos.y, 0), new Quaternion());
            }
            if(block.Value.type == blockType.door && block.Value.coreTile){
                placedBlocks[block.Value.placePos.x, block.Value.placePos.y] = Instantiate(otherObjects[2], new Vector3(block.Value.placePos.x, block.Value.placePos.y, 0), new Quaternion());
            }
            if(block.Value.type == blockType.platform){
                placeSelected = (int)mode.platform;
                Place(block.Value.placePos.x, block.Value.placePos.y, false);
            }
            if(block.Value.type == blockType.obstacle){
                placeSelected = (int)mode.obstacle;
                Place(block.Value.placePos.x, block.Value.placePos.y, false);
            }
        }
        loadingLevel = false;

        gridReload();
        log.text = "Successfully loaded " + path;
    }

    public void selectTool(int select){
        //Debug.Log(select);
        placeSelected = select;

        if(select != (int)mode.obstacle){
            rotate = 0;
        }

        if (select == (int)mode.spawn || select == (int)mode.button || select == (int)mode.door){
            singleFill.isOn = false;
            singleFill.interactable = false;
            placeMode = (int)mode.single;
        } else {
            singleFill.interactable = true;
            if(singleFill.isOn){
                placeMode = (int)mode.fill;
            } else {
                placeMode = (int)mode.single;
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

    public void selectPlaceMethod(Toggle toggle){
        if(toggle.isOn){
            placeMode = (int)mode.fill;
        } else {
            placeMode = (int)mode.single;
        }
    }

    //Reloads the grid to match dimensions
    void gridReload(){
        //Deletes past grid
        for(int i = 0; i < gridDaddy.childCount; i++){
            Destroy(gridDaddy.GetChild(i).gameObject);
        }

        //Creates new grid
        for(int x = 0; x < size[0]; x++){
            for (int y = 0; y < size[1]; y++){
                Vector3 tilePos = new Vector3 (x, y, 0);
                GameObject newTile = Instantiate(tilePrefab, tilePos, gridDaddy.rotation, gridDaddy);
                newTile.GetComponent<SpriteRenderer>().sortingOrder = 1;
            }
        }

        //Creates border around entire grid using old blocks
        foreach(Transform child in blockBorderDaddy){
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
    }

    void placeBorderVoid(int block, Vector3 newPos, float width, float height){
        Transform newBorderBlock = Instantiate(tilePrefab, newPos, new Quaternion(), blockBorderDaddy).transform;
        newBorderBlock.gameObject.GetComponent<SpriteRenderer>().sprite = borderBlocks[block];
        newBorderBlock.gameObject.GetComponent<SpriteRenderer>().color = new Color (0.65f, 0.65f, 0.65f);
        newBorderBlock.gameObject.GetComponent<SpriteRenderer>().sortingOrder = -2;
        newBorderBlock.gameObject.GetComponent<SpriteRenderer>().material = litMat;
        newBorderBlock.localScale = new Vector3(width, height, 1);
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
                                //Debug.Log(index);
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
            } else if(currentLvl.getTile(x, y).type == blockType.obstacle){
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

public enum mode{
    block = 0,
    erase = 1,
    spawn = 2,
    button = 3,
    door = 4,
    obstacle = 5,
    platform = 6,
    // ^^Priority^^
    slime = 7,
    blueBlock = 8,
    redBlock = 9,

    single = 0,
    fill = 1
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


