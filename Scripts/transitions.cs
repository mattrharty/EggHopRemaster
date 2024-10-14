using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class transitions : MonoBehaviour
{

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

}
