using System;
using System.Linq.Expressions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using System.Windows.Forms;

public class mapController : MonoBehaviour
{

    public Transform nodeDaddy;
    public Transform cam;
    lvlNode defaultNode;
    public List<lvlNode> nodes;
    int currentIndex = 0;
    lvlNode loadingNode;
    lvlNode currentNode;

    public Transform goos;

    public InputActionMap directionalInput;

    public Animator goosAnim;
    [SerializeField] AnimationClip mapWalking;

    void Awake(){
        directionalInput.Enable();

        directionalInput.FindAction("up").performed += context => input(fourPt.up);
        directionalInput.FindAction("down").performed += context => input(fourPt.down);
        directionalInput.FindAction("right").performed += context => input(fourPt.right);
        directionalInput.FindAction("left").performed += context => input(fourPt.left);

        goosAnim = goos.gameObject.GetComponent<Animator>();
        mapWalking.SetCurve("", typeof(Transform), "localPosition.x", new AnimationCurve());
        mapWalking.SetCurve("", typeof(Transform), "localPosition.y", new AnimationCurve());
        mapWalking.SetCurve("", typeof(Transform), "localPosition.z", new AnimationCurve());
    }

    void input(fourPt dir){
        if(currentNode.backInput == dir && currentIndex > 0){
            currentIndex--;
            currentNode = nodes[currentIndex];

            AnimationCurve newCurve = new AnimationCurve();
            AnimationCurve rotCurve = new AnimationCurve();
            newCurve.AddKey(1f, currentNode.transform.position.x);
            newCurve.AddKey(0, currentNode.nextNode.transform.position.x);
            for(int i = 0; i < currentNode.lineDaddy.transform.childCount; i++){
                newCurve.AddKey(0.1f * i + 0.1f, currentNode.lineDaddy.transform.GetChild(i).transform.position.x);
                if(Mathf.Abs(currentNode.camOffset.x - cam.localEulerAngles.x) <= Mathf.Abs(currentNode.camOffset.x - 360 - cam.localEulerAngles.x)){
                    rotCurve.AddKey(i * 0.1f, cam.localEulerAngles.x + (currentNode.camOffset.x - cam.localEulerAngles.x) * (i * 0.1f));
                } else {
                    rotCurve.AddKey(i * 0.1f, cam.localEulerAngles.x + (currentNode.camOffset.x - 360 - cam.localEulerAngles.x) * (i * 0.1f));
                }
            }
            rotCurve.AddKey(0.9f, cam.localEulerAngles.x + (currentNode.camOffset.x - cam.localEulerAngles.x) * (1));
            mapWalking.SetCurve("", typeof(Transform), "localPosition.x", newCurve);
            mapWalking.SetCurve("Cam", typeof(Transform), "localEulerAngles.x", rotCurve);
            newCurve = new AnimationCurve();
            rotCurve = new AnimationCurve();
            newCurve.AddKey(1f, currentNode.transform.position.y);
            newCurve.AddKey(0, currentNode.nextNode.transform.position.y);
            for(int i = 0; i < currentNode.lineDaddy.transform.childCount; i++){
                newCurve.AddKey(0.1f * i + 0.1f, currentNode.lineDaddy.transform.GetChild(i).transform.position.y);
                //rotCurve.AddKey(i * 0.1f, goos.eulerAngles.y + (currentNode.camOffset.y - goos.eulerAngles.y) * (i * 0.1f));
            }
            rotCurve.AddKey(0f, goos.eulerAngles.y);
            rotCurve.AddKey(0.9f, currentNode.camOffset.y);
            mapWalking.SetCurve("", typeof(Transform), "localPosition.y", newCurve);
            mapWalking.SetCurve("", typeof(Transform), "localEulerAngles.y", rotCurve);
            newCurve = new AnimationCurve();
            rotCurve = new AnimationCurve();
            newCurve.AddKey(1f, currentNode.transform.position.z);
            newCurve.AddKey(0, currentNode.nextNode.transform.position.z);
            for(int i = 0; i < currentNode.lineDaddy.transform.childCount; i++){
                newCurve.AddKey(0.1f * i + 0.1f, currentNode.lineDaddy.transform.GetChild(i).transform.position.z);
                //rotCurve.AddKey(i * 0.1f, cam.localEulerAngles.z + (currentNode.camOffset.w - cam.localEulerAngles.z) * (i * 0.1f));
            }
            mapWalking.SetCurve("", typeof(Transform), "localPosition.z", newCurve);
            rotCurve.AddKey(0, cam.GetChild(0).transform.localPosition.z);
            rotCurve.AddKey(1, currentNode.camOffset.w);
            mapWalking.SetCurve("Cam/Main Camera", typeof(Transform), "localPosition.z", rotCurve);

            walk();

            goos.localPosition = nodes[currentIndex].transform.position; 
            goos.localEulerAngles = new Vector3 (0, currentNode.camOffset.y, 0);
            cam.localEulerAngles = new Vector3 (currentNode.camOffset.x, 0, 0);
            cam.GetChild(0).transform.localPosition = new Vector3 (0, 0, currentNode.camOffset.w);
        } else if (currentNode.nextInput == dir && currentIndex < nodes.Count - 1){
            currentIndex++;

            AnimationCurve newCurve = new AnimationCurve();
            AnimationCurve rotCurve = new AnimationCurve();
            newCurve.AddKey(0, currentNode.transform.position.x);
            newCurve.AddKey(1f, currentNode.nextNode.transform.position.x);
            for(int i = 0; i < currentNode.lineDaddy.transform.childCount; i++){
                newCurve.AddKey(1 - 0.1f * i - 0.1f, currentNode.lineDaddy.transform.GetChild(i).transform.position.x);
                if(Mathf.Abs(currentNode.nextNode.GetComponent<lvlNode>().camOffset.x - cam.localEulerAngles.x) <= Mathf.Abs(currentNode.nextNode.GetComponent<lvlNode>().camOffset.x - 360 - cam.localEulerAngles.x)){
                    rotCurve.AddKey(i * 0.1f, cam.localEulerAngles.x + (currentNode.nextNode.GetComponent<lvlNode>().camOffset.x - cam.localEulerAngles.x) * (i * 0.1f));
                } else {
                    rotCurve.AddKey(i * 0.1f, cam.localEulerAngles.x + (currentNode.nextNode.GetComponent<lvlNode>().camOffset.x - 360 - cam.localEulerAngles.x) * (i * 0.1f));
                }
            }
            rotCurve.AddKey(0.9f, cam.localEulerAngles.x + (currentNode.nextNode.GetComponent<lvlNode>().camOffset.x - cam.localEulerAngles.x) * (1));
            mapWalking.SetCurve("", typeof(Transform), "localPosition.x", newCurve);
            mapWalking.SetCurve("Cam", typeof(Transform), "localEulerAngles.x", rotCurve);
            newCurve = new AnimationCurve();
            rotCurve = new AnimationCurve();
            newCurve.AddKey(0, currentNode.transform.position.y);
            newCurve.AddKey(1f, currentNode.nextNode.transform.position.y);
            for(int i = 0; i < currentNode.lineDaddy.transform.childCount; i++){
                newCurve.AddKey(1 - 0.1f * i - 0.1f, currentNode.lineDaddy.transform.GetChild(i).transform.position.y);
                //rotCurve.AddKey(i * 0.1f, goos.eulerAngles.y + (currentNode.nextNode.GetComponent<lvlNode>().camOffset.y - goos.eulerAngles.y) * (i * 0.1f));
            }
            rotCurve.AddKey(0f, goos.eulerAngles.y);
            rotCurve.AddKey(0.9f, currentNode.nextNode.GetComponent<lvlNode>().camOffset.y);
            mapWalking.SetCurve("", typeof(Transform), "localPosition.y", newCurve);
            mapWalking.SetCurve("", typeof(Transform), "localEulerAngles.y", rotCurve);
            newCurve = new AnimationCurve();
            rotCurve = new AnimationCurve();
            newCurve.AddKey(0, currentNode.transform.position.z);
            newCurve.AddKey(1f, currentNode.nextNode.transform.position.z);
            for(int i = 0; i < currentNode.lineDaddy.transform.childCount; i++){
                newCurve.AddKey(1 - 0.1f * i - 0.1f, currentNode.lineDaddy.transform.GetChild(i).transform.position.z);
                //rotCurve.AddKey(i * 0.1f, goos.eulerAngles.y + (currentNode.camOffset.y - goos.eulerAngles.y) * (i * 0.1f));
            }
            mapWalking.SetCurve("", typeof(Transform), "localPosition.z", newCurve);
            rotCurve.AddKey(0, cam.GetChild(0).transform.localPosition.z);
            rotCurve.AddKey(1, currentNode.nextNode.GetComponent<lvlNode>().camOffset.w);
            mapWalking.SetCurve("Cam/Main Camera", typeof(Transform), "localPosition.z", rotCurve);

            walk();

            currentNode = nodes[currentIndex];
            goos.localPosition = nodes[currentIndex].transform.position;            
            goos.localEulerAngles = new Vector3 (0, currentNode.camOffset.y, 0);
            cam.localEulerAngles = new Vector3 (currentNode.camOffset.x, 0, 0);
            cam.GetChild(0).transform.localPosition = new Vector3 (0, 0, currentNode.camOffset.w);
        }
    }

