using System;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.AddressableAssets;

public static class IDTransformer
{
    private const string CLOUD_PATH = "https://storage.googleapis.com/se-asset-bundle/{0}/{1}/{2}";

    public static string CatalogFile = "catalog_2024.01.16.07.24.17";

    //Implement a method to transform the internal ids of locations
    static string MyCustomTransform(IResourceLocation location)
    {
        if (location.InternalId.StartsWith("https") == false)
        {
            return location.InternalId;
        }

        if (location.ResourceType == typeof(IAssetBundleResource))
        {
            var fileName = GetFileNameFromUrl(location.InternalId);
            var filePath = string.Format(CLOUD_PATH, Application.platform, Application.version, fileName);

            Debug.Log($"bundle -> {filePath}");
            return filePath;
        }

        if (location.InternalId.EndsWith("hash"))
        {
            var filePath = string.Format(CLOUD_PATH, Application.platform, Application.version, $"{CatalogFile}.hash");
            
            Debug.Log($"hash -> {filePath}");
            return filePath;
        }

        if (location.InternalId.EndsWith("json"))
        {
            var filePath = string.Format(CLOUD_PATH, Application.platform, Application.version, $"{CatalogFile}.json");

            Debug.Log($"json -> {filePath}");
            return filePath;
        }

        return location.InternalId;
    }

    //Override the Addressables transform method with your custom method.
    //This can be set to null to revert to default behavior.
    [RuntimeInitializeOnLoadMethod]
    static void SetInternalIdTransform()
    {
        Addressables.InternalIdTransformFunc = MyCustomTransform;
    }

    public static string GetFileNameFromUrl(string url)
    {
        Uri uri = new Uri(url);
        string fileName = uri.Segments[^1];
        return fileName;
    }
}