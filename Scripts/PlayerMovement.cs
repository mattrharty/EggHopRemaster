using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{

    public Animator animate;
    public float speed = 1.00f;
    public float jumpHeight = 1;
    float direction = 0f;
    //public InputAction move;
    public InputAction restart;
    public InputAction space;
    public InputAction eggHop;

    public int grounded = 0;
    int jumpCooldown = 3;

    public Animator ones;
    public Animator tens;

    public int clipping = 0;

    public List<Sprite> jumping;

    public GameObject egg;
    public Transform eggDaddy;
    public int eggCount = -1;

    public Rigidbody2D rb;
    SpriteRenderer sr;

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
    }

    void layEgg(){
        if(eggCount > 0){
            eggCount--;

            GameObject newEgg = Instantiate(egg, transform.position, new Quaternion(), eggDaddy);
            newEgg.GetComponent<Rigidbody2D>().velocity = new Vector2 (0, -2);

            rb.velocity = new Vector2 (rb.velocity.x, jumpHeight * 0.82f);
        }
    }

    // Update is called once per frame
    void Update()
    {

        direction = GameObject.FindGameObjectWithTag("gameManager").GetComponent<gameManager>().axis.x;

        if(Mathf.Abs(direction) > 0.2f && Mathf.Abs(rb.velocity.y) < 0.1f){
            animate.enabled = true;
            animate.SetBool("walking", true);
        } else {
            animate.SetBool("walking", false);
            if(rb.velocity.y > 0.1f){
                animate.enabled = false;
                sr.sprite = jumping[0];
            } else if(rb.velocity.y < -0.1f){
                animate.enabled = false;
                sr.sprite = jumping[1];
            } else {
                animate.enabled = true;
            }
        }

        if(direction < -0.1f){
            sr.flipX = true;
        } else  if (direction > 0.1f){
            sr.flipX = false;
        }

        if(clipping > 0){
            //direction = 0;
        }

        //transform.position = transform.GetChild(0).transform.position;

        Vector3 newPos = new Vector3(direction * speed, rb.velocity.y, 0);
        rb.velocity = newPos;
        rb.gravityScale = 2.3f;

        jumpCooldown--;

        rb.velocity = new Vector2 (Mathf.Clamp(rb.velocity.x, -40, 40), Mathf.Clamp(rb.velocity.y, -25, 40));

        if(eggCount != -1){
            ones.SetInteger("eggCount", eggCount - Mathf.FloorToInt(eggCount / 10) * 10);
            tens.SetInteger("eggCount", Mathf.FloorToInt(eggCount) / 10);
        }
    }

    void jump(){
        if(grounded > 0 && jumpCooldown <= 0 && Mathf.Abs(rb.velocity.y) < 0.01f){
            rb.velocity = transform.up * jumpHeight;
            jumpCooldown = 3;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.tag != "goose"){
            grounded++;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if(other.gameObject.tag != "goose"){
            grounded--;
        }
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
