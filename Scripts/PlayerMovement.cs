using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{

    public Animator animate;
    public float speed = 1.00f;
    public float jumpHeight = 1;
    float direction = 0f;

    [Space]
    [Header("Input Actions")]
    public InputAction restart;
    public InputAction space;
    public InputAction eggHop;

    int jumpCooldown = 3;
    [SerializeField][Range(0f, 1f)] float stepUpHeight = 0.1f;

    [SerializeField] int terminalV = 25;

    public Animator ones;
    public Animator tens;

    public int clipping = 0;

    [SerializeField] LayerMask groundLayer;

    public List<Sprite> jumping;

    public GameObject egg;
    public Transform eggDaddy;
    public GameObject eggCounterDaddy;
    public Animator counterAnim;
    public int eggCount = -1;

    float moveTime = 0;

    [HideInInspector]
    public bool eggCooldown = true;

    public Rigidbody2D rb;
    public SpriteRenderer sr;

    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent(typeof (Rigidbody2D)) as Rigidbody2D;
        sr = this.GetComponent(typeof (SpriteRenderer)) as SpriteRenderer;

        restart.Enable();
        space.Enable();
        eggHop.Enable();

        space.performed += context => jump();
        eggHop.performed += context => layEgg();

        ones.SetTrigger("reset");
        tens.SetTrigger("reset");

        eggCooldown = true;
    }

    void layEgg(){
        if(eggCount > 0 && eggCooldown){
            counterAnim.SetTrigger("use");
            StartCoroutine(layCooldown());

            eggCount--;

            animate.enabled = true;
            animate.SetTrigger("lay");

            GameObject newEgg = Instantiate(egg, transform.position, new Quaternion(), eggDaddy);
            newEgg.GetComponent<Rigidbody2D>().linearVelocity = new Vector2 (0, -2);

            rb.linearVelocity = new Vector2 (rb.linearVelocity.x, jumpHeight * 0.82f);
        }
    }

    IEnumerator layCooldown(){
        eggCooldown = false;
        yield return new WaitForSecondsRealtime(1.25f);
        eggCooldown = true;
    }

    // Update is called once per frame
    void Update()
    {

        direction = GameObject.FindGameObjectWithTag("gameManager").GetComponent<gameManager>().axis.x;

        if(Mathf.Abs(direction) > 0.2f && Mathf.Abs(rb.linearVelocity.y) < 0.1f){
            animate.enabled = true;
            animate.SetBool("walking", true);
            moveTime += Time.deltaTime * Mathf.Abs(direction);
        } else if(eggCooldown){
            moveTime = 0;
            animate.SetBool("walking", false);
            if(rb.linearVelocity.y > 0.1f){
                animate.enabled = false;
                sr.sprite = jumping[0];
            } else if(rb.linearVelocity.y < -0.1f){
                animate.enabled = false;
                sr.sprite = jumping[1];
            } else {
                animate.enabled = true;
            }
        }

        if(direction < -0.1f){
            sr.flipX = true;
            this.GetComponentInChildren<BoxCollider2D>().offset = new Vector2 (0.05f, 0);
        } else  if (direction > 0.1f){
            sr.flipX = false;
            this.GetComponentInChildren<BoxCollider2D>().offset = new Vector2 (-0.05f, 0);
        }

        if(clipping > 0){
            //direction = 0;
        }

        //transform.position = transform.GetChild(0).transform.position;

        rb.linearVelocityX = 0;
        if(rb.linearVelocityX >= 0 && direction > 0){
            rb.linearVelocityX = Mathf.Clamp(rb.linearVelocityX, direction * speed * Mathf.Clamp(moveTime * 2, 1, 1.3f), 30);
        } else if (rb.linearVelocityX <= 0 && direction < 0){
            rb.linearVelocityX = Mathf.Clamp(rb.linearVelocityX, -30, direction * speed * Mathf.Clamp(moveTime * 2, 1, 1.3f));
        }
        
        rb.gravityScale = 2.3f;

        jumpCooldown--;

        rb.linearVelocity = new Vector2 (Mathf.Clamp(rb.linearVelocityX, -40, 40), Mathf.Clamp(rb.linearVelocityY, -1 * terminalV, terminalV));

        if(eggCount != -1){
            ones.SetInteger("eggCount", eggCount - Mathf.FloorToInt(eggCount / 10) * 10);
            tens.SetInteger("eggCount", Mathf.FloorToInt(eggCount) / 10);

            counterAnim.SetInteger("eggs", eggCount);
        }

        int dir = 1;
        if(sr.flipX){
            dir = -1;
        }

        RaycastHit2D stepUp = Physics2D.Raycast(transform.position + new Vector3 (0.345f * dir, 0.499f, 0), Vector2.down, 1f, groundLayer);
        if(1 - stepUp.distance <= stepUpHeight && !stepUp.collider.isTrigger && 1 - stepUp.distance > 0.01f && grounded() && stepUp.collider.transform.parent.GetComponent<PlatformEffector2D>() == null){
            transform.position += new Vector3 (0.05f * dir, 1 - stepUp.distance, 0);
        }
    }

    void jump(){
        if(grounded() && jumpCooldown <= 0 && Mathf.Abs(rb.linearVelocity.y) < 0.01f){
            rb.linearVelocityY += jumpHeight * (Mathf.Clamp(moveTime * 2, 1, 1.3f) / 1.3f);
            jumpCooldown = 3;
        }
    }

    public bool grounded(){
        if(Mathf.Abs(rb.linearVelocity.y) >  0.002f){
            return false;
        }
        for(int i = 0; i <= 10; i++){
            Vector3 startPos = new Vector3 (transform.position.x + 0.072f * i - 0.36f, transform.position.y, transform.position.z);
            RaycastHit2D raycast = Physics2D.Raycast(startPos, Vector2.down, 1f, groundLayer);
            if(raycast.collider != null){
                return true;
            }
        }
        return false;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if(other.gameObject.tag != "goose"){
            clipping++;
        }
    }

    void OnCollisionExit2D(Collision2D other)
    {
        if(other.gameObject.tag != "goose"){
            clipping--;
        }
    }

}
