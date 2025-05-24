using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using System.Text;
using SFB;
using System.Security.Cryptography;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;

public class levelLibrarian : MonoBehaviour
{

    [SerializeField] string path;

    makerProfile profile;

    [SerializeField] int levelRows = 12;

    bool selected = false;
    levelData selectedLvl;
    public Image selectedThumbnail;
    public TMP_Text selectedTitle;
    int selectedPlace = -4;

    [SerializeField] Scrollbar scroll;

    [SerializeField] GameObject filledSlotPrefab;
    List<GameObject> slots;
    [SerializeField] GameObject newSlotPrefab;
    [SerializeField] Transform levelSelector;


    //Variations for details panel:
    [SerializeField] GameObject emptyButtons;
    [SerializeField] GameObject filledButtons;
    [SerializeField] GameObject generalPanel;

    [SerializeField] Transform selector;

    [SerializeField] GameObject playInfo;
    [SerializeField] Sprite[] playSprites;
    [SerializeField] GameObject clearInfo;
    [SerializeField] Sprite[] clearSprites;

    gameManager gm;

    Dictionary<int, levelData> loadedLvls;
    //Level Placement, Level Data

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gm = GameObject.FindGameObjectWithTag("gameManager").GetComponent<gameManager>();

        loadedLvls = new Dictionary<int, levelData>();

        filledButtons.SetActive(false);
        emptyButtons.SetActive(false);
        generalPanel.SetActive(false);

        scroll.value = 0;

        slots = new List<GameObject>();

        for (int i = 0; i < levelRows; i++)
        {
            GameObject newSlot = Instantiate(newSlotPrefab, new Vector3(), new Quaternion(), levelSelector);
            Destroy(newSlot.GetComponent<Image>());
            Destroy(newSlot.GetComponent<Button>());
            DestroyImmediate(newSlot.transform.GetChild(0).gameObject);
            slots.Add(newSlot);
            GameObject newButton = Instantiate(newSlotPrefab, new Vector3(), new Quaternion(), newSlot.transform);
            string n = i.ToString();
            newButton.GetComponent<Button>().onClick.AddListener(() => clickSlot(int.Parse(n)));
        }

