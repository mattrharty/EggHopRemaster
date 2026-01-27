using UnityEngine;
using UnityEditor;

class center : MonoBehaviour
{

    [SerializeField]float offsetMult = 192f;
    [SerializeField] float speed = 1f;
    float time = 0;


    public void centerToParent()
    {
        transform.localPosition = transform.parent.localPosition * -1;
    }


    void Update()
    {
        transform.localPosition = transform.parent.localPosition * -1 + new Vector3 (offsetMult * ((time += 0.016666f) * speed % 100 / 100), offsetMult * ((time += 0.016666f) * speed % 100 / 100), 0);
        transform.localRotation = new Quaternion (transform.parent.parent.rotation.x * -1, transform.parent.parent.rotation.y * -1, transform.parent.parent.rotation.z * -1, 0);
    }
}
