using UnityEngine;

[ExecuteAlways]
public class UIRect : MonoBehaviour
{

    Transform upper;
    Transform lower;

    Vector2 lastSize;

    void Awake(){
        upper = transform.GetChild(0);
        lower = transform.GetChild(1);
    }

    void Update(){
        if(this.GetComponent<RectTransform>().sizeDelta != lastSize){
            this.GetComponent<RectTransform>().sizeDelta = lastSize;
        }
    }

}
