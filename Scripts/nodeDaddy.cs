using Unity.VisualScripting;
using UnityEngine;

[ExecuteInEditMode]
public class nodeDaddy : MonoBehaviour
{

    [SerializeField] GameObject linePrefab;
    [SerializeField] GameObject nodeModel;

    public void addNode(){
        GameObject newNode = new GameObject ("New Level Node", typeof(lvlNode));
        lvlNode nodeScript = newNode.GetComponent<lvlNode>();
            nodeScript.linePrefab = linePrefab;
        newNode.transform.parent = this.transform;
        newNode.name = "New Level Node";
        GameObject newNodeModel = Instantiate(nodeModel,  new Vector3 (), new Quaternion (), newNode.transform);
    }

}
