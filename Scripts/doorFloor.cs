using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class doorFloor : MonoBehaviour
{

    public Sprite[] states;
    bool done = false;

    // Update is called once per frame
    void Update()
    {
        if(buttonDoor.pressed && !done){
            done = true;
            this.GetComponent<SpriteRenderer>().sprite = states[1];
        }
    }
}
