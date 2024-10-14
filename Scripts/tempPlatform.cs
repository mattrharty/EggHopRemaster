using System.Collections;
using UnityEngine;

public class tempPlatform : MonoBehaviour
{
    Animator anim;
    Vector3 initPos;
    [SerializeField] float time = 2.5f;
    [SerializeField] float resetTime = 15f;
    [SerializeField] [Range(0, 2)] int type;

    void Start()
    {
        anim = this.gameObject.GetComponent<Animator>();
        anim.SetInteger("layer", type);
        initPos = transform.position;
    }

    void OnTriggerEnter2D(Collider2D thing)
    {
        if (thing.gameObject.tag == "goose" || thing.gameObject.tag == "egg")
        {
            float initTime = Time.fixedTime;
            Vector3 basePos = transform.GetChild(0).position;
            while (Time.fixedTime - initTime < time)
            {
                float shake = 0.2f * Mathf.Sin(5 * (Time.fixedTime - initTime));
                transform.GetChild(0).position = new Vector3(basePos.x, basePos.y + shake, basePos.z);
            }
            transform.GetChild(0).position = basePos;
            anim.SetTrigger("fall");
            StartCoroutine(fall());
        }
    }

    IEnumerator fall()
    {
        yield return new WaitUntil(() => transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().color.a == 0);
        this.GetComponent<BoxCollider2D>().enabled = false;
        transform.position = initPos;
        if (resetTime > 0)
        {
            float initTime = Time.fixedTime;
            yield return new WaitForSeconds(resetTime);
            reset();
        }
    }

    void reset()
    {
        transform.position = initPos;
        this.GetComponent<BoxCollider2D>().enabled = true;
        transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
    }
}
