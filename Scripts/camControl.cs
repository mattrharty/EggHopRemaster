using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class camControl : MonoBehaviour
{

    public Transform cam;

    [SerializeField]
    InputAction moveCam;
    [SerializeField]
    InputAction zooom;
    [SerializeField]
    public InputAction mousePos;
    public InputAction rightClick;
    public InputAction mouseMove;

    public float zoom = 0f;
    public float zoomScale = 1.1f;

    public float lastX = 0;
    public float lastY = 0;
    bool dragging = false;
    [SerializeField]
    float diffX = 0;
    [SerializeField]
    float diffY = 0;

    [Range(1f, 36f)][SerializeField]
    float camSpeed = 1.0f;
    [Range(0.01f, 1.0f)][SerializeField]
    float mouseCamSpeed = 0.5f;
    public List<int> bounds;

    GameObject newDaddy;

    // Start is called before the first frame update
    void Start()
    {
        moveCam.Enable();
        zooom.Enable();
        mousePos.Enable();
        rightClick.Enable();
        mouseMove.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        //Moves the camera
        Vector3 newPos = new Vector3();
        if(!rightClick.IsPressed()){
            dragging = false;
            newPos = new Vector3 (
                camSpeed * Time.deltaTime * moveCam.ReadValue<Vector2>().x,
                camSpeed * Time.deltaTime * moveCam.ReadValue<Vector2>().y,
                0); 
        } else {
            diffX = 0f;
            diffY = 0f;
            if(dragging){
                diffX = cam.GetComponent<Camera>().ScreenToWorldPoint(mousePos.ReadValue<Vector2>()).x - cam.GetComponent<Camera>().ScreenToWorldPoint(new Vector3 (lastX, 0, 0)).x;
                diffY = cam.GetComponent<Camera>().ScreenToWorldPoint(mousePos.ReadValue<Vector2>()).y - cam.GetComponent<Camera>().ScreenToWorldPoint(new Vector3 (0, lastY, 0)).y;
            }

            if(Mathf.Abs(mouseMove.ReadValue<Vector2>().x) < 0.05f){
                diffX = 0;
            } if (Mathf.Abs(mouseMove.ReadValue<Vector2>().y) < 0.05f){
                diffY = 0;
            }

            lastX = cam.GetComponent<Camera>().ScreenToWorldPoint(mousePos.ReadValue<Vector2>()).x;
            lastX = mousePos.ReadValue<Vector2>().x;
            lastY = cam.GetComponent<Camera>().ScreenToWorldPoint(mousePos.ReadValue<Vector2>()).y;
            lastY = mousePos.ReadValue<Vector2>().y;

            newPos = new Vector3 (diffX * -1, diffY * -1, 0);

            dragging = true;
        }
        cam.position += newPos;

        //Zooms in and out the camera
        zoom += zooom.ReadValue<float>();
        zoom = Mathf.Clamp(zoom, -1000, 3600);
        cam.GetComponent<Camera>().orthographicSize = Mathf.Pow(zoomScale, zoom);

        //Checks if the cam is out of bounds and fixes it
        if(cam.position.x > bounds[1]){
            cam.position = new Vector3 (bounds[1], cam.position.y, -10);
        } else if(cam.position.x < bounds[3]){
            cam.position = new Vector3 (bounds[3], cam.position.y, -10);
        }if(cam.position.y > bounds[0]){
            cam.position = new Vector3 (cam.position.x, bounds[0], -10);
        } else if(cam.position.y < bounds[2]){
            cam.position = new Vector3 (cam.position.x, bounds[2], -10);
        }
    }
}
