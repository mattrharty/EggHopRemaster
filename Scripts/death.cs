using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class death : MonoBehaviour
{

    [SerializeField] public int source = 0;

    void OnTriggerEnter2D(Collider2D obj){
        if(obj.gameObject.tag == "goose"){
            GameObject.FindGameObjectWithTag("transitions").GetComponent<transitions>().playTrans(false, 1);
            levelPlayer lvlMnger = GameObject.Find("levelManager").GetComponent<levelPlayer>();
            StartCoroutine(lvlMnger.resetPlayerPos(source));
            //GameObject.Find("levelManager").GetComponent<levelPlayer>().Load("", true);
        }
    }

}
