using System.IO;
using UnityEditor;
using UnityEngine;

public class ExportAssetBundles
{
    [MenuItem("Assets/Build AssetBundle")]
    static void ExportResource()
    {
        const string folderName = "Assets/AssetBundles";

        //Build for Windows platform
        BuildPipeline.BuildAssetBundles(folderName, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);

        //Refresh the Project folder
        AssetDatabase.Refresh();
    }
}
