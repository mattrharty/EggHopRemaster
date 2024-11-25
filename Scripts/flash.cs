using System.Collections;
using UnityEngine;

public class flash : MonoBehaviour
{

    public Color flashColor;
    public Material flashMat;

    public void bling(){
        StartCoroutine(Woosh());
    }

    IEnumerator Woosh(){
        Material ogMat = this.GetComponent<SpriteRenderer>().material;
        this.GetComponent<SpriteRenderer>().material = flashMat;
        yield return new WaitForSeconds(0.17f);
        this.GetComponent<SpriteRenderer>().material = ogMat;
    }

}
