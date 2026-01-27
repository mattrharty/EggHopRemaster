using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class blockButtonGrow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    [SerializeField] bool growChild = true;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!growChild)
        {
            transform.localScale = new Vector3(1.12f, 1.12f, 1);
        }
        else
        {
            transform.GetChild(0).localScale = new Vector3(1.12f, 1.12f, 1);
        }
    }

    public void OnPointerExit(PointerEventData eventData){
                if (!growChild)
        {
            transform.localScale = new Vector3(1f, 1f, 1);
        }
        else
        {
            transform.GetChild(0).localScale = new Vector3(1f, 1f, 1);
        }
    }

}
