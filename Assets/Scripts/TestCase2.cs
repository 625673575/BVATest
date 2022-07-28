using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Linq;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.IO;
using UnityEngine.UI;
using BVA;

public enum TestType
{
    GLBAvatarRuntimeLoad,
    GLBSceneRuntimeLoad,
    MultiSceneRuntimeLoad,
}

public enum LoadType
{
    CommonLoad,
    FastLoad
}

public class TestCase2 : MonoBehaviour
{
    const string HTTP = "http://";
    const string DEFAULT_URL = "127.0.0.1:7777";

    public TMP_InputField URLInput;
    public Button Btn_Link;
    public TMP_Dropdown TestTypeDrop;
    public TMP_Dropdown LoadTypeDrop;
    public GameObject LoadItem;
    public GameObject ErrorPanel;
    public Button Btn_Inspector;
    public GameObject RuntimeHierarchy;

    private Dictionary<string, List<string>> _FileList;
    private List<GameObject> _ItemList = new List<GameObject>();

    private GameObject _avatar;

    private void Start()
    {
        Btn_Inspector.onClick.AddListener(SwitchInspector);
        Btn_Link.onClick.AddListener(Link);

        LoadTypeDrop.ClearOptions();
        var loadTypeList = Enum.GetNames(typeof(LoadType)).ToList();
        LoadTypeDrop.AddOptions(loadTypeList);

        URLInput.text = DEFAULT_URL;
        Link();
    }

    private void Link()
    {
        TestTypeDrop.ClearOptions();
        ErrorPanel.SetActive(false);
        foreach (GameObject item in _ItemList)
        {
            Destroy(item);
        }
        _ItemList.Clear();

        string fileUrl = HTTP + URLInput.text + "/" + "Config.json";
        UnityWebRequest request = UnityWebRequest.Get(fileUrl);
        request.SendWebRequest();
        while (!request.isDone) { }
        if (request.downloadHandler.text == string.Empty)
        {
            ErrorPanel.SetActive(true);
        }
        else
        {
            string json = request.downloadHandler.text;
            _FileList = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);

            var testType = Enum.GetNames(typeof(TestType)).ToList();
            TestTypeDrop.AddOptions(testType);
            TestTypeDrop.onValueChanged.AddListener(SwitchTestType);

            SwitchTestType(0);
        }
    }

    private void SwitchInspector()
    {
        if (RuntimeHierarchy.activeSelf)
        {
            RuntimeHierarchy.SetActive(false);
            Camera.main.gameObject.GetComponent<AroundCamera>().Enable = true;
        }
        else
        {
            RuntimeHierarchy.SetActive(true);
            Camera.main.gameObject.GetComponent<AroundCamera>().Enable = false;
        }
    }

    private void SwitchTestType(int value)
    {
        foreach (GameObject item in _ItemList)
        {
            Destroy(item);
        }
        _ItemList.Clear();

        switch (value)
        {
            case (int)TestType.GLBAvatarRuntimeLoad:
                RefreshList(TestType.GLBAvatarRuntimeLoad);
                break;
            case (int)TestType.GLBSceneRuntimeLoad:
                RefreshList(TestType.GLBSceneRuntimeLoad);
                break;
            case (int)TestType.MultiSceneRuntimeLoad:
                RefreshList(TestType.MultiSceneRuntimeLoad);
                break;
        }
    }

    private void RefreshList(TestType testType)
    {
        if (_FileList.Count != 0)
        {
            foreach (string filePath in _FileList[testType.ToString()])
            {
                GameObject newItem = Instantiate(LoadItem, LoadItem.transform.parent);
                newItem.SetActive(true);
                newItem.GetComponentInChildren<TMP_Text>().text = Path.GetFileNameWithoutExtension(filePath);
                newItem.GetComponent<Button>().onClick.AddListener(delegate { Load(HTTP + URLInput.text + "/" + filePath); });
                _ItemList.Add(newItem);
            }
        }
    }

    private async void Load(string url)
    {
        if (_avatar)
        {
            Destroy(_avatar);
        }
        BVASceneManager.Instance.onSceneLoaded = OnLoaded;

        switch (LoadTypeDrop.value)
        {
            case (int)LoadType.CommonLoad:
                await BVASceneManager.Instance.LoadSceneAsync(url);
                break;
            case ((int)LoadType.FastLoad):
                await BVASceneManager.Instance.LoadAvatar(url);
                break;
        }
    }

    private void OnLoaded(AssetType assetType, BVAScene bVAScene)
    {
        _avatar = bVAScene.mainScene.gameObject;

        Animator animator = _avatar.GetComponentInChildren<Animator>();

        if (animator != null)
        {
            var targetTransform = animator.GetBoneTransform(HumanBodyBones.Hips);
            Camera.main.gameObject.GetComponent<AroundCamera>().ResetCamera(targetTransform);
        }
        else
        {
            Camera.main.gameObject.GetComponent<AroundCamera>().ResetCamera(_avatar.transform);
        }
    }
}