        path = Application.persistentDataPath + "/Custom Levels/";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        if (!File.Exists(path + "makerProfile.json"))
        {
            profile = new makerProfile();
            profile.clearedLvls = new Dictionary<string, bool>();
            profile.lvlPlacement = new Dictionary<string, int>();
            File.WriteAllText(path + "makerProfile.json", gm.encrypt(JsonConvert.SerializeObject(profile, Formatting.Indented)), Encoding.UTF8);
        }
        else
        {
            profile = JsonConvert.DeserializeObject<makerProfile>(gm.decrypt(File.ReadAllText(path + "makerProfile.json")));
        }
        string[] files = Directory.GetFiles(path);
        foreach (string file in files)
        {
            if (file != path + "makerProfile.json")
            {
                try
                {
                    levelData lvl = JsonConvert.DeserializeObject<levelData>(gm.decrypt(File.ReadAllText(file)));
                    if (profile.levels.Contains(lvl.levelID))
                    {
                        int i = profile.lvlPlacement[lvl.levelID];
                        loadedLvls.Add(i, lvl);

                        while(slots[i].transform.childCount > 0){
                            DestroyImmediate(slots[i].transform.GetChild(0).gameObject);
                        }
                        GameObject newSlot = Instantiate(filledSlotPrefab, new Vector3(), new Quaternion(), slots[i].transform);

                        string n = i.ToString();
                        newSlot.GetComponent<Button>().onClick.AddListener(() => clickSlot(int.Parse(n)));

                        newSlot.name = lvl.levelID;
                        newSlot.transform.GetChild(1).gameObject.GetComponent<TMP_Text>().text = lvl.title;
                        if(lvl.title.Length > 25){
                            newSlot.transform.GetChild(1).gameObject.GetComponent<TMP_Text>().text = lvl.title.Substring(0, 25) + "...";
                        }
                        if (lvl.thumbnail != null)
                        {
                            newSlot.transform.GetChild(2).gameObject.GetComponent<Image>().color = Color.white;
                            Texture2D newTex = new Texture2D(2, 2);
                            newTex.LoadImage(lvl.thumbnail);
                            newSlot.transform.GetChild(2).gameObject.GetComponent<Image>().sprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), new Vector2(0.5f, 0.5f));
                        }
                        else
                        {
                            newSlot.transform.GetChild(2).gameObject.GetComponent<Image>().color = Color.black;
                        }
                    }
                }
                catch (Exception error)
                {
                    Debug.LogError(error);
                }
            }

            List<string> ids = new List<string> {};
            foreach(KeyValuePair<string, int> place in profile.lvlPlacement){
                if(!loadedLvls.ContainsKey(place.Value)){
                    //ids.Add(place.Key);
                }
            }
            foreach(string id in ids){
                profile.lvlPlacement.Remove(id);
                profile.clearedLvls.Remove(id);
                profile.levels.Remove(id);
            }
        }

        GameObject.FindGameObjectWithTag("transitions").GetComponent<transitions>().playTrans(true, 0);
    }

    void Update(){
        selector.rotation = new Quaternion(0, 0, 0, 0);
    }

    public void newLevel()
    {
        int seed = UnityEngine.Random.Range(100000, 999999);
        selectedLvl = new levelData();
        selectedLvl.size = new int[] { 30, 30 };
        selectedLvl.seed = seed;
        selectedLvl.levelID = profile.generateLevelID(seed);
        selectedLvl.title = "New Level";
        selectedLvl.blocks = new Dictionary<coordinate2D, block>();
        selectedLvl.occupiedTiles = new List<coordinate2D>();
        selectedLvl.tags = new List<string> { "unplayable" };
        profile.addLevel(selectedLvl.levelID, selectedPlace);

        File.WriteAllText(path + selectedLvl.levelID + ".goose", gm.encrypt(JsonConvert.SerializeObject(selectedLvl, Formatting.Indented)), Encoding.UTF8);
        openLvl();
    }

    public void openLvl()
    {
        gm.currentLvl = selectedLvl;
        File.WriteAllText(path + "makerProfile.json", gm.encrypt(JsonConvert.SerializeObject(profile, Formatting.Indented)), Encoding.UTF8);
        StartCoroutine(editor());
    }

    public void playLvl()
    {
        gm.currentLvl = selectedLvl;
        File.WriteAllText(path + "makerProfile.json", gm.encrypt(JsonConvert.SerializeObject(profile, Formatting.Indented)), Encoding.UTF8);
        StartCoroutine(player());
    }

    public void deleteLvl()
    {
        string id = selectedLvl.levelID;
        profile.levels.Remove(id);
        profile.lvlPlacement.Remove(id);
        profile.clearedLvls.Remove(id);
        loadedLvls.Remove(selectedPlace);

        GameObject oldButton = slots[selectedPlace].transform.GetChild(0).gameObject;
        int i = selectedPlace;
        GameObject newButton = Instantiate(newSlotPrefab, slots[selectedPlace].transform.position, new Quaternion(), slots[selectedPlace].transform);
        selector.parent = newButton.transform;
        selector.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
        selector.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
        selector.GetComponent<Animator>().Play("Selector", 0, 0f);
        string n = i.ToString();
        newButton.GetComponent<Button>().onClick.AddListener(() => clickSlot(int.Parse(n)));
        Destroy(oldButton);
        File.Delete(path + id + ".goose");
        File.WriteAllText(path + "makerProfile.json", gm.encrypt(JsonConvert.SerializeObject(profile, Formatting.Indented)), Encoding.UTF8);
        clickSlot(selectedPlace);
    }

    public void importLvl()
    {
        var extensions = new[] {
            new ExtensionFilter("levels", "txt", "goose"),
            new ExtensionFilter("All Files", "*" ),
        };
        string iPath = "";
        try
        {
#if !UNITY_WEBGL
            iPath = StandaloneFileBrowser.OpenFilePanel("Load custom level", "", extensions, false)[0];
#else
                [DllImport("__Internal")]
                static extern void UploadFile(string gameObjectName, string methodName, string filter, bool multiple);
                UploadFile(gameObject.name, "OnFileUpload", "goose", false);
                iPath = loadPath;
#endif
        }
        catch
        {
            return;
        }
        if (iPath == null || iPath == "")
        {
            return;
        }

        selectedLvl = JsonConvert.DeserializeObject<levelData>(gm.decrypt(File.ReadAllText(iPath)));
        string[] listId = selectedLvl.levelID.Split('-');
        selectedLvl.levelID = listId[0] + "-" + listId[1] + "-" + RandomString(3) + selectedPlace;
        if(profile.levels.Contains(selectedLvl.levelID)){
            return;
        }
        File.WriteAllText(path + selectedLvl.levelID + ".goose", gm.encrypt(JsonConvert.SerializeObject(selectedLvl, Formatting.Indented)), Encoding.UTF8);
        profile.addLevel(selectedLvl.levelID, selectedPlace);
        File.WriteAllText(path + "makerProfile.json", gm.encrypt(JsonConvert.SerializeObject(profile, Formatting.Indented)), Encoding.UTF8);

        string id = selectedLvl.levelID;
        GameObject oldButton = slots[selectedPlace].transform.GetChild(0).gameObject;
        int i = selectedPlace;
        Vector3 newPos = slots[selectedPlace].transform.position;
        GameObject newSlot = Instantiate(filledSlotPrefab, newPos, new Quaternion(), slots[selectedPlace].transform);
        selector.parent = newSlot.transform;
        selector.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
        selector.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
        selector.GetComponent<Animator>().Play("Selector", 0, 0f);
        string n = i.ToString();
        levelData lvl = selectedLvl;
        newSlot.GetComponent<Button>().onClick.AddListener(() => clickSlot(int.Parse(n)));
        newSlot.name = lvl.levelID;
        newSlot.transform.GetChild(1).gameObject.GetComponent<TMP_Text>().text = lvl.title;
        if(lvl.title.Length > 25){
            newSlot.transform.GetChild(1).gameObject.GetComponent<TMP_Text>().text = lvl.title.Substring(0, 25) + "...";
        }
        if (lvl.thumbnail != null)
        {
            newSlot.transform.GetChild(2).gameObject.GetComponent<Image>().color = Color.white;
            Texture2D newTex = new Texture2D(2, 2);
            newTex.LoadImage(lvl.thumbnail);
            newSlot.transform.GetChild(2).gameObject.GetComponent<Image>().sprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), new Vector2(0.5f, 0.5f));
        }
        else
        {
            newSlot.transform.GetChild(2).gameObject.GetComponent<Image>().color = Color.black;
        }
        loadedLvls[selectedPlace] = selectedLvl;
        Destroy(oldButton);
        File.WriteAllText(path + "makerProfile.json", gm.encrypt(JsonConvert.SerializeObject(profile, Formatting.Indented)), Encoding.UTF8);
        clickSlot(selectedPlace);
        StartCoroutine(clickDelay(selectedPlace));
    }

    System.Collections.IEnumerator clickDelay(int i){
        yield return new WaitForEndOfFrame();
        clickSlot(i);
    }

    public void clickSlot(int lvlPlace)
    {
        selectedPlace = lvlPlace;
        generalPanel.SetActive(true);
        if (loadedLvls.ContainsKey(lvlPlace))
        {
            selectedLvl = loadedLvls[lvlPlace];
            filledButtons.SetActive(true);
            emptyButtons.SetActive(false);
            selectedTitle.text = selectedLvl.title;
            if(selectedLvl.title.Length > 65){
                selectedTitle.fontSize = 36;
            } else {
                selectedTitle.fontSize = 48;
            }
            if (selectedLvl.thumbnail != null)
            {
                selectedThumbnail.color = Color.white;
                Texture2D newTex = new Texture2D(2, 2);
                newTex.LoadImage(selectedLvl.thumbnail);
                selectedThumbnail.sprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), new Vector2(0.5f, 0.5f));
            }
            else
            {
                selectedThumbnail.color = Color.black;
            }

            selector.parent = slots[lvlPlace].transform;
            selector.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
            selector.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
            selector.GetComponent<Animator>().Play("Selector", 0, 0f);
            selector.GetComponent<Animator>().speed = 1;

            clearInfo.SetActive(true);
            playInfo.SetActive(true);
            if(profile.clearedLvls[selectedLvl.levelID]){
                clearInfo.transform.GetChild(0).GetComponent<Image>().sprite = clearSprites[0];
                clearInfo.transform.GetChild(1).GetComponent<TMP_Text>().text = "Cleared";
            } else {
                clearInfo.transform.GetChild(0).GetComponent<Image>().sprite = clearSprites[1];
                clearInfo.transform.GetChild(1).GetComponent<TMP_Text>().text = "Uncleared";
            }

            if(!selectedLvl.tags.Contains("unplayable")){
                playInfo.transform.GetChild(0).GetComponent<Image>().sprite = playSprites[0];
                playInfo.transform.GetChild(1).GetComponent<TMP_Text>().text = "Playable";

                filledButtons.transform.GetChild(1).GetComponent<Button>().interactable = true;
                filledButtons.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = new Color32(217, 206, 189, 255);
                filledButtons.transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<Image>().color = new Color32(217, 206, 189, 255);
                filledButtons.transform.GetChild(1).GetChild(0).GetChild(1).GetComponent<Image>().color = new Color32(217, 206, 189, 255);
            } else {
                playInfo.transform.GetChild(0).GetComponent<Image>().sprite = playSprites[1];
                playInfo.transform.GetChild(1).GetComponent<TMP_Text>().text = "Unplayable";

                filledButtons.transform.GetChild(1).GetComponent<Button>().interactable = false;
                filledButtons.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = new Color32(180, 180, 180, 255);
                filledButtons.transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<Image>().color = new Color32(180, 180, 180, 255);
                filledButtons.transform.GetChild(1).GetChild(0).GetChild(1).GetComponent<Image>().color = new Color32(180, 180, 180, 255);
            }
        }
        else
        {
            clearInfo.SetActive(false);
            playInfo.SetActive(false);

            filledButtons.SetActive(false);
            emptyButtons.SetActive(true);
            selectedTitle.text = "Empty Slot";
            selectedThumbnail.color = Color.black;

            selector.parent = slots[lvlPlace].transform;
            selector.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
            selector.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
            Debug.Log("reselect");
            selector.GetComponent<Animator>().Play("Selector", 0, 0f);
        }

        
    }

    public void exit()
    {
        File.WriteAllText(path + "makerProfile.json", gm.encrypt(JsonConvert.SerializeObject(profile, Formatting.Indented)), Encoding.UTF8);
        StartCoroutine(mainMenu());
    }

    public System.Collections.IEnumerator mainMenu()
    {
        GameObject.FindGameObjectWithTag("transitions").GetComponent<transitions>().playTrans(false, 0);
        yield return new WaitUntil(() => GameObject.FindGameObjectWithTag("transitions").GetComponent<transitions>().anims[0].GetCurrentAnimatorStateInfo(0).normalizedTime < 1);
        yield return new WaitUntil(() => GameObject.FindGameObjectWithTag("transitions").GetComponent<transitions>().anims[0].GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        SceneManager.LoadScene("Main Menu");
    }

    public System.Collections.IEnumerator editor()
    {
        GameObject.FindGameObjectWithTag("transitions").GetComponent<transitions>().playTrans(false, 0);
        yield return new WaitUntil(() => GameObject.FindGameObjectWithTag("transitions").GetComponent<transitions>().anims[0].GetCurrentAnimatorStateInfo(0).normalizedTime < 1);
        yield return new WaitUntil(() => GameObject.FindGameObjectWithTag("transitions").GetComponent<transitions>().anims[0].GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        SceneManager.LoadScene("level editor");
    }

    public System.Collections.IEnumerator player()
    {
        GameObject.FindGameObjectWithTag("transitions").GetComponent<transitions>().playTrans(false, 0);
        yield return new WaitUntil(() => GameObject.FindGameObjectWithTag("transitions").GetComponent<transitions>().anims[0].GetCurrentAnimatorStateInfo(0).normalizedTime < 1);
        yield return new WaitUntil(() => GameObject.FindGameObjectWithTag("transitions").GetComponent<transitions>().anims[0].GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        SceneManager.LoadScene("Level Player");
    }

    private static System.Random random = new System.Random();
    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}

[Serializable]
public class makerProfile
{
    public string userID;

    public List<string> levels;


    public Dictionary<string, int> lvlPlacement;
    //Level ID, Library Placement

    public Dictionary<string, bool> clearedLvls;
    //Level ID, clearState

    public makerProfile()
    {
        string Id = RandomString(6);
        userID = Id;
        levels = new List<string>();
    }

    public string generateLevelID(int seed)
    {
        string Id = userID + "-" + RandomString(4) + Mathf.FloorToInt(seed / 1000).ToString() + "-" + RandomString(3);
        return Id;
    }

    public void addLevel(string lvlID, int requestedPlace)
    {
        levels.Add(lvlID);
        lvlPlacement.Add(lvlID, requestedPlace);
        clearedLvls.Add(lvlID, false);
    }

    private static System.Random random = new System.Random();
    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}

public class buttonIndex
{
    public int index = 0;
}
