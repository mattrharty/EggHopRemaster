using UnityEngine;

[ExecuteAlways]
public class sizeMatcher : MonoBehaviour
{

    [SerializeField] RectTransform other;

    void Update(){
        this.GetComponent<RectTransform>().sizeDelta = other.sizeDelta;
    }

}
