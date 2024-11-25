using UnityEngine;
using System;

[ExecuteInEditMode]
public class lvlNode : MonoBehaviour
{

    [SerializeField] public  bool defaultNode;
    bool defaultOther;

    public float interval = 0.1f;
    float intervalOther = 0.1f;

    [Range(-2.5f, 2.5f)]
    public float curve;
    float curveOther;

    public Vector4 camOffset;

    [SerializeField]public Transform nextNode;
    Vector3 nextNodeOther;

    public string levelName;
    public string displayName;

    Vector3 thisNodeOther;

    [SerializeField]public fourPt backInput;

    [SerializeField]public fourPt nextInput;

    [SerializeField] public bool secretExit;
    bool secretExitOther;
    [HideInInspector] public Transform secretLvl;
    Transform secretLvlOther;
    [HideInInspector] public fourPt secretInput;

    public GameObject lineDaddy;
    [HideInInspector]public GameObject linePrefab;

    void Awake()
    {
        this.tag = "node";
    }

    public void resetLine(){
        if(interval < 0.05f || nextNode == null){
            return;
        }
        if(lineDaddy != null){
            GameObject.DestroyImmediate(lineDaddy);
        }
        while(GameObject.Find("Line " + this.name) != null){
            GameObject.DestroyImmediate(GameObject.Find("Line " + this.name));
        }
        lineDaddy = new GameObject();
        lineDaddy.name = "Line " + this.name;
        lineDaddy.transform.position = transform.position;
        for(float i = -0.9f; i < 0; i += interval){
            GameObject newObj = Instantiate(linePrefab, createLine(i), new Quaternion(), lineDaddy.transform);
            //newObj.transform.localPosition = createLine(i);
        }
        int n = 0;
        if(transform.position.z - nextNode.position.z > 0){
            n = 1;
        } else {
            n = 0;
        }
        Vector3 newRot = new Vector3 (0, (float)Math.Atan((transform.position.x - nextNode.position.x) / (transform.position.z - nextNode.position.z)) * (180 / (float)Math.PI) + 90 + 180 * n, 0);
        lineDaddy.transform.Rotate(newRot);
        //Debug.Log(lineDaddy.transform.rotation);
    }

    Vector3 createLine(float t){
        Vector3 starting = new Vector3 (transform.position.x, transform.position.y, transform.position.z);
        Vector3 slope = new Vector3 (transform.position.x - nextNode.position.x, transform.position.y - nextNode.position.y, transform.position.z - nextNode.position.z);
        slope = new Vector3 (Mathf.Pow(Mathf.Pow(slope.x, 2) + Mathf.Pow(slope.z, 2), 0.5f), slope.y, 0);
        Vector3 finalVector = starting + t * slope;
        slope = new Vector3 (transform.position.x - nextNode.position.x, transform.position.y - nextNode.position.y, transform.position.z - nextNode.position.z);
        //Debug.Log((Mathf.Pow(Mathf.Pow(slope.x, 2) + Mathf.Pow(slope.z, 2), 0.5f) / 2 + transform.position.x) + "\n" + Mathf.Pow((Mathf.Pow(Mathf.Pow(transform.position.x - nextNode.position.x, 2) + Mathf.Pow(transform.position.z - nextNode.position.z, 2), 0.25f) * (curve * -1) - t) / (curve * -1), 0.5f));
        //return finalVector;
        int i = 1;
        if(transform.position.x < nextNode.position.x){
            i = 1;
        } else {
            i = 1;
        }
        return new Vector3
        (finalVector.x
        , finalVector.y
        , curve * 0.1f * Mathf.Pow(finalVector.x + ((Mathf.Pow(Mathf.Pow(slope.x, 2) + Mathf.Pow(slope.z, 2), 0.5f) / (2 * i)) - transform.position.x), 2) + Mathf.Pow(Mathf.Pow(Mathf.Pow(slope.x, 2) + Mathf.Pow(slope.z, 2), 0.5f) / 2, 2f) * -1 * (curve * 0.1f) + transform.position.z);
    }

    public void destroy(){
        GameObject.DestroyImmediate(lineDaddy);
        GameObject.DestroyImmediate(this.gameObject);
    }

    void Update(){
        if(defaultOther != defaultNode){
            //Debug.Log(defaultNode);
            if(defaultNode){
                foreach(GameObject node in GameObject.FindGameObjectsWithTag("node")){
                    if(node.gameObject != this.gameObject){
                        node.GetComponent<lvlNode>().defaultNode = false;
                    }
                }
            }
            defaultOther = defaultNode;
        }
        if(secretLvl != null){
            if (secretLvl.position != secretLvlOther.position){
                secretLvlOther.position = secretLvl.position;
            }
        }
        if(nextNode == null){
            return;
        }
        if(interval != intervalOther || nextNode.position != nextNodeOther || secretExit != secretExitOther || thisNodeOther != transform.position || curveOther != curve){
            intervalOther = interval;
            nextNodeOther = nextNode.position;
            secretExitOther = secretExit;
            thisNodeOther = transform.position;
            curveOther = curve;
            resetLine();
        }

        if(camOffset.x < 0){
            camOffset += new Vector4(360, 0, 0, 0);
        }
        if(camOffset.y < 0){
            camOffset += new Vector4(360, 0, 0, 0);
        }
        if(camOffset.x > 359){
            camOffset -= new Vector4(360, 0, 0, 0);
        }
        if(camOffset.y > 89){
            camOffset -= new Vector4(90, 0, 0, 0);
        }
    }
}

public enum fourPt {
    up,
    down,
    left,
    right
}

public class direction{
    public enum eightPt {
        north,
        northeast,
        east,
        southeast,
        south,
        southwest,
        west,
        northwest
    }

    public static coordinate2D getVector(eightPt input){
        if(input == eightPt.north){
            return new coordinate2D(0, 1);
        } else
        if(input == eightPt.northeast){
            return new coordinate2D(1, 1);
        } else
        if(input == eightPt.east){
            return new coordinate2D(1, 0);
        } else
        if(input == eightPt.southeast){
            return new coordinate2D(1, -1);
        } else
        if(input == eightPt.south){
            return new coordinate2D(0, -1);
        } else
        if(input == eightPt.southwest){
            return new coordinate2D(-1, -1);
        } else
        if(input == eightPt.west){
            return new coordinate2D(-1, 0);
        } else
        if(input == eightPt.northwest){
            return new coordinate2D(-1, 1);
        }
        return new coordinate2D(0, 0);
    }

    public static eightPt setVector(coordinate2D input){
        if(input == new coordinate2D(0, 1)){
            return eightPt.north;
        } else
        if(input == new coordinate2D(1, 1)){
            return eightPt.northeast;
        } else
        if(input == new coordinate2D(1, 0)){
            return eightPt.east;
        } else
        if(input == new coordinate2D(1, -1)){
            return eightPt.southeast;
        } else
        if(input == new coordinate2D(0, -1)){
            return eightPt.south;
        } else
        if(input == new coordinate2D(-1, -1)){
            return eightPt.southwest;
        } else
        if(input == new coordinate2D(-1, 0)){
            return eightPt.west;
        } else
        if(input == new coordinate2D(-1, 1)){
            return eightPt.northwest;
        }
        return eightPt.north;
    }
}
