using System.Collections;
using UnityEngine;

public class tempPlatform : MonoBehaviour
{
    Animator anim;
    Vector3 initPos;
    [SerializeField] float time = 2.5f;
    [SerializeField] float resetTime = 0f;
    [SerializeField] [Range(0, 2)] public int type;

    void Start()
    {
        anim = this.gameObject.GetComponent<Animator>();
        anim.enabled = false;
        anim.SetInteger("layer", type);
        initPos = transform.position;
    }

    void OnTriggerEnter2D(Collider2D thing)
    {
        if (thing.gameObject.tag == "goose" || thing.gameObject.tag == "egg")
        {
            float initTime = Time.fixedTime;
            Vector3 basePos = transform.GetChild(0).position;
            
            StartCoroutine(shake(initTime, basePos));
        }
    }

    IEnumerator shake (float initTime, Vector3 basePos){
        while (Time.fixedTime - initTime < time)
        {
            float shake = 0.015f * Mathf.Sin(36 * (Time.fixedTime - initTime));
            transform.GetChild(0).position = new Vector3(basePos.x, basePos.y + shake, basePos.z);
            yield return new WaitForEndOfFrame();
        }

        transform.GetChild(0).position = basePos;
        anim.enabled = true;
        anim.SetTrigger("fall");
        StartCoroutine(fall());
    }

    IEnumerator fall()
    {
        yield return new WaitUntil(() => transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().color.a == 0);
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
