using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class menuManager : MonoBehaviour
{

    public TMP_Text version;

    public void play(){
        SceneManager.LoadScene("Level Player");
    }

    public void make(){
        SceneManager.LoadScene("level editor");
    }

    public void quit(){
        Application.Quit();
    }

    void Start(){
        version.text = Application.version;
        levelTemp.levelPlaying = levelTemp.currentLvl;
    }

}
