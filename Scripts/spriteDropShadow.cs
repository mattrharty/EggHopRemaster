using System.Collections.Generic;
using UnityEngine;

public class spriteDropShadow : MonoBehaviour
{

    List<SpriteRenderer> sr = new List<SpriteRenderer> { };
    public List<int> children = new List<int> { 0 };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (int i in children)
        {
            GameObject shadow = new GameObject("drop shadow", typeof(SpriteRenderer));
            shadow.transform.parent = transform;
            sr.Add(shadow.GetComponent<SpriteRenderer>());

            if (this.gameObject.GetComponent<SpriteRenderer>() != null)
            {
                shadow.transform.localPosition = new Vector3(0.17f, -0.17f, 0);
                sr[i].sprite = this.gameObject.GetComponent<SpriteRenderer>().sprite;
            }
            else
            {
                shadow.transform.localPosition = new Vector3(transform.GetChild(i).localPosition.x + 0.17f, transform.GetChild(i).localPosition.y + -0.17f, 0);
                shadow.transform.localScale = transform.GetChild(i).localScale;
                sr[i].sprite = transform.GetChild(i).gameObject.GetComponent<SpriteRenderer>().sprite;
            }

            sr[i].color = new Color(0, 0, 0, 0.5f);
            sr[i].sortingOrder = -2;
        }
    }

    void Update()
    {
        foreach (int i in children)
        {
            if (this.gameObject.GetComponent<SpriteRenderer>() != null)
            {
                sr[i].sprite = this.gameObject.GetComponent<SpriteRenderer>().sprite;
            }
            else
            {
                sr[i].sprite = transform.GetChild(i).gameObject.GetComponent<SpriteRenderer>().sprite;
            }
        }
    }
}
