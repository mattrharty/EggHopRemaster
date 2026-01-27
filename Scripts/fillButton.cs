using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class fillButton : MonoBehaviour, IPointerDownHandler, IPointerExitHandler, IPointerUpHandler
{

    Slider fill;
    bool clicked = false;
    [SerializeField] levelLibrarian library;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fill = transform.GetChild(0).GetComponent<Slider>();
    }

    void Update(){
        if(clicked){
            fill.value += 0.75f * Time.deltaTime;
            if(fill.value == 1){
                fill.value = 0;
                clicked = false;
                library.deleteLvl();
            }
        }
    }

    public void OnPointerDown(PointerEventData data){
        clicked = true;
    }

    public void OnPointerUp(PointerEventData data){
        fill.value = 0;
        clicked = false;
    }

    public void OnPointerExit(PointerEventData data){
        fill.value = 0;
        clicked = false;
    }

}
