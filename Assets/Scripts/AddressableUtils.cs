using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

public class AddressableUtils : MonoBehaviour
{
    private const int TIME_OUT_SECONDS = 60;
    private const int MAX_RETRIES = 3;
    private const string PRODUCTS_LABEL = "default";
    private const string CATALOG_PATCH_PATH = "https://storage.googleapis.com/summoner_era/examples/catalog_patch_{0}.json";
    private const string BUNDLE_PLATFORM_PATH = "https://storage.googleapis.com/summoner_era/examples/{0}/{1}.bundle";
    
    private float _percent;

    [SerializeField] private GameObject _loading;
    [SerializeField] private Text _textContent;
    [SerializeField] private Image _imageContent;

    [SerializeField] private Button _buttonInit;
    [SerializeField] private Button _buttonDownload;
    [SerializeField] private Button _buttonClearCache;
    [SerializeField] private Button _buttonNext;
    [SerializeField] private TMP_InputField _inputField;

    private float _downloadSizeInMB;

    private int _imageIndex = 1;

    private void Awake()
    {
        this._buttonInit.onClick.AddListener(InitializeAsync);
        //this._buttonDownload.onClick.AddListener(DownloadAsync);
        this._buttonClearCache.onClick.AddListener(ClearCache);
        this._buttonNext.onClick.AddListener(Next);

        this._inputField.onValueChanged.AddListener(OnCataLogChange);
    }

    private  void OnCataLogChange(string fileName)
    {
        IDTransformer.CatalogFile = fileName;
    }
    
    private void OnEnable()
    {
        this._loading.transform.localScale = new Vector3(0, 1, 1);
        this._buttonDownload.enabled = false;
        this._buttonClearCache.enabled = true;
        this._textContent.text = "Init First";
    }

    private async void InitializeAsync()
    {
        this._textContent.text = "Initialize Async";
        
        try
        {
            string json = await DownloadJsonAsync(string.Format(CATALOG_PATCH_PATH, 1));
            Debug.Log("Downloaded JSON: " + json);
            // You can now process the JSON string as needed
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
        }
        
        // Addressables.InitializeAsync().Completed += handle =>
        // {
        //     try
        //     {
        //         this._textContent.text = "Get Download Size Async";
        //         Debug.Log("Addressables.InitializeAsync.Completed:@" + Time.time);
        //         Addressables.GetDownloadSizeAsync(PRODUCTS_LABEL).Completed += handle =>
        //         {
        //             Debug.Log("Addressables.GetDownloadSizeAsync.Completed:@" + Time.time);
        //             this._downloadSizeInMB = (float)handle.Result / 1024 / 1024;
        //             Debug.Log("GetDownloadSizeAsync: " + handle.Result + " bytes, which is " + this._downloadSizeInMB +
        //                       " MB");
        //             this._textContent.text =
        //                 $"Size of download: {this._downloadSizeInMB.ToString(CultureInfo.InvariantCulture)}";
        //             this._buttonInit.enabled = false;
        //             this._buttonDownload.enabled = true;
        //         };
        //     }
        //     catch (Exception e)
        //     {
        //         Debug.Log(e);
        //     }
        // };
    }
    
    private async UniTask<string> DownloadJsonAsync(string url)
    {
        int attempt = 0;

        while (attempt < MAX_RETRIES)
        {
            using (var request = UnityWebRequest.Get(url))
            {
                request.timeout = TIME_OUT_SECONDS; // Set the timeout

                // Await the completion of the web request
                await request.SendWebRequest().WithCancellation(this.GetCancellationTokenOnDestroy());

                if (request.result == UnityWebRequest.Result.Success)
                {
                    // Successfully downloaded the JSON string
                    return request.downloadHandler.text;
                }

                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError ||
                    request.result == UnityWebRequest.Result.DataProcessingError)
                {
                    Debug.LogError($"Attempt {attempt + 1}: Error downloading JSON: {request.error}");
                }
                // Increment attempt counter
                attempt++;
            }

            // Optionally wait before retrying
            await UniTask.Delay(1000); // Wait for 1 second before retrying
        }

        // If reached here, all attempts failed
        throw new System.Exception("Failed to download JSON after multiple attempts.");
    }
    
    private async UniTask<AssetBundle> LoadAssetBundle(string url)
    {
        using UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(url);
        // Send the request and await its completion without blocking the main thread
        await uwr.SendWebRequest().WithCancellation(this.GetCancellationTokenOnDestroy());

        if (uwr.result == UnityWebRequest.Result.Success)
        {
            // Download succeeded, load the AssetBundle
            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(uwr);
            return bundle;
        }

        Debug.LogError($"Failed to download AssetBundle: {uwr.error}");
        return null;
    }

    private void ClearCache()
    {
        this._textContent.text = "Clear Cache";

        this._buttonInit.enabled = true;

        Addressables.ClearDependencyCacheAsync(PRODUCTS_LABEL);
    }

    private async UniTask<bool> SetFrame()
    {
        try
        {
            var sprite = await Addressables.LoadAssetAsync<Sprite>(_imageIndex.ToString());
            this._imageContent.sprite = sprite;
            return true;
        }
        catch (Exception _)
        {
            Debug.Log($"Sprite is null");

            return false;
        }
    }

    private async void Next()
    {
        var result = await SetFrame();

        if (result)
        {
            this._imageIndex++;
        }
        else
        {
            this._imageIndex = 1;
            Next();
        }
    }
}