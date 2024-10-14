using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class menuManager : MonoBehaviour
{

    public TMP_Text version;

    public void play(){
        StartCoroutine(playDif());
    }

    public void make(){
        StartCoroutine(makeDif());
    }

    public void quit(){
        StartCoroutine(quitDif());
    }

    public IEnumerator playDif(){
        GameObject.FindGameObjectWithTag("transitions").GetComponent<transitions>().playTrans(false, 0);
        yield return new WaitUntil(() => GameObject.FindGameObjectWithTag("transitions").GetComponent<transitions>().anims[0].GetCurrentAnimatorStateInfo(0).normalizedTime < 1);
        yield return new WaitUntil(() => GameObject.FindGameObjectWithTag("transitions").GetComponent<transitions>().anims[0].GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        SceneManager.LoadScene("Level Player");
    }

    public IEnumerator makeDif(){
        GameObject.FindGameObjectWithTag("transitions").GetComponent<transitions>().playTrans(false, 0);
        yield return new WaitUntil(() => GameObject.FindGameObjectWithTag("transitions").GetComponent<transitions>().anims[0].GetCurrentAnimatorStateInfo(0).normalizedTime < 1);
        yield return new WaitUntil(() => GameObject.FindGameObjectWithTag("transitions").GetComponent<transitions>().anims[0].GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        SceneManager.LoadScene("level editor");
    }

    public IEnumerator quitDif(){
        GameObject.FindGameObjectWithTag("transitions").GetComponent<transitions>().playTrans(false, 0);
        yield return new WaitUntil(() => GameObject.FindGameObjectWithTag("transitions").GetComponent<transitions>().anims[0].GetCurrentAnimatorStateInfo(0).normalizedTime < 1);
        yield return new WaitUntil(() => GameObject.FindGameObjectWithTag("transitions").GetComponent<transitions>().anims[0].GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        Application.Quit();
    }

    void Awake(){
        version.text = Application.version;
        levelTemp.levelPlaying = levelTemp.currentLvl;
    } 

}
