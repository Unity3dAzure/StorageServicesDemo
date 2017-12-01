﻿using RESTClient;
using Azure.StorageServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using UnityEngine.Networking;
using System.Xml;

public class AssetBundleDemo : MonoBehaviour
{

  [Header("Azure Storage Service")]
  [SerializeField]
  private string storageAccount;
  [SerializeField]
  private string accessKey;
  [SerializeField]
  private string container;

  private StorageServiceClient client;
  private BlobService blobService;

  [Header("Asset Bundle Demo")]
  public Text label;
  public string assetBundleName = "cloud";
  private AssetBundle assetBundle;
  private GameObject loadedObject;

  [Header("Audio")]
  public AudioSource audioSource;

  private string localPath;
  private string saveFileXML = "scene.xml";
  private string saveFileJSON = "scene.json";

  // Use this for initialization
  void Start()
  {
    if (string.IsNullOrEmpty(storageAccount) || string.IsNullOrEmpty(accessKey))
    {
      Log.Text(label, "Storage account and access key are required", "Enter storage account and access key in Unity Editor", Log.Level.Error);
    }

    client = StorageServiceClient.Create(storageAccount, accessKey);
    blobService = client.GetBlobService();
  }

  // Update is called once per frame
  void Update()
  {

  }

  void OnDestroy()
  {
    UnloadAssetBundle();
  }

  private void UnloadAssetBundle()
  {
    if (assetBundle != null)
    {
      RemovePrefabs();
      loadedObject = null;
      assetBundle.Unload(true);
      Debug.Log("Unloaded Asset Bundle");
    }
  }

  public void TappedLoadAssetBundle()
  {
    UnloadAssetBundle();
    string filename = assetBundleName + "-" + GetAssetBundlePlatformName() + ".unity3d";
    string resourcePath = container + "/" + filename;
    Log.Text(label, "Load asset bundle: " + resourcePath);
    StartCoroutine(blobService.GetAssetBundle(GetAssetBundleComplete, resourcePath));
  }

  private void GetAssetBundleComplete(IRestResponse<AssetBundle> response)
  {
    if (response.IsError)
    {
      Log.Text(label, "Failed to load asset bunlde: " + response.StatusCode, response.ErrorMessage, Log.Level.Error);
    }
    else
    {
      Log.Text(label, "Loaded Asset Bundle:" + response.Url);
      assetBundle = response.Data;
      StartCoroutine(LoadAssets(assetBundle, "CloudCube")); ;
    }
  }

  private IEnumerator LoadAssets(AssetBundle bundle, string name)
  {
    // Load the object asynchronously
    AssetBundleRequest request = bundle.LoadAssetAsync(name, typeof(GameObject));

    // Wait for completion
    yield return request;

    // Get the reference to the loaded object
    loadedObject = request.asset as GameObject;
    loadedObject.tag = "Player";

    AddPrefab(new Vector3(0, 4, 0));
    Log.Text(label, "+ Prefab" + loadedObject.name, "Added prefab name: " + loadedObject.name);
  }

  private void AddPrefab(Vector3 position = default(Vector3))
  {
    AddPrefab(position, Quaternion.identity, Vector3.one, Color.clear);
  }

  private void AddPrefab(Vector3 position, Quaternion rotation, Vector3 scale, Color color)
  {
    if (assetBundle == null || loadedObject == null)
    {
      Log.Text(label, "Load asset bundle first", "Error, Asset Bundle was null", Log.Level.Warning);
      return;
    }
    GameObject gameObject = Instantiate(loadedObject, position, rotation);
    gameObject.transform.localScale = scale;
    gameObject.transform.GetComponent<Renderer>().material.SetColor("_EmissionColor", color);
  }

  public void TappedRemovePrefabs()
  {
    if (assetBundle == null)
    {
      Log.Text(label, "Tap 'Load Asset Bundle' first", "No asset bundles loaded", Log.Level.Warning);
      return;
    }
    RemovePrefabs();
    Log.Text(label, "- Remove Prefabs", "Remove Prefabs");
  }

  private void RemovePrefabs()
  {
    GameObject[] objs = GameObject.FindGameObjectsWithTag("Player");
    foreach (GameObject obj in objs)
    {
      Destroy(obj);
    }
  }

  private string GetAssetBundlePlatformName()
  {
    switch (Application.platform)
    {
      case RuntimePlatform.WindowsEditor:
      case RuntimePlatform.WindowsPlayer:
        return SystemInfo.operatingSystem.Contains("64 bit") ? "x64" : "x86";
      case RuntimePlatform.WSAPlayerX86:
      case RuntimePlatform.WSAPlayerX64:
      case RuntimePlatform.WSAPlayerARM:
        return "WSA";
      case RuntimePlatform.Android:
        return "Android";
      case RuntimePlatform.IPhonePlayer:
        return "iOS";
      case RuntimePlatform.OSXEditor:
      case RuntimePlatform.OSXPlayer:
        return "OSX";
      default:
        throw new Exception("Platform not listed");
    }
  }