    void walk(){
        //goosAnim.SetTrigger("walkTrigger");
        goosAnim.SetBool("test", true);
        goosAnim.SetTrigger("walkTrigger");
    }

    // Start is called before the first frame update
    void Start()
    {
        foreach(Transform node in nodeDaddy){
            if(node.gameObject.GetComponent<lvlNode>().defaultNode){
                defaultNode = node.gameObject.GetComponent<lvlNode>();
                loadingNode = defaultNode;
                currentNode = defaultNode;
                goos.position = defaultNode.gameObject.transform.position;
            }
        }
        if(defaultNode == null){
            defaultNode = nodeDaddy.GetChild(0).GetComponent<lvlNode>();
            loadingNode = defaultNode;
            currentNode = defaultNode;
            goos.localPosition = defaultNode.gameObject.transform.localPosition;
        }
        nodes.Add(loadingNode);
        while(loadingNode.nextNode != null){
            nodes.Add(loadingNode.nextNode.gameObject.GetComponent<lvlNode>());
            loadingNode = loadingNode.nextNode.gameObject.GetComponent<lvlNode>();
        }
        goos.localEulerAngles = new Vector3 (0, currentNode.camOffset.y, 0);
        cam.localEulerAngles = new Vector3 (currentNode.camOffset.x, 0, 0);
        cam.GetChild(0).transform.localPosition = new Vector3 (0, 0, currentNode.camOffset.w);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
