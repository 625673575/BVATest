using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using TMPro;
using BVA;
using UnityEngine.Networking;
using Newtonsoft.Json;

// using UnityEditor;

public class TestCase : MonoBehaviour
{
    public TMP_Dropdown Dropdown;
    public GameObject Item;
    public GameObject AItem;

    //public Button LoadButton;
    private Dictionary<string, List<string>> _typePathsPairs = new Dictionary<string, List<string>>();
    private Dictionary<string, List<GameObject>> _itemDictionary = new Dictionary<string, List<GameObject>>();
    private string _lastType = string.Empty;
    private GameObject _lastItem = null;
    private GameObject _loadingItem;
    private Dictionary<string, GameObject> _animationList = new Dictionary<string, GameObject>();

    private GameObject _avatar;
    private Animation _animation => _avatar.GetComponent<Animation>();

    private void Awake()
    {
        Dropdown.ClearOptions();
    }

    private void Start()
    {
        DirectoryInfo info = null;
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_IOS
        info = new DirectoryInfo(Application.streamingAssetsPath);



#elif UNITY_ANDROID
        string configPath = Application.streamingAssetsPath + "/config.json";
        UnityWebRequest request = UnityWebRequest.Get(configPath);
        request.SendWebRequest();
        while (!request.isDone) { }
        File.WriteAllBytes(Application.persistentDataPath + "/config.json", request.downloadHandler.data);
        configPath = Application.persistentDataPath + "/config.json";

        string json = File.ReadAllText(configPath);
        List<string> allPaths = JsonConvert.DeserializeObject<List<string>>(json);
        foreach (string path in allPaths)
        {
            CreateDir(path);
            UnityWebRequest fileRequest = UnityWebRequest.Get(Application.streamingAssetsPath + path);
            fileRequest.SendWebRequest();
            while (!fileRequest.isDone) { }
            File.WriteAllBytes(Application.persistentDataPath + path, fileRequest.downloadHandler.data);
        }

        info = new DirectoryInfo(Application.persistentDataPath);
#endif

        List<string> directorys = new List<string>();
        foreach (var directory in info.GetDirectories())
        {
            directorys.Add(directory.Name);
            _typePathsPairs.Add(directory.Name, new List<string>());

            var files = directory.GetFiles();

            foreach (FileInfo file in files)
            {
                _typePathsPairs[directory.Name].Add(file.FullName);
            }
        }

        Dropdown.AddOptions(directorys);
        Dropdown.onValueChanged.AddListener(ChangeTestType);
        //LoadButton.onClick.AddListener(GetOutModel);
        ChangeTestType(0);
    }

    private void CreateDir(string path)
    {
        string dir = "";

        if (Path.GetDirectoryName(path) != string.Empty)
        {
            Path.GetDirectoryName(path);
            dir = "/" + Path.GetDirectoryName(path);
        }

        if (!Directory.Exists(Application.persistentDataPath + dir))
        {
            Directory.CreateDirectory(Application.persistentDataPath + dir);
        }
    }

    private void ChangeTestType(int value)
    {
        if (_lastType != string.Empty)
        {
            foreach (GameObject item in _itemDictionary[_lastType])
            {
                item.SetActive(false);
            }
        }
        string typename = Dropdown.options[value].text;
        _lastType = typename;
        if (_itemDictionary.ContainsKey(_lastType))
        {
            foreach (GameObject item in _itemDictionary[_lastType])
            {
                item.SetActive(true);
            }
        }
        else
        {
            _itemDictionary.Add(_lastType, new List<GameObject>());

            foreach (string path in _typePathsPairs[typename])
            {
                if (Path.GetExtension(path) == ".gltf")
                {
                    string assetName = Path.GetFileNameWithoutExtension(path);

                    GameObject newItem = Instantiate(Item, Item.transform.parent);
                    newItem.SetActive(true);
                    newItem.GetComponentInChildren<TMP_Text>().text = assetName;
                    newItem.GetComponent<Button>().onClick.AddListener(delegate { LoadGLTFModelByPath(newItem, path); });
                    _itemDictionary[_lastType].Add(newItem);
                }
            }
        }
    }

