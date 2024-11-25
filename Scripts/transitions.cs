using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class transitions : MonoBehaviour
{

    EventSystem system;
    Scene lastScene;

    public List<Animator> anims;
    /*
    Generic
    Live Goose
    Dead Goose
    */

    public void playTrans(bool open, int anim){
        for(int i = 0; i < anims.Count; i++){
            if(i == anim){
                anims[i].gameObject.SetActive(true);
            } else {
                anims[i].gameObject.SetActive(false);
            }
        }
        if(open){
            anims[anim].SetTrigger("open");
            anims[anim].transform.rotation = new Quaternion (0, 180, 0, 0);
        } else {
            anims[anim].SetTrigger("close");
            anims[anim].transform.rotation = new Quaternion (0, 0, 0, 0);
        }
    }

    void Start(){
        lastScene = SceneManager.GetActiveScene();
        system = EventSystem.current;
    }

    void Update(){
        if(lastScene != SceneManager.GetActiveScene()){
            lastScene = SceneManager.GetActiveScene();
            system = EventSystem.current;
        }
        if(anims[0].GetCurrentAnimatorStateInfo(0).normalizedTime < 1){
            system.enabled = false;
        } else {
            system.enabled = true;
        }
    }

}
