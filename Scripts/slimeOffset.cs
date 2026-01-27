using System;
using UnityEngine;

public class slimeOffset : MonoBehaviour
{

    Vector2 offset = new Vector2 (0, 0);
    float scale = 70f;
    Material mat;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mat = this.GetComponent<SpriteRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        mat.SetFloat("_scale", 70f + 10 * (Mathf.Sin(Time.time) + 1) / 2);
        mat.SetVector("_offset", offset += new Vector2 (0.0005f, 0f));
    }
}
