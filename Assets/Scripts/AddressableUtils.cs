using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.UI;

public class AddressableUtils : MonoBehaviour
{
    private const string PRODUCTS_LABEL = "asset1";
    private float _percent = 0f;
    private GameObject _obj;
    
    [SerializeField] private string keySpawn1;
    [SerializeField] private string keySpawn2;
    [SerializeField] private TextMeshProUGUI _loadingDisplayText;
    [SerializeField] private TextMeshProUGUI _checkUpdateText;
    [SerializeField] private Button spawnBtn1;
    [SerializeField] private Button spawnBtn2;
    [SerializeField] private Button clearCache;
    
    private IEnumerator Start()
    {
        // StartCoroutine(CheckUpdate());
        
        AsyncOperationHandle<long> getDownloadSize = Addressables.GetDownloadSizeAsync(PRODUCTS_LABEL);
        yield return getDownloadSize;
        float downloadSize = getDownloadSize.Result / 1024 / 1024;
        Debug.Log("Download size in MB: " + downloadSize);
        
        if (getDownloadSize.Result > 0)
        {
            AsyncOperationHandle downloadDependencies = Addressables.DownloadDependenciesAsync(PRODUCTS_LABEL, true);
            while (!downloadDependencies.IsDone)
            {
                _percent = downloadDependencies.GetDownloadStatus().Percent;
                _loadingDisplayText.SetText($"Downloading {downloadSize}mb, {_percent * 100:F2}% completed");
                yield return null;
            }
        }
        
        spawnBtn1.gameObject.SetActive(true);
        spawnBtn2.gameObject.SetActive(true);
        spawnBtn1.onClick.AddListener(() => SpawnObj(keySpawn1));
        spawnBtn2.onClick.AddListener(() => SpawnObj(keySpawn2));
        clearCache.onClick.AddListener(ClearCache);
    }
    
    private IEnumerator CheckUpdate()
    {
        var init = Addressables.InitializeAsync();

        yield return(init);

        AsyncOperationHandle <List <string> > checkHandle = Addressables.CheckForCatalogUpdates(false);

        yield return(checkHandle);

        if (checkHandle.Status == AsyncOperationStatus.Succeeded)
        {
            List <string> catalogs = checkHandle.Result;
            if (catalogs != null && catalogs.Count > 0)
            {
                Debug.Log("download start");
                var updateHandle = Addressables.UpdateCatalogs(catalogs, false);
                yield return(updateHandle);

                Debug.Log("download finish");
                _loadingDisplayText.SetText($"Check Update");
            }
        }
        Addressables.Release(checkHandle);

    }
    
    public void ClearCache()
    {
        Addressables.ClearDependencyCacheAsync(PRODUCTS_LABEL);
    }

    private void SpawnObj(string key)
    {
        Addressables.InstantiateAsync(key).Completed += OnLoadDone;
    }

    private void OnLoadDone(AsyncOperationHandle<GameObject> asyncOperationHandle)
    {
        _obj = asyncOperationHandle.Result;
    }
}