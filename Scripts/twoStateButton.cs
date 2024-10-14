using UnityEngine;
using UnityEngine.Rendering;

public class twoStateButton : MonoBehaviour
{
    levelPlayer lvl;
    bool pressed = false;
    bool selfPressed = false;
    SpriteRenderer sr;
    Animator anim;
    twoState local;

    void Start()
    {
        anim = this.gameObject.GetComponent<Animator>();
        lvl = GameObject.FindWithTag("levelManager").GetComponent<levelPlayer>();
        sr = this.GetComponent<SpriteRenderer>();
        local = lvl.twoStateGlobal;
    }

    void OnTriggerEnter2D(Collider2D thing)
    {
        if ((thing.gameObject.tag == "goose" || thing.gameObject.tag == "egg") && !pressed)
        {
            pressed = true;
            selfPressed = true;
            lvl.twoStateGlobal = lvl.otherState();
        }
    }

    void OnTriggerExit2D(Collider2D thing)
    {
        if (thing.gameObject.tag == "goose" || thing.gameObject.tag == "egg")
        {
            selfPressed = false;
            pressed = false;
        }
    }

    void Update()
    {
        if (local != lvl.twoStateGlobal)
        {
            local = lvl.twoStateGlobal;
            press();
        }

        if(anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.999f && !selfPressed){
            pressed = false;
        } else {
            pressed = true;
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
