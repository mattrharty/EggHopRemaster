using UnityEngine;

public class renderCheck : MonoBehaviour
{

    SpriteRenderer render;

    void Start(){
        render = GetComponent<SpriteRenderer>();
    }

    void OnBecameInvisible()
    {
        render.enabled = false;
    }

    void OnBecameVisible()
    {
        render.enabled = true;
    }
}
