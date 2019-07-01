using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;
using UnityEngine.Networking;

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

    #endregion

    #region UNTIY_MONOBEHAVIOUR_METHODS

    /// <summary>
    /// register for events at the CloudRecoBehaviour
    /// </summary>
    void Start()
    {
        // register this event handler at the cloud reco behaviour
       // LoadAssetBundle(path);
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
    public void OnInitialized()
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

        string model_name = targetSearchResult.MetaData;


        if (augmentation != null)
            augmentation.transform.parent = newImageTarget.transform;

        // enable the new result with the same ImageTargetBehaviour:
        ImageTargetBehaviour imageTargetBehaviour = mImageTracker.TargetFinder.EnableTracking(targetSearchResult, newImageTarget);


        Debug.Log("Metadata value is " + model_name);
        mTargetMetadata = model_name;
        StartCoroutine(GetAssetBundle());
        

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
        }
        
    }

    #endregion // ICloudRecoEventHandler_IMPLEMENTATION

    void OnGUI()
    {
        GUI.Box(new Rect(100, 200, 200, 50), "Metadata: " + mTargetMetadata);
    }



}
