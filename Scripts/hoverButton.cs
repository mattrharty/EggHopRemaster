using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class hoverButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    string defaultText;
    TMP_Text textBox;

    void Start(){
        textBox = transform.GetComponentInChildren<TMP_Text>();
        defaultText = textBox.text;
    }

    public void OnPointerEnter(PointerEventData data){
        if(this.GetComponent<Button>().interactable){
            textBox.text = "> " + defaultText + " <";
        }
    }

    public void OnPointerExit(PointerEventData data){
        textBox.text = defaultText;
    }

}
