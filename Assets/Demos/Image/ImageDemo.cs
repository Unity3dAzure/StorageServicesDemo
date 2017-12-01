using RESTClient;
using Azure.StorageServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class ImageDemo : MonoBehaviour
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

  [Header("Image Demo")]
  public Image image;
  public Text label;

  private bool isCaptured = false;

  private string localPath;

  void Start()
  {
    if (string.IsNullOrEmpty(storageAccount) || string.IsNullOrEmpty(accessKey))
    {
      Log.Text(label, "Storage account and access key are required", "Enter storage account and access key in Unity Editor", Log.Level.Error);
    }

    client = StorageServiceClient.Create(storageAccount, accessKey);
    blobService = client.GetBlobService();
  }

  void Update()
  {
    // Display image once capture has saved file to local storage
    if (isCaptured && File.Exists(localPath))
    {
      Log.Text(label, "Saved: " + localPath, "Screenshot capture saved as:" + localPath);
      DisplayImage();
      isCaptured = false;
    }
  }

  public void TappedCaptureScreenshot()
  {
    string filename = string.Format("{0}.png", DateTime.UtcNow.ToString("yy-MM-dd-HH-mm-ss-ff"));
    localPath = Screenshot.Capture(filename);
    isCaptured = true;
    label.text = "Capture as: " + localPath;
  }

  public void TappedSave()
  {
    if (!IsFileReady())
    {
      return;
    }
    byte[] imageBytes = File.ReadAllBytes(localPath);
    PutImage(imageBytes);
  }

  private void PutImage(byte[] imageBytes)
  {
    string filename = Path.GetFileName(localPath);
    label.text = "Put " + filename;
    StartCoroutine(blobService.PutImageBlob(PutImageCompleted, imageBytes, container, filename, "image/png"));
  }

  private void PutImageCompleted(RestResponse response)
  {
    if (response.IsError)
    {
      Log.Text(label, response.ErrorMessage, "Error putting blob image:" + response.Content, Log.Level.Error);
      return;
    }
    Log.Text(label, response.Url, "Put image blob:" + response.Content);
  }

  public void TappedLoad()
  {
    ChangeImage(new Texture2D(1, 1));
    if (!IsFileReady())
    {
      return;
    }
    string filename = Path.GetFileName(localPath);
    string resourcePath = container + "/" + filename;
    Log.Text(label, "Load: " + resourcePath);
    StartCoroutine(blobService.GetImageBlob(GetImageBlobComplete, resourcePath));
  }

  private void GetImageBlobComplete(IRestResponse<Texture> response)
  {
    if (response.IsError)
    {
      Log.Text(label, "Failed to load image: " + response.StatusCode, response.ErrorMessage, Log.Level.Error);
    }
    else
    {
      Log.Text(label, "Loaded image:" + response.Url);
      Texture texture = response.Data;
      ChangeImage(texture);
    }
  }

  private void DisplayImage()
  {
    byte[] imageBytes = File.ReadAllBytes(localPath);
    Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
    texture.LoadImage(imageBytes);
    ChangeImage(texture);
  }

  private void ChangeImage(Texture2D texture)
  {
    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
    image.GetComponent<Image>().sprite = sprite;
  }

  private void ChangeImage(Texture texture)
  {
    ChangeImage(texture as Texture2D);
  }

  private bool IsFileReady()
  {
    if (string.IsNullOrEmpty(localPath) && !File.Exists(localPath))
    {
      Log.Text(label, "Tap 'Capture screenshot' button", "Capture screenshot first", Log.Level.Warning);
      return false;
    }
    return true;
  }

  public void TappedNext()
  {
    SceneManager.LoadScene("ListScene");
  }
}
