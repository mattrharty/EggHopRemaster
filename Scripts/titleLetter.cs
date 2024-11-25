using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class titleLetter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public Image letters;
    public Sprite jagged;
    public Sprite erect;

    public void OnPointerEnter(PointerEventData eventData)
    {
        letters.sprite = erect;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        letters.sprite = jagged;
    }
}
