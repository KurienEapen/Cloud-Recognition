using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;
using UnityEngine.Networking;
using UnityEngine.Video; 

public class LoadAsset : MonoBehaviour, ICloudRecoEventHandler
{


    #region PRIVATE_MEMBER_VARIABLES

    // CloudRecoBehaviour reference to avoid lookups
    private CloudRecoBehaviour mCloudRecoBehaviour;
    // ImageTracker reference to avoid lookups
    private ObjectTracker mImageTracker;

    private bool mIsScanning = false;

    private string mTargetMetadata = "";
    AssetBundle myLoadedAssetbundle;

    #endregion // PRIVATE_MEMBER_VARIABLES



    #region EXPOSED_PUBLIC_VARIABLES

    /// <summary>
    /// can be set in the Unity inspector to reference a ImageTargetBehaviour that is used for augmentations of new cloud reco results.
    /// </summary>
    public ImageTargetBehaviour ImageTargetTemplate;
    public string path;
    public GameObject newImageTarget;
    public GameObject player;
    Object video ; 
    private AudioSource[] allAudioSources;
    private bool detected; 
    public AudioSource audioSource;

    #endregion

    #region UNTIY_MONOBEHAVIOUR_METHODS

    /// <summary>
    /// register for events at the CloudRecoBehaviour
    /// </summary>
    void Start()
    {
        // register this event handler at the cloud reco behaviour
       // LoadAssetBundle(path);
        video = Resources.Load("Video") as GameObject;
        CloudRecoBehaviour cloudRecoBehaviour = GetComponent<CloudRecoBehaviour>();
        if (cloudRecoBehaviour)
        {
            cloudRecoBehaviour.RegisterEventHandler(this);
        }

        // remember cloudRecoBehaviour for later
        mCloudRecoBehaviour = cloudRecoBehaviour;

    }

    #endregion // UNTIY_MONOBEHAVIOUR_METHODS


    #region ICloudRecoEventHandler_IMPLEMENTATION

    /// <summary>
    /// called when TargetFinder has been initialized successfully
    /// </summary>
    public void OnInitialized(TargetFinder targetFinder)
    {
        // get a reference to the Image Tracker, remember it
        mImageTracker = (ObjectTracker)TrackerManager.Instance.GetTracker<ObjectTracker>();
    }

    /// <summary>
    /// visualize initialization errors
    /// </summary>
    public void OnInitError(TargetFinder.InitState initError)
    {
    }

    /// <summary>
    /// visualize update errors
    /// </summary>
    public void OnUpdateError(TargetFinder.UpdateState updateError)
    {
    }

    /// <summary>
    /// when we start scanning, unregister Trackable from the ImageTargetTemplate, then delete all trackables
    /// </summary>
    public void OnStateChanged(bool scanning)
    {
        mIsScanning = scanning;
        if (scanning)
        {
            // clear all known trackables
            ObjectTracker tracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
            tracker.TargetFinder.ClearTrackables(false);
        }
        else 
        {
            if(mTargetMetadata.Contains("audio") == true)
                StopAllAudio();
            Debug.Log("Pause Audio");
        }
    }

 
    void StopAllAudio() {
        allAudioSources = FindObjectsOfType(typeof(AudioSource)) as AudioSource[];
        foreach( AudioSource audioS in allAudioSources) {
        audioS.Stop();
        }
    }

    /// <summary>
    /// Handles new search results
    /// </summary>
    /// <param name="targetSearchResult"></param>
    public void OnNewSearchResult(TargetFinder.TargetSearchResult targetSearchResult)
    {
        // duplicate the referenced image target
        newImageTarget = Instantiate(ImageTargetTemplate.gameObject) as GameObject;

        GameObject augmentation = null;
         TargetFinder.CloudRecoSearchResult cloudRecoSearchResult = 
        (TargetFinder.CloudRecoSearchResult)targetSearchResult;

        string model_name = cloudRecoSearchResult.MetaData;


        if (augmentation != null)
            augmentation.transform.parent = newImageTarget.transform;

        // enable the new result with the same ImageTargetBehaviour:
        ObjectTracker tracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
        ImageTargetBehaviour imageTargetBehaviour =
        (ImageTargetBehaviour)tracker.TargetFinder.EnableTracking(
        targetSearchResult, newImageTarget);


        Debug.Log("Metadata value is " + model_name);
        mTargetMetadata = model_name;
        if (model_name.Contains("video") != true)
        {  detected = true ;
          StartCoroutine(GetAssetBundle());
        }
        else {
            Debug.Log("Player Activated");
            GameObject obj;
            obj = (GameObject) Instantiate(video, newImageTarget.transform.position, newImageTarget.transform.rotation);
            //GameObject child =newImageTarget.transform.GetChild(0).gameObject;
            //GameObject child = mTargetMetadata.Find("Video").gameObject;
            //child.SetActive(true);
            obj.transform.parent = newImageTarget.transform;
            obj.transform.localScale = new Vector3(1,1,1);
            obj.transform.rotation = Quaternion.AngleAxis(90, Vector3.right);
            audioSource = newImageTarget.AddComponent<AudioSource>();
            newImageTarget.GetComponentInChildren<VideoPlayer>().source = VideoSource.Url;
 
            // Set mode to Audio Source.
            newImageTarget.GetComponentInChildren<VideoPlayer>().audioOutputMode = VideoAudioOutputMode.AudioSource;

            // We want to control one audio track with the video player
            newImageTarget.GetComponentInChildren<VideoPlayer>().controlledAudioTrackCount = 1;

            // We enable the first track, which has the id zero
            newImageTarget.GetComponentInChildren<VideoPlayer>().EnableAudioTrack(0, true);

            // ...and we set the audio source for this track
            newImageTarget.GetComponentInChildren<VideoPlayer>().SetTargetAudioSource(0, audioSource);

            // now set an url to play
            newImageTarget.GetComponentInChildren<VideoPlayer>().url = model_name;
           // player.GetComponent<VideoPlayer>().start = true;
        }

        if (!mIsScanning)
        {
            // stop the target finder
            mCloudRecoBehaviour.CloudRecoEnabled = true;
        }
    }

    void LoadAssetBundle(string bundlerUrl)
    {
        myLoadedAssetbundle = AssetBundle.LoadFromFile(bundlerUrl);
        Debug.Log(myLoadedAssetbundle == null ? "Failed to load AssetBundle" : "AssetBundle succesfully loaded");
    }

    void InstantiateObjectFromBundle()
    {
        //var prefab = myLoadedAssetbundle.LoadAsset(assetName);
        var prefab = myLoadedAssetbundle.LoadAllAssets();
        GameObject NewObject = (GameObject)Instantiate(prefab[0], newImageTarget.transform);
        //NewObject.transform.localScale = new Vector3(0.5f, 0.5f, .5f);
    }

    IEnumerator GetAssetBundle()
    {
        
        UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(mTargetMetadata);
        //UnityWebRequest www   = WWW.LoadFromCacheOrDownload(mTargetMetadata,5);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log("URL ERROR" + www.error);
        }
        else
        {
            myLoadedAssetbundle = DownloadHandlerAssetBundle.GetContent(www);
            Debug.Log("URL SUCCESSFUL");
            InstantiateObjectFromBundle();
            detected = false;
            myLoadedAssetbundle.Unload(false);
        }
        
    }

    #endregion // ICloudRecoEventHandler_IMPLEMENTATION

    void OnGUI()
    {
        GUI.skin.box.fontSize = 60;
        if(detected)
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height/20), "Loading content...");
    }



}
