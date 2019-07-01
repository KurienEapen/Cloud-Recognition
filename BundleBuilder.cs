using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BundleBuilder : Editor {

    [MenuItem("Assets/ Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
      
        BuildPipeline.BuildAssetBundles(@"C:\Users\kurie\Desktop\AssetBundles\Android", BuildAssetBundleOptions.None, BuildTarget.Android);
    }
}
