using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using System.IO;
using System;
using System.Linq;

public class SaveJsonCase2 : EditorWindow
{
    [MenuItem("BVA Test/SaveJsonCase2")]
    static void Open()
    {
        GetWindow<SaveJsonCase2>();
    }

    private string path;
    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("Path", GUILayout.Width(50f));
            path = GUILayout.TextField(path);
            if (GUILayout.Button("Broser", GUILayout.Width(50f)))
            {
                path = EditorUtility.OpenFolderPanel("Path", Application.dataPath, "");
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(1.0f);
        if (GUILayout.Button("SaveJson", GUILayout.Width(200f)))
        {
            Save(path);
        }
    }

    private void Save(string path)
    {
        Dictionary<string, List<string>> pathDic = GetPath2(path);
        string json = JsonConvert.SerializeObject(pathDic);
        File.WriteAllText(path + "/config.json", json);
    }

    private Dictionary<string, List<string>> GetPath2(string path)
    {
        Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();

        DirectoryInfo directoryInfo = new DirectoryInfo(path);
        var dirs = directoryInfo.GetDirectories();

        List<string> typeList = Enum.GetNames(typeof(TestType)).ToList();

        foreach (string type in typeList)
        {
            if (!Directory.Exists(directoryInfo.FullName + "/" + type))
            {
                Directory.CreateDirectory(directoryInfo.FullName + "/" + type);
            }
        }

        foreach (DirectoryInfo directory in dirs)
        {
            result.Add(directory.Name, new List<string>());

            List<string> files = null;
            switch (directory.Name)
            {
                case "GLBAvatarRuntimeLoad":
                    files = GetFilePathWithExtension(directory.FullName, directory.Name, ".glb");
                    result[directory.Name].AddRange(files);
                    files = GetFilePathWithExtension(directory.FullName, directory.Name, ".gltf");
                    result[directory.Name].AddRange(files);
                    break;
                case "GLBSceneRuntimeLoad":
                    files = GetFilePathWithExtension(directory.FullName, directory.Name, ".glb");
                    result[directory.Name].AddRange(files);
                    files = GetFilePathWithExtension(directory.FullName, directory.Name, ".gltf");
                    result[directory.Name].AddRange(files);
                    break;
                case "MultiSceneRuntimeLoad":
                    files = GetFilePathWithExtension(directory.FullName, directory.Name, ".glb");
                    result[directory.Name].AddRange(files);
                    files = GetFilePathWithExtension(directory.FullName, directory.Name, ".gltf");
                    result[directory.Name].AddRange(files);
                    break;
            }
        }

        return result;
    }

    private List<string> GetFilePathWithExtension(string path, string dir, string extension)
    {
        string lastDir = dir;
        List<string> result = new List<string>();

        DirectoryInfo directoryInfo = new DirectoryInfo(path);
        var dirs = directoryInfo.GetDirectories();
        var files = directoryInfo.GetFiles();

        foreach (FileInfo file in files)
        {
            if (file.Extension == extension)
            {
                result.Add(lastDir + "/" + file.Name);
            }
        }

        foreach (DirectoryInfo directory in dirs)
        {
            result.AddRange(GetFilePathWithExtension(directory.FullName, lastDir + "/" + directory.Name, extension));
        }

        return result;
    }
}
