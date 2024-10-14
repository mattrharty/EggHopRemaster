using UnityEngine;

public class twoStateLever : MonoBehaviour
{
    levelPlayer lvl;
    bool pressed = false;
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
        if (thing.gameObject.tag == "goose" || thing.gameObject.tag == "egg")
        {
            if (thing.gameObject.GetComponent<Rigidbody2D>().linearVelocity.x < 0)
            {
                if (lvl.twoStateGlobal != twoState.red)
                {
                    lvl.twoStateGlobal = twoState.red;
                    press();
                }
            }
            else
            {
                if (lvl.twoStateGlobal != twoState.blue)
                {
                    lvl.twoStateGlobal = twoState.blue;
                    press();
                }
            }
        }
    }

    void OnTriggerExit2D(Collider2D thing)
    {
        if (thing.gameObject.tag == "goose" || thing.gameObject.tag == "egg")
        {
            if (thing.gameObject.GetComponent<Rigidbody2D>().linearVelocity.x < 0)
            {
                if (lvl.twoStateGlobal != twoState.red)
                {
                    lvl.twoStateGlobal = twoState.red;
                    press();
                }
            }
            else
            {
                if (lvl.twoStateGlobal != twoState.blue)
                {
                    lvl.twoStateGlobal = twoState.blue;
                    press();
                }
            }
        }
    }

    void Update()
    {
        if (local != lvl.twoStateGlobal)
        {
            local = lvl.twoStateGlobal;
            press();
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
