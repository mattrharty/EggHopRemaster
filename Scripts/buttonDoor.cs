using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class buttonDoor : MonoBehaviour
{

    public static bool pressed = false;

    public blockType type;
    public List<Sprite> state;
    levelPlayer lvl;
    SpriteRenderer sr;
    Animator anim;

    SpriteRenderer goose;

    Light2D bing;

    void Start(){
        pressed = false;

        sr = transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
        sr.sprite = state[0];

        lvl = GameObject.Find("levelManager").GetComponent<levelPlayer>();

        if(type == blockType.door){
            anim = transform.GetChild(0).gameObject.GetComponent<Animator>();
            anim.SetBool("pressed", false);
            anim.SetBool("exit", false);
        } else if (type == blockType.button){
            bing = this.GetComponent<Light2D>();
            bing.color = Color.red;
        }
    }

    void Update(){
        if(type == blockType.door){
            anim.SetBool("pressed", pressed);
            if(goose != null){
                if(sr.sprite == state[2]){
                    goose.enabled = false;
                    lvl.Finish();
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D obj){
        //Debug.Log(obj);
        if(obj.gameObject.tag == "goose" || obj.gameObject.tag == "egg"){
            if(type == blockType.button && !pressed){
                pressed = true;
                sr.sprite = state[1];
                bing.color = Color.green;
            }
        }
    }

    void OnTriggerStay2D(Collider2D obj){
        //Debug.Log(obj);
        if (type == blockType.door && obj.gameObject.tag == "goose"){
            if(pressed && obj.transform.parent.gameObject.GetComponent<PlayerMovement>().grounded()){
                goose = obj.transform.parent.gameObject.GetComponent<SpriteRenderer>();
                anim.SetBool("exit", true);
                obj.transform.parent.gameObject.GetComponent<PlayerMovement>().rb.linearVelocity = new Vector2 (0, 0);
                obj.transform.parent.gameObject.GetComponent<PlayerMovement>().enabled = false;
                obj.transform.parent.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
                obj.gameObject.GetComponent<BoxCollider2D>().enabled = false;
            }
        }
    }
}
