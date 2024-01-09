using System;
using System.Globalization;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class AddressableUtils : MonoBehaviour
{
    private const string PRODUCTS_LABEL = "asset1";
    private float _percent;

    [SerializeField] private GameObject _loading;
    [SerializeField] private Text _textContent;
    [SerializeField] private Image _imageContent;
    
    [SerializeField] private Button _buttonInit;
    [SerializeField] private Button _buttonDownload;
    [SerializeField] private Button _buttonClearCache;

    private float _downloadSizeInMB;

    private void Awake()
    {
        this._buttonInit.onClick.AddListener(InitializeAsync);
        this._buttonDownload.onClick.AddListener(DownloadAsync);
        this._buttonClearCache.onClick.AddListener(ClearCache);
    }


    private void OnEnable()
    {
        this._loading.transform.localScale = new Vector3(0, 1, 1);
        this._buttonDownload.enabled = false;
        this._buttonClearCache.enabled = false;
        this._textContent.text = "Init First";
    }

    private async void DownloadAsync()
    {
        this._buttonDownload.enabled = false;
        if (this._downloadSizeInMB > 0)
        {
            AsyncOperationHandle downloadDependencies = Addressables.DownloadDependenciesAsync(PRODUCTS_LABEL, true);
            while (!downloadDependencies.IsDone)
            {
                _percent = downloadDependencies.GetDownloadStatus().Percent;

                this._textContent.text = $"Downloading {_percent * 100}% ({_downloadSizeInMB * _percent} MB / {_downloadSizeInMB} MB)";

                await UniTask.Yield();
            }
        }

        this._buttonClearCache.enabled = true;
    }
    
    private void InitializeAsync()
    {
        this._textContent.text = "Initialize Async";
        Addressables.InitializeAsync().Completed += handle =>
        {
            try
            {
                this._textContent.text = "Get Download Size Async";
                Debug.Log("Addressables.InitializeAsync.Completed:@" + Time.time);
                Addressables.GetDownloadSizeAsync(PRODUCTS_LABEL).Completed += handle =>
                {
                    Debug.Log("Addressables.GetDownloadSizeAsync.Completed:@" + Time.time);
                    this._downloadSizeInMB = (float)handle.Result / 1024 / 1024;
                    Debug.Log("GetDownloadSizeAsync: " + handle.Result + " bytes, which is " + this._downloadSizeInMB + " MB");
                    this._textContent.text = $"Size of download: {this._downloadSizeInMB.ToString(CultureInfo.InvariantCulture)}";
                    this._buttonInit.enabled = false;
                    this._buttonDownload.enabled = true;
                };
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        };
    }
    
    public void ClearCache()
    {
        Addressables.ClearDependencyCacheAsync(PRODUCTS_LABEL);
    }
}