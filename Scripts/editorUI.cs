using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class editorUI : MonoBehaviour
{

    public Image tens;
    public Image ones;
    public Button up;
    public Button down;

    public Image ribbon;

    public List<Sprite> counterSprites;
    public List<Sprite> blockSelect;
    public List<coordinate2D> thumbnailSize;

    public editorController edit;
    GameObject pauseMenu;

    public InputAction esc;

    public void togglePanel(Animator anim){
        anim.SetBool("open", !anim.GetBool("open"));
    }

    public void minus(){
        edit.currentLvl.eggCount--;
    }

    public void add(){
        edit.currentLvl.eggCount++;
    }

    void Update(){
        if(edit.currentLvl.eggCount == 0){
            down.interactable = false;
        } else {
            down.interactable = true;
        }
        if(edit.currentLvl.eggCount == 99){
            up.interactable = false;
        } else {
            up.interactable = true;
        }

        ones.sprite = counterSprites[edit.currentLvl.eggCount - Mathf.FloorToInt(edit.currentLvl.eggCount / 10) * 10];
        tens.sprite = counterSprites[Mathf.FloorToInt(edit.currentLvl.eggCount / 10)];

        ribbon.sprite = blockSelect[edit.placeSelected];
        ribbon.gameObject.transform.localScale = new Vector3 (thumbnailSize[edit.placeSelected].x / 100f, thumbnailSize[edit.placeSelected].y / 100f, 1f);
    }

    void Start(){
        pauseMenu = GameObject.Find("Pause");
        pause(false);

        esc.Enable();
        esc.performed += context => pause(!pauseMenu.activeSelf);

        pauseMenu.transform.GetChild(pauseMenu.transform.childCount - 1).gameObject.GetComponent<Toggle>().isOn = Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen;
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
        SceneManager.LoadScene("Main Menu");
    }

}