    private async void LoadGLTFModelByPath(GameObject item, string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        _loadingItem = item;
        if (_avatar != null)
        {
            Destroy(_avatar);
        }

        Debug.LogFormat("{0}", path);
        var ext = Path.GetExtension(path).ToLower();
        switch (ext)
        {
            case ".gltf":
            case ".glb":
            case ".zip":
                BVASceneManager.Instance.onSceneLoaded = OnLoaded;
                await BVASceneManager.Instance.LoadSceneAsync(path);
                break;
        }
    }

    private void OnLoaded(AssetType assetType, BVAScene bVAScene)
    {
        if (_lastItem != null)
        {
            _lastItem.GetComponent<Image>().color = Color.white;
        }
        _lastItem = _loadingItem;
        _lastItem.GetComponent<Image>().color = Color.yellow;
        foreach (string key in _animationList.Keys)
        {
            Destroy(_animationList[key]);
        }
        _animationList.Clear();

        _avatar = bVAScene.mainScene.gameObject;

        Animation animation = null;
        if (_avatar.TryGetComponent(out animation))
        {
            animation.wrapMode = WrapMode.Loop;
            int layer = 0;
            string firstAniName = string.Empty;
            foreach (AnimationState state in animation)
            {
                state.layer = layer;
                layer++;

                GameObject newAni = Instantiate(AItem, AItem.transform.parent);
                newAni.SetActive(true);
                newAni.GetComponentInChildren<TMP_Text>().text = state.clip.name;
                newAni.GetComponent<Button>().onClick.AddListener(delegate { SwitchAnimation(state.clip.name); });
                _animationList.Add(state.clip.name, newAni);

                if (firstAniName == string.Empty)
                {
                    firstAniName = state.clip.name;
                }
            }

            SwitchAnimation(firstAniName);
        }

        Camera.main.gameObject.GetComponent<AroundCamera>().ResetCamera(_avatar.transform);
        // GetAvatarsValue();
    }

    private void SwitchAnimation(string animationName)
    {
        if (_animation.IsPlaying(animationName))
        {
            _animation.Stop(animationName);
            _animationList[animationName].GetComponent<Image>().color = Color.white;
        }
        else
        {
            _animation.Play(animationName);
            _animationList[animationName].GetComponent<Image>().color = Color.yellow;
        }
    }

    //private void GetOutModel()
    //{
    //    if (!EditorApplication.isPlaying)
    //    {
    //        EditorApplication.EnterPlaymode();
    //    }
    //    else
    //    {
    //        var glbPath = EditorUtility.OpenFilePanel("glb or gltf file", "", "glb,gltf");

    //        LoadGLTFModelByPath(glbPath);
    //    }
    //}

    //private async void LoadGLTFModelByPath(string path)
    //{
    //    if (!File.Exists(path))
    //    {
    //        return;
    //    }

    //    if (_avatar != null)
    //    {
    //        Destroy(_avatar);
    //    }

    //    Debug.LogFormat("{0}", path);
    //    var ext = Path.GetExtension(path).ToLower();
    //    switch (ext)
    //    {
    //        case ".gltf":
    //        case ".glb":
    //        case ".zip":
    //            BVASceneManager.Instance.onSceneLoaded = OnLoaded;
    //            await BVASceneManager.Instance.LoadSceneAsync(path);
    //            break;
    //    }
    //}
 
    //private void GetAvatarsValue(){
    //    MeshFilter[] filters = _avatar.GetComponentsInChildren<MeshFilter>();
    //    int tris1 = 0;
    //    int verts1 = 0;
    //    foreach(MeshFilter filter in filters)
    //    {
    //        Debug.Log(filter);
    //        Debug.Log(filter.sharedMesh.triangles);
    //        foreach(var val in filter.sharedMesh.triangles){
    //            Debug.Log(val);
    //        }
    //        Debug.Log(filter.sharedMesh.vertexCount);
    //        Debug.Log(filter.sharedMesh);
    //        tris1 += filter.sharedMesh.triangles.Length/3;
    //        verts1 += filter.sharedMesh.vertexCount;
    //    }
 
    //    Debug.Log("tris1:"+tris1+"verts1:"+verts1);
    //}
}
