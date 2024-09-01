using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class death : MonoBehaviour
{

    void OnTriggerEnter2D(Collider2D obj){
        if(obj.gameObject.tag == "goose"){
            levelPlayer lvlMnger = GameObject.Find("levelManager").GetComponent<levelPlayer>();
            StartCoroutine(lvlMnger.resetPlayerPos());
            //GameObject.Find("levelManager").GetComponent<levelPlayer>().Load("", true);
        }
    }

}
