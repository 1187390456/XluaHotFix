using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestCS : MonoBehaviour
{
    private void loadProgress(string bundleName, float progress)
    {
        Debug.Log(progress + " : " + bundleName);

        if (progress >= 1f && bundleName == "scene1/buildings.assetbundle")
        {
            //  Instantiate(AssetBundleManager.Instance.LoadAsset("Scene1", "Buildings", "Building1"));
        }
    }

    private void Start()
    {
        // AssetBundleManager.Instance.LoadAssetBundle("Scene1", "Buildings", loadProgress);
    }
}