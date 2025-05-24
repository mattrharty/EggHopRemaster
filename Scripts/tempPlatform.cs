using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tempPlatform : MonoBehaviour
{
    Animator anim;
    Vector3 initPos;
    bool fallin = false;
    [SerializeField] float time = 2.5f;
    [SerializeField] float resetTime = 0f;
    [SerializeField] [Range(0, 2)] public int type;
    [SerializeField] List<Sprite> sprites0;
    [SerializeField] List<Sprite> sprites1;
    [SerializeField] List<Sprite> sprites2;

    void Start()
    {
        anim = this.gameObject.GetComponent<Animator>();
        anim.enabled = false;
        anim.SetInteger("layer", type);
        initPos = transform.position;
    }

    void OnTriggerEnter2D(Collider2D thing)
    {
        if ((thing.gameObject.tag == "goose" || thing.gameObject.tag == "egg") && !fallin)
        {
            fallin = true;
            Vector3 basePos = transform.GetChild(0).position;
            
            StartCoroutine(shake(Time.fixedTime, basePos));
        }
    }

    IEnumerator shake (float initTime, Vector3 basePos){
        for(int i = 1; i < 5; i++){
            while (Time.fixedTime - initTime < time * i / 4)
            {
                float shake = 0.025f * Mathf.Sin(36 * (Time.fixedTime - initTime));
                transform.GetChild(3).position = new Vector3(basePos.x, basePos.y + shake, basePos.z);
                yield return new WaitForEndOfFrame();
            }
            transform.GetChild(i - 1).GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
            transform.GetChild(3).GetComponent<SpriteRenderer>().sprite = spriteSelect(i);
            if(i == 4){
                transform.GetChild(i - 1).GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
                transform.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
            }
        }

        /*transform.GetChild(0).position = basePos;
        anim.SetTrigger("fall");
        anim.enabled = true;
        StartCoroutine(fall());*/
    }

    public Sprite spriteSelect(int i){
        if(type == 0){
            return sprites0[i];
        } else if(type == 1){
            return sprites1[i];
        } else if(type == 2){
            return sprites2[i];
        }
        return sprites0[i];
    }

    IEnumerator fall()
    {
        Debug.Log("falling");
        yield return new WaitUntil(() => transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().color.a <= 0.1f);
        anim.enabled = false;
        transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
        foreach(BoxCollider2D box in this.GetComponents<BoxCollider2D>()){
            box.enabled = false;
        }
        transform.position = initPos;
        if (resetTime > 0.1f)
        {
            Debug.Log("resetting");
            float initTime = Time.fixedTime;
            yield return new WaitForSeconds(resetTime);
            blockReset();
        }
    }

    void blockReset()
    {
        transform.position = initPos;
        foreach(BoxCollider2D box in this.GetComponents<BoxCollider2D>()){
            box.enabled = false;
        }
        transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
    }
}