  #region XML blob example

  public void TappedSaveXML()
  {
    XmlDocument xml = XmlHelper.LoadResourceDocument("sceneDoc");
    Debug.Log(xml.OuterXml);
    StartCoroutine(blobService.PutTextBlob(PutXmlCompleted, xml.OuterXml, container, saveFileXML, "application/xml"));
  }

  private void PutXmlCompleted(RestResponse response)
  {
    if (response.IsError)
    {
      Log.Text(label, "Put XML error: " + response.ErrorMessage, "Put XML error: " + response.ErrorMessage, Log.Level.Warning);
      return;
    }
    Log.Text(label, "Put XML: " + response.Url, "Put XML: " + response.Url);
  }

  public void TappedLoadXML()
  {
    string resourcePath = container + "/" + saveFileXML;
    Log.Text(label, "Load XML: " + resourcePath);
    StartCoroutine(blobService.GetXmlBlob<SceneDoc>(LoadXMLComplete, resourcePath));
  }

  private void LoadXMLComplete(IRestResponse<SceneDoc> response)
  {
    if (response.IsError)
    {
      Log.Text(label, "Tap 'Save XML' first", response.StatusCode + " Failed to load XML: " + response.Url, Log.Level.Warning);
    }
    else
    {
      Debug.Log("xml:" + response.Content);
      ProcessSceneXML(response.Data);
    }
  }

  private void ProcessSceneXML(SceneDoc sceneData)
  {
    Debug.LogFormat("Levels: {0}", sceneData.Levels.Length);

    if (sceneData.Levels.Length <= 0)
    {
      return;
    }

    if (loadedObject == null)
    {
      Log.Text(label, "Tap 'Load Asset Bundle' first to load prefabs");
      return;
    }

    LevelDoc level = sceneData.Levels[0];
    foreach (PrefabDoc prefab in level.Prefabs)
    {
      prefab.Init();
      Debug.LogFormat("Name: {0} Color: {1} Location: {2} Scale: {3}", prefab.name, prefab.GetColour(), prefab.GetPosition(), prefab.GetScale());
      AddPrefab(prefab.GetPosition(), Quaternion.identity, prefab.GetScale(), prefab.GetColour());
    }
  }

  #endregion

  #region JSON blob example

  public void TappedSaveJSON()
  {
    string filename = "scene";
    TextAsset asset = (TextAsset)Resources.Load(filename);
    string json = asset.text;

    Scene scene = JsonUtility.FromJson<Scene>(json);
    string jsonString = JsonUtility.ToJson(scene);

    StartCoroutine(blobService.PutTextBlob(PutJSONCompleted, jsonString, container, saveFileJSON, "application/json"));
  }

  private void PutJSONCompleted(RestResponse response)
  {
    if (response.IsError)
    {
      Log.Text(label, "Put JSON error: " + response.ErrorMessage, "Put JSON error: " + response.ErrorMessage, Log.Level.Warning);
      return;
    }
    Log.Text(label, "Put JSON: " + response.Url, "Put JSON: " + response.Url);
  }

  public void TappedLoadJSON()
  {
    string resourcePath = container + "/" + saveFileJSON;
    Log.Text(label, "Load JSON: " + resourcePath);
    StartCoroutine(blobService.GetJsonBlob<Scene>(LoadJSONComplete, resourcePath));
  }

  private void LoadJSONComplete(IRestResponse<Scene> response)
  {
    if (response.IsError)
    {
      Log.Text(label, "Tap 'Save XML' first", response.StatusCode + " Failed to load XML: " + response.Url, Log.Level.Warning);
    }
    else
    {
      Debug.Log("json: " + response.Content);
      ProcessJSONSceneData(response.Data);
    }
  }

  private void ProcessJSONSceneData(Scene sceneData)
  {
    Debug.LogFormat("Levels: {0}", sceneData.levels.Length);

    if (sceneData.levels.Length <= 0)
    {
      return;
    }

    if (loadedObject == null)
    {
      Log.Text(label, "Tap 'Load Asset Bundle' first to load prefabs");
      return;
    }

    Level level = sceneData.levels[0];
    foreach (Prefab prefab in level.prefabs)
    {
      prefab.Init();
      Debug.LogFormat("Name: {0} Color: {1} Location: {2} Scale: {3}", prefab.name, prefab.Colour, prefab.Position, prefab.Scale);
      AddPrefab(prefab.Position, Quaternion.identity, prefab.Scale, prefab.Colour);
    }
  }

  #endregion

  public void TappedNext()
  {
    SceneManager.LoadScene("AudioScene");
  }
}
