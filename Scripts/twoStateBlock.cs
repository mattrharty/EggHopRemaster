using UnityEngine;

public class twoStateBlock : MonoBehaviour
{
    BoxCollider2D collide;
    SpriteRenderer sr;
    levelPlayer lvl;
    [SerializeField] twoState type;
    twoState local;
    [SerializeField] Sprite[] sprites;

    void Start()
    {
        collide = this.gameObject.GetComponent<BoxCollider2D>();
        sr = this.gameObject.GetComponent<SpriteRenderer>();
        lvl = GameObject.FindWithTag("levelManager").GetComponent<levelPlayer>();
        local = lvl.twoStateGlobal;
        setSprite();
    }

    void setSprite()
    {
        if (lvl.twoStateGlobal == type)
        {
            sr.sprite = sprites[1];
            collide.enabled = true;
        }
        else
        {
            sr.sprite = sprites[0];
            collide.enabled = false;
        }
    }

    void Update()
    {
        if (local != lvl.twoStateGlobal)
        {
            this.GetComponent<flash>().bling();
            local = lvl.twoStateGlobal;
            setSprite();
        }
    }
}
