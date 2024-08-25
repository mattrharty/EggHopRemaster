using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{

    public Animator animate;
    public float speed = 1.00f;
    public float jumpHeight = 1;
    float direction = 0f;

    int grounded = 0;
    int jumpCooldown = 3;

    public int clipping = 0;

    Rigidbody2D rb;
    SpriteRenderer sr;

    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent(typeof (Rigidbody2D)) as Rigidbody2D;
        sr = this.GetComponent(typeof (SpriteRenderer)) as SpriteRenderer;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.R)){
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        direction = Input.GetAxis("Horizontal");

        if(Mathf.Abs(direction) > 0.2f){
            animate.SetBool("walking", true);
        } else {
            animate.SetBool("walking", false);
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

        Vector3 newPos = new Vector3(direction * (speed * 1.0f) * Time.deltaTime, 0, 0);
        transform.position += newPos;

        if(Input.GetKey(KeyCode.Space) && grounded > 0 && jumpCooldown <= 0){
            rb.velocity = transform.up * jumpHeight;
            jumpCooldown = 3;
        }

        jumpCooldown--;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.name == "Blocks"){
            grounded++;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if(other.gameObject.name == "Blocks"){
            grounded--;
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if(other.gameObject.name == "Blocks"){
            clipping++;
        }
    }

    void OnCollisionExit2D(Collision2D other)
    {
        if(other.gameObject.name == "Blocks"){
            clipping--;
        }
    }

}
