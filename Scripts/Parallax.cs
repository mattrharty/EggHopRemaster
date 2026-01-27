using UnityEditor.SearchService;
using UnityEngine;

public class Parallax : MonoBehaviour
{

    Vector3 ogPos;
    Vector3 ogCam;
    Camera cam;
    [SerializeField]
    [Range(-100, 100)]
    float intensity = 1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ogPos = transform.position;
        cam = Camera.main;
        ogCam = cam.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = (cam.transform.position - ogCam) * intensity + ogPos;
    }
}
