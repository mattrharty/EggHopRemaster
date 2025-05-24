using System.Collections.Generic;
using UnityEngine;

public class twoStateLever : MonoBehaviour
{
    levelPlayer lvl;
    bool pressed = false;
    SpriteRenderer sr;
    Animator anim;
    twoState local;
    Dictionary<Transform, bool> primed;

    void Start()
    {
        anim = this.gameObject.GetComponent<Animator>();
        lvl = GameObject.FindWithTag("levelManager").GetComponent<levelPlayer>();
        sr = this.GetComponent<SpriteRenderer>();
        local = lvl.twoStateGlobal;

        primed = new Dictionary<Transform, bool>();
    }

    void OnTriggerEnter2D(Collider2D thing)
    {
        if (thing.gameObject.tag == "goose" || thing.gameObject.tag == "egg")
        {
            if(thing.transform.position.x <= transform.position.x){
                primed.Add(thing.transform, false);
            } else if(thing.transform.position.x > transform.position.x){
                primed.Add(thing.transform, true);
            }
        }
    }

    void Update(){
        if (local != lvl.twoStateGlobal)
        {
            local = lvl.twoStateGlobal;
            press();
        }

        if(primed.Count == 0){
            return;
        }

        List<Transform> swaps = new List<Transform>();

        foreach(KeyValuePair<Transform, bool> obj in primed){
            if(obj.Value){
                if(obj.Key.position.x < transform.position.x){
                    if (lvl.twoStateGlobal != twoState.red)
                    {
                        lvl.twoStateGlobal = twoState.red;
                        press();
                    }
                    swaps.Add(obj.Key);
                }
            } else {
                if(obj.Key.position.x > transform.position.x){
                    if (lvl.twoStateGlobal != twoState.blue)
                    {
                        lvl.twoStateGlobal = twoState.blue;
                        press();
                    }
                    swaps.Add(obj.Key);
                }
            }
        }

        foreach(Transform obj in swaps){
            primed[obj] = !primed[obj];
        }
    }

    void OnTriggerExit2D(Collider2D thing)
    {
        if (thing.gameObject.tag == "goose" || thing.gameObject.tag == "egg")
        {
            if(thing.transform.position.x <= transform.position.x){
                primed.Remove(thing.transform);
            } else if(thing.transform.position.x > transform.position.x){
                primed.Remove(thing.transform);
            }
        }
    }

    void press(){
        if(lvl.twoStateGlobal == twoState.red){
            anim.SetBool("red", true);
        } else {
            anim.SetBool("red", false);
        }
    }
}
