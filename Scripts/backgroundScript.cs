using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.IK;

public class backgroundScript : MonoBehaviour
{
    float oldWidth = 4;
    float oldHeight = 4;

    public List<Sprite> sprites;

    // Update is called once per frame
    void Update()
    {
        if(transform.parent.localScale.x != oldWidth || transform.parent.localScale.x != oldHeight){
            oldWidth = transform.parent.localScale.x;
            oldHeight = transform.parent.localScale.x;
            
            while(transform.childCount != 0){
                DestroyImmediate(transform.GetChild(0));
            }

            for(int x = 0; x < Mathf.CeilToInt(oldWidth / 2); x++){
                for(int y = 0; y < Mathf.CeilToInt(oldHeight / 2); y++){
                    GameObject tempObj = new GameObject();
                    GameObject newObj = Instantiate(tempObj, new Vector3 (x * 4 - 2, y * 4 - 2, 0), new Quaternion(), transform.parent);
                    SpriteRenderer sr = newObj.AddComponent<SpriteRenderer>();
                    sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                    sr.sortingOrder = -2;
                    sr.sprite = sprites[Random.Range(0, 4)];
                    sr.color = new Color (0.65f, 0.65f, 0.65f, 1f);

                    newObj.transform.localScale = new Vector3 (1.75f/Mathf.Ceil(newObj.transform.lossyScale.x), 1.75f/Mathf.Ceil(newObj.transform.lossyScale.y), 1);
                    newObj.transform.localPosition = new Vector3 (x * 2 + 2, y * 2 + 2, 0);

                    Destroy(tempObj);
                }
            }
        }
    }
}
