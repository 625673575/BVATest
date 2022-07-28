#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using System.IO;

public class SaveJson : MonoBehaviour
{
    [MenuItem("BVA Test/SaveJson")]
    static void Save()
    {
        List<string> allPath = GetPath(Application.streamingAssetsPath);
        string json = JsonConvert.SerializeObject(allPath);
        File.WriteAllText(Application.streamingAssetsPath + "/config.json", json);
    }

    static List<string> GetPath(string path, string dir = "")
    {
        string lastDir = dir;

        List<string> result = new List<string>();

        DirectoryInfo directoryInfo = new DirectoryInfo(path);
        var dirs = directoryInfo.GetDirectories();
        var files = directoryInfo.GetFiles();

        foreach (FileInfo file in files)
        {
            if (file.Extension == ".meta")
            {
                continue;
            }

            result.Add(lastDir + "/" + file.Name);
        }

        foreach (DirectoryInfo directory in dirs)
        {
            result.AddRange(GetPath(directory.FullName, lastDir + "/" + directory.Name));
        }

        return result;
    }
}

#endif