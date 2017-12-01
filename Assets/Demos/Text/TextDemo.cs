using RESTClient;
using Azure.StorageServices;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class TextDemo : MonoBehaviour
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

  [Header("Text Demo")]
  [SerializeField]
  private InputField inputField;
  [SerializeField]
  private string filename;
  public Text label;

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

  public void TappedSaveText()
  {
    ChangeLabelText("Saving...");
    string text = !string.IsNullOrEmpty(inputField.text) ? inputField.text : "hello";
    StartCoroutine(blobService.PutTextBlob(PutTextBlobComplete, text, container, filename));
  }

  private void PutTextBlobComplete(RestResponse response)
  {
    if (response.IsError)
    {
      Log.Text(label, response.ErrorMessage + " Error putting blob:" + response.Content);
      return;
    }
    Log.Text(label, "Put blob status:" + response.StatusCode);
  }

  public void TappedLoadText()
  {
    ChangeLabelText("Loading...");
    string resourcePath = container + "/" + filename;
    StartCoroutine(blobService.GetTextBlob(GetTextBlobComplete, resourcePath));
  }

  private void GetTextBlobComplete(RestResponse response)
  {
    if (response.IsError)
    {
      Log.Text(label, response.ErrorMessage + " Error getting blob:" + response.Content);
      return;
    }
    Log.Text(label, "Get blob:" + response.Content);
  }

  private void ChangeLabelText(string text)
  {
    label.text = text;
  }

  public void TappedNext()
  {
    SceneManager.LoadScene("AssetBundleScene");
  }
}
