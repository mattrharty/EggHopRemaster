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
    public List<Sprite> blueRedBlocks;
    public Image blueRedBlock;
    public List<Sprite> blueRedSpikes;
    public Image blueRedSpike;

    public editorController edit;
    GameObject pauseMenu;

    public InputAction esc;

    public void togglePanel(Animator anim){
        anim.SetBool("open", !anim.GetBool("open"));
    }

    public void setRed(Animator anim){
        anim.SetBool("isRed", edit.blueRed.isOn);
        edit.redSelect = edit.blueRed.isOn;
        if(edit.placeSelected == mode.redBlock || edit.placeSelected == mode.blueBlock){
            if(edit.redSelect){
                edit.placeSelected = mode.redBlock;
            } else {
                edit.placeSelected = mode.blueBlock;
            }
        }

        if(edit.placeSelected == mode.redSpike || edit.placeSelected == mode.blueSpike){
            if(edit.redSelect){
                edit.placeSelected = mode.redSpike;
            } else {
                edit.placeSelected = mode.blueSpike;
            }
        }
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

        foreach (blockEditor obj in edit.palette)
        {
            if (obj.isTwoState)
            {
                if (edit.redSelect)
                {
                    blockSelect[edit.palette.IndexOf(obj)] = obj.primaryTex[0];
                }
                else
                {
                    blockSelect[edit.palette.IndexOf(obj)] = obj.primaryTex[1];
                }
                //blueRedBlock.sprite = blockSelect[8];
            }
        }

        /*if (edit.redSelect)
        {
            blockSelect[7] = blueRedBlocks[0];
        }
        else
        {
            blockSelect[7] = blueRedBlocks[1];
        }
        blueRedBlock.sprite = blockSelect[8];

        if(edit.redSelect){
            blockSelect[8] = blueRedSpikes[0];
        } else {
            blockSelect[8] = blueRedSpikes[1];
        }
        blueRedSpike.sprite = blockSelect[12];*/


        ribbon.sprite = blockSelect[(int)edit.placeSelected];
        ribbon.gameObject.transform.localScale = new Vector3 (thumbnailSize[(int)edit.placeSelected].x / 100f, thumbnailSize[(int)edit.placeSelected].y / 100f, 1f);

        edit.blueRed.transform.GetComponent<Animator>().SetBool("Disabled", !edit.blueRed.interactable);
    }

    void Start(){
        pauseMenu = GameObject.Find("Pause");
        pause(false);

        esc.Enable();
        esc.performed += context => pause(!pauseMenu.activeSelf);

        pauseMenu.transform.GetChild(pauseMenu.transform.childCount - 1).gameObject.GetComponent<Toggle>().isOn = Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen;
        GameObject.FindGameObjectWithTag("transitions").GetComponent<transitions>().playTrans(true, 0);
    }

    public void pause(bool paused){
        pauseMenu.SetActive(paused);
        if(paused){
            Time.timeScale = 0;
        } else {
            Time.timeScale = 1;
        }
    }

    public void mainMenu(){
        esc.Disable();
        StartCoroutine(loadMain());
    }

    IEnumerator loadMain(){
        GameObject.FindGameObjectWithTag("transitions").GetComponent<transitions>().playTrans(false, 0);
        yield return new WaitUntil(() => GameObject.FindGameObjectWithTag("transitions").GetComponent<transitions>().anims[0].GetCurrentAnimatorStateInfo(0).normalizedTime < 1);
        yield return new WaitUntil(() => GameObject.FindGameObjectWithTag("transitions").GetComponent<transitions>().anims[0].GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        SceneManager.LoadScene("level editor");
    }

}
