using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class mainMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{

    [SerializeField] Transform selector;
    [SerializeField] int selectOffset = 166;

    [Space][Header("Text colors")]
    [SerializeField] Color normal;
    [SerializeField] Color selected;

    bool hover = false;

    bool clicked = false;

    TMP_Text text;
    Vector3 textPos;
    List<GameObject> fades;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        text = transform.GetChild(1).GetComponent<TMP_Text>();

        fades = new List<GameObject> {};

        Time.timeScale = 1;

        selector.GetChild(0).GetComponent<Animator>().SetBool("select", false);
    }

    public void OnPointerEnter(PointerEventData data){
        hover = true;
        StartCoroutine(coolText());
    }

    public void OnPointerExit(PointerEventData data){
        hover = false;
    }

    public void OnPointerClick(PointerEventData data){
        if(hover){
            clicked = true;
            selector.GetChild(0).GetComponent<Animator>().SetBool("select", true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine(resetHover());
        if(hover){
            text.color = selected;
            text.alignment = TextAlignmentOptions.Right;
        } else {
            text.color = normal;
            text.alignment = TextAlignmentOptions.Center;
        }

        for(int i = fades.Count - 1; i >= 0; i--){
            fades[i].GetComponent<RectTransform>().position -= new Vector3 (0, 30 * Time.deltaTime, 0);
            fades[i].GetComponent<TMP_Text>().color -= new Color (0, 0, 0, 0.14f * Time.deltaTime);
            if(fades[i].GetComponent<TMP_Text>().color.a <= 0){
                Destroy(fades[i]);
                fades.RemoveAt(i);
            }
        }
    }

    IEnumerator coolText(){
        yield return new WaitForSeconds(0.2f);
        while(hover){
            GameObject newText = Instantiate(text.gameObject, transform.GetChild(0));
            fades.Add(newText);
            newText.GetComponent<TMP_Text>().color = new Color32 (255, 255, 255, 60);
            yield return new WaitForSeconds(0.6f);
        }
    }

    IEnumerator resetHover(){
        if(hover || clicked){
            selector.GetChild(0).GetComponent<UnityEngine.UI.Image>().color = new Color32(255, 255, 255, 255);
            selector.position = new Vector3 (-1 * selectOffset * (Screen.width / 1920), 0, 0) + transform.position;
        }
        yield return new WaitForEndOfFrame();
        if(!hover){
            selector.GetChild(0).GetComponent<UnityEngine.UI.Image>().color = new Color32(255, 255, 255, 0);
        }
    }
}
