using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using DG.Tweening;

public class Hotspot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {

    public GameObject ThisPanorama;
    public static int Targets = 2;
    public GameObject[] TargetPanorama = new GameObject[Targets];
    public Transform Camera;
    private bool isTarget = false;
    int waitTime = 0;
    public Color lerpedColor = Color.white;
    public AudioSource adSource;
    public AudioClip[] adClips;

    void Start()
    {
        adSource = gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
        //this.AddComponent<AudioSource>();
        //adSource = this.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update () 
    {

        
        if(isTarget)
        {
            waitTime +=1;
            Debug.Log("waitTime :"+ waitTime);
            if(waitTime > 100)
            {
                waitTime = 0;
                isTarget = false;
                OnHotspotTransition();
            }
        }
        else
        {
            float emission = Mathf.PingPong (Time.time, 1.6f);
            Color baseColor = Color.yellow; 
            Color finalColor = baseColor * Mathf.LinearToGammaSpace (emission);
            this.GetComponent<Renderer>().material.color =  finalColor;
            // lerpedColor = Color.Lerp(Color.white, Color.yellow, Mathf.PingPong(Time.time, 1));
            // this.GetComponent<Renderer>().material.color = lerpedColor;
        }
      
    }

    public void OnPointerClick(PointerEventData eventData) 
    {

        this.GetComponent<Renderer> ().material.color = Color.blue;
        transform.DOScale(new Vector3(0.02f, 0.02f, 0.02f), 0.3f);
        OnPointerExit(eventData);
        OnHotspotTransition();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOScale(new Vector3(0.04f, 0.04f, 0.04f), 0.3f);
        isTarget = true ;
        this.GetComponent<Renderer> ().material.color = Color.green;

    }

    public void OnPointerDownDelegate(PointerEventData data)
{
    Debug.Log(data.pointerCurrentRaycast.gameObject.name);
}

    public void OnPointerExit(PointerEventData eventData)
    {
        
        isTarget = false ;
        transform.DOScale(new Vector3(0.02f, 0.02f, 0.02f), 0.3f);
        waitTime = 0;
        this.GetComponent<Renderer> ().material.color = Color.white;

    }

    public void OnHotspotTransition() 
    {     
        transform.DOScale(new Vector3(0.02f, 0.02f, 0.02f), 0.3f);
        StartCoroutine(playAudioSequentially());
        setCamera();
    }

    // private void SetSkyBox() 
    // {
    //     if(TourManager.SetCameraPosition != null)
    //         TourManager.SetCameraPosition(TargetPanorama.transform.position, ThisPanorama.transform.position);  
    //     TargetPanorama.gameObject.SetActive(true);
    //     ThisPanorama.gameObject.SetActive(false);
    // }

    public void setCamera()
    {
        Camera.transform.position = TargetPanorama[0].transform.position;
        TargetPanorama[0].gameObject.SetActive(true);
        var newTargetRenderers = TargetPanorama[0].gameObject.GetComponentsInChildren<Renderer>(true);
        foreach (var component in newTargetRenderers)
            component.enabled = true;
        
        //this.transform.parent.gameObject.SetActive(false);
        GameObject Parent = this.transform.parent.gameObject;
        var rendererComponents = Parent.GetComponentsInChildren<Renderer>(true);
        foreach (var component in rendererComponents)
            component.enabled = false;
        
    }

    IEnumerator playAudioSequentially()
{
    yield return null;

    for (int i = 0; i < adClips.Length; i++)
    {
        adSource.clip = adClips[i];
        adSource.Play();
        while (adSource.isPlaying)
        {
            yield return null;
        }
    }
}

   

}

