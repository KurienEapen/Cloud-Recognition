using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class Highlight : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {

	public Color lerpedColor = Color.white;
	bool onEnter = false;
	Color a1,a2,alphaColor,currentColor;
	public float timeToFade = 1.0f;

	// Use this for initialization
	void Start () {
			alphaColor = this.GetComponent<MeshRenderer>().material.color;
			currentColor = alphaColor;
			currentColor.a = 0.3f;
            alphaColor.a = 0;
		
	}
	
	// Update is called once per frame
	void Update () {
		if(onEnter)
		{
			this.GetComponent<MeshRenderer>().material.color = Color.Lerp(currentColor, alphaColor, Mathf.PingPong(Time.time, 1));
			
		}
	}

	public void OnPointerClick(PointerEventData eventData) 
    {
		
    }

	public void OnPointerEnter(PointerEventData eventData)
    {
		onEnter = true;
	}

	public void OnPointerExit(PointerEventData eventData)
    {
		onEnter = false;
		this.GetComponent<MeshRenderer>().material.color = alphaColor;
	}

	public void OnHotspotTransition() 
    {     
        
    }


}
