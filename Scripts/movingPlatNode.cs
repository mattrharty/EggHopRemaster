using System;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class movingPlatNode : MonoBehaviour
{

    public movingPlatAttr attributes;
    [SerializeField] bool editor = false;
    LineRenderer line;
    movingPlatAttr[] connectedNodes;
    public levelData currentLvl;

    public void Start(){
        if(editor){
            return;
        }
        //Creates line
        line = this.gameObject.GetComponent<LineRenderer>();
        coordinate2D dir = direction.getVector(attributes.forwardDir);
        line.startColor = Color.red;
        line.endColor = Color.red;
        line.startWidth = 0.1f;
        line.SetPosition(0, transform.position);
        line.SetPosition(1, new Vector3(dir.x, dir.y, 0) + transform.position);
    }

    public void Update(){
        if(editor){
            if(connectedNodes.Length == 0){
                for(int i = 0; i < 9; i++){
                    if(currentLvl.getTile(i - Mathf.FloorToInt(i / 3) * 3 - 1, Mathf.FloorToInt(i / 3) - 1).type == blockType.movingNode && i != 4){
                        int x = i - Mathf.FloorToInt(i / 3) * 3 - 1;
                        int y = Mathf.FloorToInt(i / 3) - 1;
                        if(x == 1 || (x == 0 && y == 1)){
                            attributes.forwardDir = direction.setVector(new coordinate2D(x, y));
                        } else
                        if(x == -1 || (x == 0 && y == -1)){
                            attributes.forwardDir = direction.setVector(new coordinate2D(x, y));
                        }
                    }
                }
            }
        }
    }

}

[Serializable]
public enum triggerType{
    none,
    player,
    twoState
}

[Serializable] 
public class movingPlatAttr {
    public float speed = 1.0f;
    public triggerType waitTrigger = triggerType.none;
    public direction.eightPt forwardDir;
    public direction.eightPt backwardDir;
}