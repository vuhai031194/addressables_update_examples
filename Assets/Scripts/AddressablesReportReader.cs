using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class AddressablesReportReader : MonoBehaviour
{
    [MenuItem("Addressables/Read Newest Build Report")]
    public static void ReadNewestBuildReport()
    {
        string folderPath = Path.Combine(Application.dataPath, "../Library/com.unity.addressables/BuildReports");
        var directoryInfo = new DirectoryInfo(folderPath);
        var newestReportFile = directoryInfo.GetFiles("*.json")
            .OrderByDescending(f => f.LastWriteTime)
            .FirstOrDefault();

        if (newestReportFile != null)
        {
            Debug.Log($"Reading: {newestReportFile.Name}");
            string jsonData = File.ReadAllText(newestReportFile.FullName);
            ParseAndPrintAssetBundleInfo(jsonData);
        }
        else
        {
            Debug.LogError("No build report found.");
        }
    }

    private static void ParseAndPrintAssetBundleInfo(string jsonData)
    {
        List<BundleInformation> bundleList = new();
        Dictionary<int, BundleInformation> bundleDict = new();
        Dictionary<string, int> assetDict = new();
        
        var jsonObject = JObject.Parse(jsonData);
        var references = jsonObject["references"]["RefIds"];

        foreach (var refId in references)
        {
            var bundleInformation = new BundleInformation();
            if (refId["type"]["class"].ToString() == "BuildLayout/Bundle")
            {
                bundleInformation.rid = refId["rid"].ToObject<int>();
                bundleInformation.name = refId["data"]["Name"].ToString();
                bundleInformation.size = refId["data"]["FileSize"].ToObject<long>();
                bundleInformation.hash = refId["data"]["Hash"]["Hash"].ToString();

                bundleInformation.dependencyRids = new List<int>();
                JArray dependencies = (JArray)refId["data"]["Dependencies"];
                foreach (var dependency in dependencies)
                {
                    int dependencyRid = dependency["rid"].ToObject<int>();
                    bundleInformation.dependencyRids.Add(dependencyRid);
                }
                
                bundleList.Add(bundleInformation);
                bundleDict.Add(bundleInformation.rid, bundleInformation);
            }

            if (refId["type"]["class"].ToString() == "BuildLayout/ExplicitAsset")
            {
                int bundleRid = refId["data"]["Bundle"]["rid"].ToObject<int>();
                string assetName = refId["data"]["AddressableName"].ToString();
                assetDict.Add(assetName, bundleRid);
            }
        }

        foreach (var data in assetDict)
        {
            if (bundleDict.TryGetValue(data.Value, out var bundleInformation))
            {
                bundleInformation.assets.Add(data.Key);
            }
        }
        
        File.WriteAllText($"{Application.dataPath}/Resources/bundle_manifest.json", JsonConvert.SerializeObject(bundleList));
    }
}

public class BundleInformation
{
    public int rid;
    public string name;
    public long size;
    public string hash;
    public List<int> dependencyRids = new();
    public List<string> assets = new();

    public override string ToString()
    {
        return $"Rid {rid}, Bundle Name: {name}, Size: {size}, Hash: {hash}, Dependencies: {dependencyRids.Count}";
    }
}